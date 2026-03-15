using System.Collections.Generic;
using UnityEngine;
using TTCS.Core;
using TTCS.Core.Events;
using TTCS.Debugging;
using static TTCS.Debugging.DebugLogger;

namespace TTCS.Combat.Managers
{
    /// <summary>
    /// 🔵 Dev A - Turn Manager
    /// Quản lý thứ tự lượt chiến đấu theo hệ thống CTB (Count Turn Battle)
    /// dựa trên chỉ số SPD.
    ///
    /// Cơ chế:
    ///   - Mỗi entity có một "gauge" bắt đầu từ 0.
    ///   - Khi GetNextActor() được gọi, gauge của tất cả entity tăng theo SPD
    ///     cho đến khi entity đầu tiên đạt >= TURN_THRESHOLD.
    ///   - Sau hành động: gauge của entity_hiện_tại -= actionCost.
    ///   - Entity có SPD cao hơn đến lượt thường xuyên hơn.
    ///
    /// Usage:
    ///   TurnManager.Instance.RegisterEntity("char_warrior", 120);
    ///   string actorId = TurnManager.Instance.GetNextActor();
    ///   TurnManager.Instance.StartTurn(actorId);
    ///   // ... thực hiện hành động ...
    ///   TurnManager.Instance.EndTurn(actorId, Constants.DEFAULT_TURN_COST);
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        // ─── Singleton ────────────────────────────────────────────────
        private static TurnManager _instance;
        public static TurnManager Instance
        {
            get
            {
                if (_instance == null)
                    Debug.LogError("[TurnManager] Instance is null — add TurnManager to the scene!");
                return _instance;
            }
        }

        // ─── Constants ────────────────────────────────────────────────
        /// <summary>Ngưỡng gauge đủ để được đi (đồng bộ với DEFAULT_TURN_COST)</summary>
        private const float TURN_THRESHOLD = 100f;

        // ─── State ────────────────────────────────────────────────────
        /// <summary>Gauge hiện tại của mỗi entity (entityId → gauge value)</summary>
        private readonly Dictionary<string, float> _gauges = new();

        /// <summary>SPD của mỗi entity (entityId → speed)</summary>
        private readonly Dictionary<string, int> _speeds = new();

        /// <summary>Danh sách entity đang active (còn sống trong combat)</summary>
        private readonly List<string> _activeEntities = new();

        /// <summary>Số lượt đã đi kể từ khi combat bắt đầu</summary>
        private int _turnCounter = 0;

        /// <summary>Entity đang trong lượt hiện tại (null nếu không ai)</summary>
        public string CurrentActor { get; private set; }

        // ──────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            DebugLogger.Log("TurnManager initialized.", LogCategory.Combat);
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        #endregion

        // ──────────────────────────────────────────────────────────────
        #region Registration

        /// <summary>
        /// Đăng ký entity vào hệ thống turn (gọi trước khi combat bắt đầu).
        /// </summary>
        /// <param name="entityId">ID duy nhất của entity</param>
        /// <param name="speed">Chỉ số SPD — quyết định tần suất lượt</param>
        public void RegisterEntity(string entityId, int speed)
        {
            if (string.IsNullOrEmpty(entityId))
            {
                DebugLogger.LogWarning("RegisterEntity: entityId rỗng — bỏ qua.", LogCategory.Combat);
                return;
            }

            if (_gauges.ContainsKey(entityId))
            {
                DebugLogger.LogWarning($"RegisterEntity: '{entityId}' đã được đăng ký — ghi đè.", LogCategory.Combat);
            }

            int safeSpeed = Mathf.Max(1, speed); // Tránh chia cho 0 khi tính virtual ticks
            _gauges[entityId] = 0f;
            _speeds[entityId] = safeSpeed;

            if (!_activeEntities.Contains(entityId))
                _activeEntities.Add(entityId);

            DebugLogger.Log($"Registered entity '{entityId}' with SPD={safeSpeed}.", LogCategory.Combat);
        }

        /// <summary>
        /// Xóa entity khỏi timeline (gọi khi entity chết trong combat).
        /// </summary>
        /// <param name="entityId">ID của entity cần xóa</param>
        public void RemoveEntity(string entityId)
        {
            if (!_gauges.ContainsKey(entityId))
            {
                DebugLogger.LogWarning($"RemoveEntity: '{entityId}' không tồn tại trong timeline.", LogCategory.Combat);
                return;
            }

            _gauges.Remove(entityId);
            _speeds.Remove(entityId);
            _activeEntities.Remove(entityId);

            DebugLogger.Log($"Removed entity '{entityId}' from timeline.", LogCategory.Combat);
            EventBus.Instance.Publish(new TimelineUpdatedEvent());
        }

        #endregion

        // ──────────────────────────────────────────────────────────────
        #region Turn Flow

        /// <summary>
        /// Tính toán (virtual tick) và trả về entityId có lượt tiếp theo.
        /// Phương thức này KHÔNG bắt đầu lượt — gọi StartTurn() sau khi nhận được actorId.
        ///
        /// Cơ chế virtual tick:
        ///   1. Tìm entity cần ít "ticks" nhất để đạt TURN_THRESHOLD.
        ///   2. Advance tất cả entity bằng minTicks × Speed của mỗi entity.
        ///   3. Trả về entity vừa đạt ngưỡng.
        /// </summary>
        /// <returns>entityId của actor tiếp theo, hoặc null nếu không có entity nào.</returns>
        public string GetNextActor()
        {
            if (_activeEntities.Count == 0)
            {
                DebugLogger.LogWarning("GetNextActor: Không có entity nào trong timeline!", LogCategory.Combat);
                return null;
            }

            // Tìm entity cần ít "ticks" nhất để đạt ngưỡng
            string nextActorId = null;
            float minTicksNeeded = float.MaxValue;

            foreach (string id in _activeEntities)
            {
                if (!_gauges.ContainsKey(id) || !_speeds.ContainsKey(id))
                    continue;

                float currentGauge = _gauges[id];

                // Nếu đã đạt ngưỡng (edge case từ lượt trước), ticks = 0
                float ticksNeeded = currentGauge >= TURN_THRESHOLD
                    ? 0f
                    : (TURN_THRESHOLD - currentGauge) / _speeds[id];

                if (ticksNeeded < minTicksNeeded)
                {
                    minTicksNeeded = ticksNeeded;
                    nextActorId = id;
                }
            }

            if (nextActorId == null)
                return null;

            // Advance toàn bộ entity theo minTicks
            if (minTicksNeeded > 0f)
            {
                foreach (string id in _activeEntities)
                {
                    if (_gauges.ContainsKey(id) && _speeds.ContainsKey(id))
                        _gauges[id] += _speeds[id] * minTicksNeeded;
                }
            }

            DebugLogger.Log(
                $"GetNextActor → '{nextActorId}' (gauge={_gauges[nextActorId]:F1}, ticks advanced={minTicksNeeded:F2})",
                LogCategory.Combat);

            EventBus.Instance.Publish(new TimelineUpdatedEvent());
            return nextActorId;
        }

        /// <summary>
        /// Bắt đầu lượt của entity — tăng turnCounter và phát TurnStartedEvent.
        /// </summary>
        /// <param name="entityId">ID của entity bắt đầu lượt</param>
        public void StartTurn(string entityId)
        {
            if (!_gauges.ContainsKey(entityId))
            {
                DebugLogger.LogWarning($"StartTurn: '{entityId}' không tồn tại trong timeline!", LogCategory.Combat);
                return;
            }

            _turnCounter++;
            CurrentActor = entityId;

            DebugLogger.Log($"Turn {_turnCounter} started — Actor: {entityId}", LogCategory.Combat);
            EventBus.Instance.Publish(new TurnStartedEvent(entityId, _turnCounter));
        }

        /// <summary>
        /// Kết thúc lượt của entity — trừ gauge theo actionCost và phát TurnEndedEvent.
        /// </summary>
        /// <param name="entityId">ID của entity kết thúc lượt</param>
        /// <param name="actionCost">Chi phí hành động vừa thực hiện (mặc định = Constants.DEFAULT_TURN_COST)</param>
        public void EndTurn(string entityId, int actionCost = -1)
        {
            if (!_gauges.ContainsKey(entityId))
            {
                DebugLogger.LogWarning($"EndTurn: '{entityId}' không tồn tại trong timeline!", LogCategory.Combat);
                return;
            }

            if (actionCost < 0)
                actionCost = Constants.DEFAULT_TURN_COST;

            // Trừ gauge sau hành động — entity cần fill lại để đến lượt tiếp
            _gauges[entityId] -= actionCost;

            CurrentActor = null;

            DebugLogger.Log(
                $"Turn {_turnCounter} ended — Actor: {entityId}, cost={actionCost}, gauge remaining={_gauges[entityId]:F1}",
                LogCategory.Combat);

            EventBus.Instance.Publish(new TurnEndedEvent(entityId));
            EventBus.Instance.Publish(new TimelineUpdatedEvent());
        }

        #endregion

        // ──────────────────────────────────────────────────────────────
        #region Preview & Utilities

        /// <summary>
        /// Xem trước N lượt tiếp theo trong timeline (NON-DESTRUCTIVE — không thay đổi state).
        /// Dùng cho UI hiển thị thứ tự lượt.
        /// </summary>
        /// <param name="previewCount">Số lượt muốn xem trước (khuyến nghị: 5–10)</param>
        /// <returns>Danh sách entityId theo thứ tự sẽ đến lượt</returns>
        public List<string> GetTimelinePreview(int previewCount = 5)
        {
            if (_activeEntities.Count == 0)
                return new List<string>();

            // Clone state để không ảnh hưởng state thật
            var simulatedGauges = new Dictionary<string, float>(_gauges);
            var result = new List<string>();

            for (int i = 0; i < previewCount; i++)
            {
                string nextId = null;
                float minTicks = float.MaxValue;

                foreach (string id in _activeEntities)
                {
                    if (!simulatedGauges.ContainsKey(id) || !_speeds.ContainsKey(id))
                        continue;

                    float ticksNeeded = simulatedGauges[id] >= TURN_THRESHOLD
                        ? 0f
                        : (TURN_THRESHOLD - simulatedGauges[id]) / _speeds[id];

                    if (ticksNeeded < minTicks)
                    {
                        minTicks = ticksNeeded;
                        nextId = id;
                    }
                }

                if (nextId == null)
                    break;

                // Advance simulation
                if (minTicks > 0f)
                {
                    foreach (string id in _activeEntities)
                    {
                        if (simulatedGauges.ContainsKey(id) && _speeds.ContainsKey(id))
                            simulatedGauges[id] += _speeds[id] * minTicks;
                    }
                }

                // Deduct cost in simulation
                simulatedGauges[nextId] -= Constants.DEFAULT_TURN_COST;
                result.Add(nextId);
            }

            return result;
        }

        /// <summary>Trả về gauge hiện tại của entity (0–100+)</summary>
        public float GetGauge(string entityId)
        {
            return _gauges.TryGetValue(entityId, out float gauge) ? gauge : 0f;
        }

        /// <summary>
        /// Set gauge trực tiếp cho entity (dùng cho setup lượt mở màn).
        /// </summary>
        public void SetGauge(string entityId, float gauge)
        {
            if (!_gauges.ContainsKey(entityId))
            {
                DebugLogger.LogWarning($"SetGauge: '{entityId}' không tồn tại trong timeline.", LogCategory.Combat);
                return;
            }

            _gauges[entityId] = gauge;
            EventBus.Instance.Publish(new TimelineUpdatedEvent());
        }

        /// <summary>Trả về số lượt đã diễn ra từ đầu combat</summary>
        public int TurnCounter => _turnCounter;

        /// <summary>Trả về danh sách entity đang active (readonly)</summary>
        public IReadOnlyList<string> ActiveEntities => _activeEntities.AsReadOnly();

        #endregion

        // ──────────────────────────────────────────────────────────────
        #region Combat Reset

        /// <summary>
        /// Reset toàn bộ state combat (gọi khi combat kết thúc hoặc scene change).
        /// </summary>
        public void ResetCombat()
        {
            _gauges.Clear();
            _speeds.Clear();
            _activeEntities.Clear();
            _turnCounter = 0;
            CurrentActor = null;

            DebugLogger.Log("Combat reset — timeline cleared.", LogCategory.Combat);
        }

        /// <summary>
        /// Tiện ích: Đăng ký nhiều entity cùng lúc rồi trả về TurnManager để chaining.
        /// </summary>
        /// <param name="entities">List của (entityId, speed) tuples</param>
        public void InitializeCombat(List<(string entityId, int speed)> entities)
        {
            ResetCombat();
            foreach (var (id, spd) in entities)
                RegisterEntity(id, spd);

            DebugLogger.Log($"Combat initialized with {entities.Count} entities.", LogCategory.Combat);
        }

        #endregion
    }
}

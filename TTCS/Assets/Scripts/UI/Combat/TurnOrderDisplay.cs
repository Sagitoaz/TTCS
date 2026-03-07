using System.Collections.Generic;
using UnityEngine;
using TTCS.Combat.Managers;
using TTCS.Core.Events;
using TTCS.Debugging;
using static TTCS.Debugging.DebugLogger;

namespace TTCS.UI.Combat
{
    /// <summary>
    /// 🔵 Dev A - Turn Order Display
    /// Hiển thị hàng đợi lượt (tối đa 8 slot) dựa trên TurnManager timeline preview.
    ///
    /// Subscribe: TimelineUpdatedEvent → rebuild slot list.
    /// </summary>
    public class TurnOrderDisplay : MonoBehaviour
    {
        // ─── Inspector ────────────────────────────────────────────────────
        [SerializeField] private TurnOrderSlot _slotPrefab;
        [SerializeField] private Transform     _slotContainer;
        [SerializeField] private int           _previewCount = 5;

        // ─── Pool ─────────────────────────────────────────────────────────
        private readonly List<TurnOrderSlot> _pool = new();

        // ─── Entity Name Cache ────────────────────────────────────────────
        private readonly Dictionary<string, (string displayName, bool isPlayer)> _entityCache = new();

        // ──────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            BuildPool();
        }

        private void OnEnable()
        {
            EventBus.Instance.Subscribe<TimelineUpdatedEvent>(OnTimelineUpdated);
        }

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<TimelineUpdatedEvent>(OnTimelineUpdated);
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Initialization

        /// <summary>
        /// Đăng ký thông tin entity để TurnOrderDisplay biết tên và phe.
        /// Gọi từ CombatUIController.Initialize() trước khi battle bắt đầu.
        /// </summary>
        public void RegisterEntity(string entityId, string displayName, bool isPlayer)
        {
            _entityCache[entityId] = (displayName, isPlayer);
        }

        private void BuildPool()
        {
            if (_slotPrefab == null)
            {
                LogWarning("TurnOrderDisplay: SlotPrefab chưa được gán!", LogCategory.UI);
                return;
            }

            for (int i = 0; i < _previewCount; i++)
            {
                var slot = Instantiate(_slotPrefab, _slotContainer);
                slot.gameObject.SetActive(false);
                _pool.Add(slot);
            }
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Refresh

        private void OnTimelineUpdated(TimelineUpdatedEvent _)
        {
            Refresh();
        }

        /// <summary>Rebuild toàn bộ turn order display từ TurnManager preview.</summary>
        public void Refresh()
        {
            if (TurnManager.Instance == null) return;

            var preview      = TurnManager.Instance.GetTimelinePreview(_previewCount);
            string currentId = TurnManager.Instance.CurrentActor;

            // Ẩn tất cả trước
            foreach (var slot in _pool)
                slot.gameObject.SetActive(false);

            for (int i = 0; i < preview.Count && i < _pool.Count; i++)
            {
                string entityId = preview[i];

                if (!_entityCache.TryGetValue(entityId, out var info))
                    info = (entityId, isPlayer: false); // fallback

                bool isCurrentActor =  i == 0;
                _pool[i].SetData(entityId, info.displayName, info.isPlayer, isCurrentActor);
            }
        }

        #endregion
    }
}

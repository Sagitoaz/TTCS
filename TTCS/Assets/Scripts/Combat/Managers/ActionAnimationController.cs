using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TTCS.Core.Events;
using TTCS.Visual;

namespace TTCS.Combat.Managers
{
    /// <summary>
    /// 🟢 Dev B Sprint 2 - Action Animation Controller
    ///
    /// Lắng nghe EventBus combat events và điều phối animation sequences.
    /// Là cầu nối giữa combat engine (Sprint 1) và visual layer (Sprint 2 Dev B).
    ///
    /// TRÁCH NHIỆM:
    ///   - Map entityId → CharacterView / EnemyView
    ///   - SkillCastEvent    → PlayAttackSequence (lunge + hit + hurt targets)
    ///   - DamageTakenEvent  → flash đỏ on target (DoT / area damage không có attacker anim)
    ///   - EntityDeathEvent  → PlayDeath on entity
    ///   - TurnStartedEvent  → clear highlights, highlight current actor
    ///   - CombatEndedEvent  → PlayVictory cho player team nếu thắng
    ///
    /// PHÂN CÔNG:
    ///   ActionAnimationController (Dev B) → visual animation
    ///   CombatBridge (Dev A)             → UI updates (HP bars, floating text)
    ///   Cả hai subscribe EventBus độc lập, không block nhau.
    ///
    /// SETUP TRONG UNITY:
    ///   1. Đặt ActionAnimationController trên một GameObject trong CombatScene.
    ///   2. CombatSceneManager (Dev A) gọi RegisterCharacterView / RegisterEnemyView sau khi spawn.
    ///      Hoặc: tự đăng ký trong CharacterView.Start() nếu muốn tách coupling.
    /// </summary>
    public class ActionAnimationController : MonoBehaviour
    {
        // ─── Singleton ────────────────────────────────────────────────────
        private static ActionAnimationController _instance;
        public  static ActionAnimationController Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        // ─── View Registry ────────────────────────────────────────────────
        private readonly Dictionary<string, CharacterView> _characterViews = new Dictionary<string, CharacterView>();
        private readonly Dictionary<string, EnemyView>     _enemyViews     = new Dictionary<string, EnemyView>();

        /// <summary>
        /// Đăng ký CharacterView — gọi từ CombatSceneManager sau khi spawn.
        /// entityId phải khớp với CombatEntity.ID.
        /// </summary>
        public void RegisterCharacterView(string entityId, CharacterView view)
        {
            if (string.IsNullOrEmpty(entityId) || view == null) return;
            _characterViews[entityId] = view;
        }

        /// <summary>Đăng ký EnemyView — gọi từ CombatSceneManager</summary>
        public void RegisterEnemyView(string entityId, EnemyView view)
        {
            if (string.IsNullOrEmpty(entityId) || view == null) return;
            _enemyViews[entityId] = view;
        }

        /// <summary>
        /// Lookup view theo entityId — trả về CharacterView (hoặc EnemyView vì kế thừa).
        /// Dùng bởi VFXController để lấy vị trí spawn VFX.
        /// </summary>
        public CharacterView GetViewForEntity(string entityId)
        {
            if (_characterViews.TryGetValue(entityId, out var cv)) return cv;
            if (_enemyViews.TryGetValue(entityId, out var ev))     return ev;
            return null;
        }

        /// <summary>Xóa toàn bộ registry khi combat kết thúc / reset</summary>
        public void ClearRegistry()
        {
            _characterViews.Clear();
            _enemyViews.Clear();
        }

        // ─── EventBus Subscription ────────────────────────────────────────
        private void OnEnable()
        {
            EventBus.Instance.Subscribe<SkillCastEvent>(OnSkillCast);
            EventBus.Instance.Subscribe<DamageTakenEvent>(OnDamageTaken);
            EventBus.Instance.Subscribe<EntityDeathEvent>(OnEntityDeath);
            EventBus.Instance.Subscribe<TurnStartedEvent>(OnTurnStart);
            EventBus.Instance.Subscribe<CombatEndedEvent>(OnCombatEnded);
        }

        private void OnDisable()
        {
            if (EventBus.Instance == null) return;
            EventBus.Instance.Unsubscribe<SkillCastEvent>(OnSkillCast);
            EventBus.Instance.Unsubscribe<DamageTakenEvent>(OnDamageTaken);
            EventBus.Instance.Unsubscribe<EntityDeathEvent>(OnEntityDeath);
            EventBus.Instance.Unsubscribe<TurnStartedEvent>(OnTurnStart);
            EventBus.Instance.Unsubscribe<CombatEndedEvent>(OnCombatEnded);
        }

        // ─── Event Handlers ───────────────────────────────────────────────
        private void OnSkillCast(SkillCastEvent e)
        {
            var attackerView = GetAnyView(e.CasterId);
            if (attackerView == null) return;

            var targetViews = new List<CharacterView>();
            if (e.TargetIds != null)
            {
                foreach (var tid in e.TargetIds)
                {
                    var v = GetAnyView(tid);
                    if (v != null) targetViews.Add(v);
                }
            }

            // Nếu kẻ tấn công là enemy → hiện telegraph trước
            if (_enemyViews.TryGetValue(e.CasterId, out var enemyView))
            {
                // Telegraph đã được trigger bởi CombatFlowController trước khi fire event
                // Ở đây chỉ cần play attack animation
            }

            StartCoroutine(PlayAttackSequence(attackerView, targetViews));
        }

        private void OnDamageTaken(DamageTakenEvent e)
        {
            // Hurt animation chính được xử lý bởi PlayAttackSequence (sync với hit frame).
            // Handler này chỉ xử lý damage "không có attacker" (DoT effects, etc.)
            // Kiểm tra: nếu sourceId là effect (không phải entity) mới play hurt ở đây.
            // Quy ước: effect sourceId bắt đầu bằng "effect_"
            if (!string.IsNullOrEmpty(e.SourceId) && e.SourceId.StartsWith("effect_"))
            {
                var view = GetAnyView(e.TargetId);
                view?.Animator?.PlayHurt();
            }
        }

        private void OnEntityDeath(EntityDeathEvent e)
        {
            var view = GetAnyView(e.EntityId);
            if (view == null) return;

            view.Animator?.PlayDeath();
            view.SetHighlight(false);
        }

        private void OnTurnStart(TurnStartedEvent e)
        {
            // Clear tất cả highlights
            foreach (var v in _characterViews.Values) v.SetHighlight(false);
            foreach (var v in _enemyViews.Values)     v.SetHighlight(false);

            // Highlight current actor — FEH vàng nhạt
            var current = GetAnyView(e.EntityId);
            current?.SetHighlight(true);
        }

        private void OnCombatEnded(CombatEndedEvent e)
        {
            if (e.Victory)
            {
                // Victory animation cho tất cả player characters còn sống
                foreach (var v in _characterViews.Values)
                {
                    if (v != null && v.gameObject.activeSelf)
                        v.Animator?.PlayVictory();
                }
            }
        }

        // ─── Attack Sequence ──────────────────────────────────────────────

        /// <summary>
        /// Coroutine điều phối attack animation:
        ///   1. Attacker PlayAttack (lunge)
        ///   2. Chờ OnAttackHitFrame event (timeout 2.5s)
        ///   3. Targets PlayHurt cùng lúc
        /// </summary>
        private IEnumerator PlayAttackSequence(CharacterView attacker, List<CharacterView> targets)
        {
            bool hitFrameReceived = false;

            // Dùng local method để có thể unsubscribe đúng cách
            void OnHitFrame() => hitFrameReceived = true;

            if (attacker.Animator != null)
                attacker.Animator.OnAttackHitFrame += OnHitFrame;

            attacker.Animator?.PlayAttack();

            // Chờ hit frame — timeout phòng Animator không có event
            const float timeout = 2.5f;
            float elapsed       = 0f;
            while (!hitFrameReceived && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Play hurt trên tất cả targets đồng thời
            foreach (var target in targets)
                target.Animator?.PlayHurt();

            // Cleanup event listener
            if (attacker.Animator != null)
                attacker.Animator.OnAttackHitFrame -= OnHitFrame;
        }

        // ─── Helpers ──────────────────────────────────────────────────────

        /// <summary>Lookup view bất kể character hay enemy</summary>
        private CharacterView GetAnyView(string entityId)
        {
            if (string.IsNullOrEmpty(entityId)) return null;
            if (_characterViews.TryGetValue(entityId, out var cv)) return cv;
            if (_enemyViews.TryGetValue(entityId, out var ev))     return ev;
            return null;
        }
    }
}

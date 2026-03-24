using System.Collections.Generic;
using UnityEngine;
using TTCS.Combat.Entities;
using TTCS.Combat.Timing;
using TTCS.Core.Events;
using TTCS.Debugging;
using TTCS.UI.Combat;
using static TTCS.Debugging.DebugLogger;

namespace TTCS.Combat.Managers
{
    /// <summary>
    /// 🔵 Dev A - Combat Bridge
    /// Lắng nghe EventBus và kết nối combat engine với visual layer.
    ///
    /// Nhiệm vụ:
    ///   - Forward damage/death/skill events → trigger CharacterAnimator (Dev B)
    ///   - Forward turn events → cập nhật UI highlight
    ///   - Quản lý timing window lifecycle với TelegraphVisual
    ///
    /// CombatBridge là "dây nối" một chiều: engine → visual.
    /// Visual layer KHÔNG được gọi ngược vào engine, chỉ gọi vào CombatBridge để notify.
    ///
    /// Setup trong Unity:
    ///   Đặt CombatBridge trên cùng scene với CombatFlowController.
    ///   Developer B gọi RegisterView() để đăng ký CharacterView/EnemyView.
    ///
    /// Integration với Developer B:
    ///   - Dev B gọi CombatBridge.Instance.RegisterView(entityId, animatorInterface)
    ///   - CombatBridge gọi animatorInterface.PlayAttack(), PlayHurt(), PlayDeath()
    /// </summary>
    public class CombatBridge : MonoBehaviour
    {
        // ─── Singleton ────────────────────────────────────────────────────
        private static CombatBridge _instance;
        public  static CombatBridge Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                UnsubscribeEvents();
            }
        }

        // ─── Animator View Registry ───────────────────────────────────────
        /// <summary>
        /// Interface tối thiểu mà CharacterView/EnemyView (Dev B) phải implement.
        /// Cho phép CombatBridge trigger animations mà không import namespace Visual.
        /// </summary>
        public interface ICharacterAnimatorBridge
        {
            void PlayAttack();
            void PlayHurt();
            void PlayDeath();
            void PlayVictory();
            Transform GetWorldTransform();
        }

        private readonly Dictionary<string, ICharacterAnimatorBridge> _views = new();

        // ─── Telegraph Callback ───────────────────────────────────────────
        /// <summary>
        /// Delegate dùng bởi Dev B TelegraphVisual để notify khi telegraph kết thúc.
        /// CombatBridge dùng để mở TimingWindow sau telegraph.
        /// </summary>
        public delegate void TelegraphCompleteHandler(string enemyId, float attackDuration);

        // ──────────────────────────────────────────────────────────────────
        #region Registration (Dev B calls these)

        /// <summary>
        /// Đăng ký view của entity. Gọi từ CharacterView/EnemyView (Developer B) khi spawn.
        /// </summary>
        public void RegisterView(string entityId, ICharacterAnimatorBridge view)
        {
            if (string.IsNullOrEmpty(entityId) || view == null) return;
            _views[entityId] = view;

            // Đăng ký position với CombatUIController
            CombatUIController.Instance?.RegisterEntityPosition(entityId, view.GetWorldTransform());
            Log($"CombatBridge: Registered view for '{entityId}'.", LogCategory.Combat);
        }

        /// <summary>Gỡ đăng ký view khi entity bị destroy.</summary>
        public void UnregisterView(string entityId)
        {
            _views.Remove(entityId);
        }

        /// <summary>
        /// Dev B gọi để notify telegraph kết thúc → CombatBridge mở TimingWindow.
        /// </summary>
        /// <param name="enemyId">ID của enemy đang telegraph</param>
        /// <param name="windowDuration">Thời lượng timing window (giây)</param>
        public void NotifyTelegraphComplete(string enemyId, float windowDuration = 1.5f)
        {
            Log($"CombatBridge: Telegraph complete for '{enemyId}' — opening timing window.", LogCategory.Combat);

            if (TimingSystem.Instance != null)
            {
                var window = TimingWindow.CreateDefault(Time.time, windowDuration);
                TimingSystem.Instance.OpenWindow(window);
            }
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Event Subscription

        private void OnEnable()  => SubscribeEvents();
        private void OnDisable() => UnsubscribeEvents();

        private void SubscribeEvents()
        {
            var bus = EventBus.Instance;
            if (bus == null) return;

            // DamageTakenEvent: handled by ActionAnimationController (DO NOT subscribe here!)
            // SkillCastEvent: trigger attacker animation (PlayAttack)
            bus.Subscribe<SkillCastEvent>(OnSkillCast);
            bus.Subscribe<EntityDeathEvent>(OnEntityDeath);
            bus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            bus.Subscribe<CombatEndedEvent>(OnCombatEnded);
        }

        private void UnsubscribeEvents()
        {
            var bus = EventBus.Instance;
            if (bus == null) return;

            // DamageTakenEvent: handled by ActionAnimationController (DO NOT subscribe here!)
            bus.Unsubscribe<SkillCastEvent>(OnSkillCast);
            bus.Unsubscribe<EntityDeathEvent>(OnEntityDeath);
            bus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            bus.Unsubscribe<CombatEndedEvent>(OnCombatEnded);
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Event Handlers

        private void OnDamageTaken(DamageTakenEvent e)
        {
            // ❌ REMOVED: PlayHurt đã được xử lý bởi ActionAnimationController
            // - Normal skill damage: PlayAttackSequence() gọi target.PlayHurt()
            // - DoT effects: ActionAnimationController.OnDamageTaken() gọi PlayHurt()
            // Nếu gọi ở đây sẽ trigger 2 lần!
        }

        private void OnEntityDeath(EntityDeathEvent e)
        {
            if (_views.TryGetValue(e.EntityId, out var view))
                view.PlayDeath();
        }

        private void OnSkillCast(SkillCastEvent e)
        {
            // ActionAnimationController chịu trách nhiệm chính cho skill cast visuals.
            // Bridge chỉ fallback khi controller này không tồn tại trong scene.
            if (ActionAnimationController.Instance != null) return;

            if (_views.TryGetValue(e.CasterId, out var casterView))
                casterView.PlayAttack();
        }

        private void OnTurnStarted(TurnStartedEvent e)
        {
            // Có thể dùng để highlight entity trong future
            Log($"CombatBridge: Turn started for '{e.EntityId}'.", LogCategory.Combat);
        }

        private void OnCombatEnded(CombatEndedEvent e)
        {
            if (!e.Victory || CombatFlowController.Instance == null)
                return;

            // Chỉ trigger victory cho player còn sống.
            // Tránh gọi trên enemy vì có thể cắt ngang death sequence khi vừa bị hạ.
            var players = CombatFlowController.Instance.GetPlayerTeam();
            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if (player == null || player.IsDead) continue;

                if (_views.TryGetValue(player.ID, out var view))
                    view.PlayVictory();
            }
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Utilities

        /// <summary>Trả về view của entity hoặc null.</summary>
        public ICharacterAnimatorBridge GetView(string entityId)
        {
            return _views.TryGetValue(entityId, out var v) ? v : null;
        }

        #endregion
    }
}

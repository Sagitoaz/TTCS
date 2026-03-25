using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TTCS.Core.Data;
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
        private readonly HashSet<string> _runningActionCasters = new HashSet<string>();
        private readonly HashSet<string> _hitFrameReachedCasters = new HashSet<string>();

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
            _runningActionCasters.Clear();
            _hitFrameReachedCasters.Clear();
        }

        /// <summary>
        /// True khi caster đang chạy sequence cast/attack và chưa hoàn tất return animation.
        /// CombatFlowController dùng để đồng bộ EndTurn.
        /// </summary>
        public bool IsActionAnimationRunningFor(string entityId)
        {
            if (string.IsNullOrEmpty(entityId)) return false;
            return _runningActionCasters.Contains(entityId);
        }

        /// <summary>
        /// True nếu action hiện tại của caster đã nhận NotifyAttackHitFrame từ animation.
        /// </summary>
        public bool HasHitFrameTriggeredFor(string entityId)
        {
            if (string.IsNullOrEmpty(entityId)) return false;
            return _hitFrameReachedCasters.Contains(entityId);
        }

        /// <summary>Kiểm tra entity có view visual được register trong scene hay không.</summary>
        public bool HasViewForEntity(string entityId)
        {
            if (string.IsNullOrEmpty(entityId)) return false;
            return _characterViews.ContainsKey(entityId) || _enemyViews.ContainsKey(entityId);
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
            _runningActionCasters.Clear();
            _hitFrameReachedCasters.Clear();
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

            var skill = DataManager.Instance?.LoadSkill(e.SkillId);
            string skillType = skill?.type ?? "attack";
            bool isSupportSkill = skillType == "heal" || skillType == "buff";
            bool isAttackSkill = skillType == "attack" || skillType == "debuff";
            bool isMeleeAttack = isAttackSkill && ShouldUseMeleeMovement(skill, targetViews.Count);

           

            _runningActionCasters.Add(e.CasterId);
            _hitFrameReachedCasters.Remove(e.CasterId);

            if (isSupportSkill)
                StartCoroutine(RunTrackedActionSequence(e.CasterId, PlaySupportSequence(attackerView, targetViews)));
            else if (isMeleeAttack)
                StartCoroutine(RunTrackedActionSequence(e.CasterId,
                    PlayAttackSequence(attackerView, targetViews, useMeleeMovement: true)));
            else
                StartCoroutine(RunTrackedActionSequence(e.CasterId,
                    PlayAttackSequence(attackerView, targetViews, useMeleeMovement: false)));
        }

        private IEnumerator RunTrackedActionSequence(string casterId, IEnumerator sequence)
        {
            yield return sequence;
            _runningActionCasters.Remove(casterId);
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
        private IEnumerator PlayAttackSequence(CharacterView attacker, List<CharacterView> targets, bool useMeleeMovement)
        {
            bool hitFrameReceived = false;
            bool animationComplete = false;

            // Dùng local method để có thể unsubscribe đúng cách
            void OnHitFrame()
            {
                hitFrameReceived = true;
                if (!string.IsNullOrEmpty(attacker.EntityId))
                    _hitFrameReachedCasters.Add(attacker.EntityId);
            }
            void OnAnimationComplete() => animationComplete = true;

            if (attacker.Animator != null)
            {
                attacker.Animator.OnAttackHitFrame += OnHitFrame;
                attacker.Animator.OnAnimationComplete += OnAnimationComplete;
            }

            if (useMeleeMovement && targets.Count > 0)
                attacker.Animator?.PlayAttackMelee(GetMeleeApproachPosition(targets));
            else
                attacker.Animator?.PlayAttackInPlace();

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

            // Đợi attacker hoàn tất phase quay về vị trí idle trước khi kết thúc sequence.
            const float completeTimeout = 2.5f;
            elapsed = 0f;
            while (!animationComplete && elapsed < completeTimeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Cleanup event listener
            if (attacker.Animator != null)
            {
                attacker.Animator.OnAttackHitFrame -= OnHitFrame;
                attacker.Animator.OnAnimationComplete -= OnAnimationComplete;
            }
        }

        /// <summary>
        /// Support skills (buff/heal): không phát hurt đỏ, thay bằng flash xanh để dễ phân biệt.
        /// </summary>
        private IEnumerator PlaySupportSequence(CharacterView caster, List<CharacterView> targets)
        {
            bool hitFrameReceived = false;
            bool animationComplete = false;

            void OnHitFrame()
            {
                hitFrameReceived = true;
                if (!string.IsNullOrEmpty(caster.EntityId))
                    _hitFrameReachedCasters.Add(caster.EntityId);
            }
            void OnAnimationComplete() => animationComplete = true;

            if (caster.Animator != null)
            {
                caster.Animator.OnAttackHitFrame += OnHitFrame;
                caster.Animator.OnAnimationComplete += OnAnimationComplete;
            }

            caster.Animator?.PlayAttackInPlace();

            const float timeout = 1.2f;
            float elapsed = 0f;
            while (!hitFrameReceived && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            var supportColor = new Color(0.35f, 1f, 0.45f, 1f);
            foreach (var target in targets)
            {
                if (target == null) continue;
                target.SetAllPartsColor(supportColor);
            }

            yield return new WaitForSeconds(0.18f);

            foreach (var target in targets)
            {
                if (target == null) continue;
                target.ResetPartsColor();
            }

            const float completeTimeout = 2.5f;
            elapsed = 0f;
            while (!animationComplete && elapsed < completeTimeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (caster.Animator != null)
            {
                caster.Animator.OnAttackHitFrame -= OnHitFrame;
                caster.Animator.OnAnimationComplete -= OnAnimationComplete;
            }
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

        private static bool ShouldUseMeleeMovement(TTCS.Data.SkillDataModel skill, int targetCount)
        {
            if (targetCount <= 0) return false;

            if (skill == null)
                return true;

            string style = skill.visual?.attackStyle;
            if (!string.IsNullOrEmpty(style))
            {
                style = style.Trim().ToLowerInvariant();
                if (style == "ranged") return false;
                if (style == "melee") return true;
            }

            return true;
        }

        private static Vector3 GetMeleeApproachPosition(List<CharacterView> targets)
        {
            if (targets == null || targets.Count == 0 || targets[0] == null)
                return Vector3.zero;

            if (targets.Count == 1)
                return targets[0].WorldPosition;

            Vector3 sum = Vector3.zero;
            int validCount = 0;
            for (int i = 0; i < targets.Count; i++)
            {
                var t = targets[i];
                if (t == null) continue;
                sum += t.WorldPosition;
                validCount++;
            }

            if (validCount == 0)
                return targets[0].WorldPosition;

            return sum / validCount;
        }
    }
}

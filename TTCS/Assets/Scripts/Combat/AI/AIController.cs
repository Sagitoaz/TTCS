using System.Collections.Generic;
using UnityEngine;
using TTCS.Combat.Managers;
using TTCS.Debugging;
using static TTCS.Debugging.DebugLogger;

namespace TTCS.Combat.AI
{
    /// <summary>
    /// Kết quả quyết định của AI — skill nào sẽ dùng và target nào.
    /// </summary>
    [System.Serializable]
    public struct AIDecision
    {
        /// <summary>Skill ID được chọn (rỗng = pass/skip turn)</summary>
        public string skillId;

        /// <summary>Danh sách entityId là target</summary>
        public List<string> targetIds;

        /// <summary>Lý do chọn quyết định này (cho debug/log)</summary>
        public string reason;

        /// <summary>True nếu AI quyết định được hành động hợp lệ</summary>
        public bool IsValid => !string.IsNullOrEmpty(skillId) && targetIds != null && targetIds.Count > 0;

        public AIDecision(string skill, List<string> targets, string reason)
        {
            skillId = skill;
            targetIds = targets;
            this.reason = reason;
        }

        /// <summary>Quyết định rỗng (không làm gì được)</summary>
        public static AIDecision None => new AIDecision("", new List<string>(), "No valid action found");
    }

    /// <summary>
    /// 🔵 Dev A - AI Controller
    /// Đưa ra quyết định hành động cho enemy theo decision tree từ AIBehavior profile.
    ///
    /// Decision Tree:
    ///   1. IF self.HP &lt; hpThresholdHeal AND healSkillId available → Heal
    ///   2. ELIF any enemy HP &lt; hpThresholdAggressive AND strongAttackSkillId ready → Strong Attack
    ///   3. ELIF any skill in skillPreferences is ready → Use first available priority skill
    ///   4. ELSE → Basic Attack (fallback)
    ///
    /// Usage (gọi khi đến lượt enemy):
    ///   var decision = AIController.DecideAction(
    ///       selfSnapshot,
    ///       allEntities,
    ///       aiBehavior,
    ///       SkillManager.Instance
    ///   );
    ///
    ///   if (decision.IsValid)
    ///   {
    ///       SkillManager.Instance.UseSkill(selfSnapshot.entityId, decision.skillId);
    ///       // Dev B: resolve damage/effects on decision.targetIds
    ///   }
    /// </summary>
    public static class AIController
    {
        /// <summary>
        /// Đưa ra quyết định hành động tốt nhất cho entity AI.
        /// </summary>
        /// <param name="self">Snapshot của entity AI đang đưa ra quyết định</param>
        /// <param name="allEntities">Tất cả entity trong combat (cả 2 phe)</param>
        /// <param name="behavior">Profile AI (ScriptableObject) — null = dùng fallback</param>
        /// <param name="skillManager">SkillManager để kiểm tra CanUseSkill</param>
        /// <returns>AIDecision chứa skillId và targetIds tốt nhất</returns>
        public static AIDecision DecideAction(
            CombatEntitySnapshot self,
            IEnumerable<CombatEntitySnapshot> allEntities,
            AIBehavior behavior,
            SkillManager skillManager)
        {
            if (behavior == null)
            {
                DebugLogger.LogWarning(
                    $"[AIController] '{self.entityId}': AIBehavior là null — dùng basic attack fallback.",
                    LogCategory.AI);
                return FallbackBasicAttack(self, allEntities, basicAttackSkillId: "");
            }

            if (!behavior.IsValid())
            {
                return FallbackBasicAttack(self, allEntities, behavior.basicAttackSkillId);
            }

            // ── Priority 1: Heal khi HP thấp ──────────────────────────
            if (ShouldHeal(self, behavior))
            {
                var healDecision = TryUseSkill(
                    self, behavior.healSkillId,
                    allEntities, behavior,
                    skillManager, "low HP → Heal");

                if (healDecision.IsValid) return healDecision;
            }

            // ── Priority 2: Strong Attack khi enemy HP thấp ───────────
            if (HasWeakEnemy(self, allEntities, behavior) && !string.IsNullOrEmpty(behavior.strongAttackSkillId))
            {
                var strongDecision = TryUseSkill(
                    self, behavior.strongAttackSkillId,
                    allEntities, behavior,
                    skillManager, "enemy low HP → Strong Attack");

                if (strongDecision.IsValid) return strongDecision;
            }

            // ── Priority 3: Skill Preferences (thử lần lượt) ─────────
            foreach (string skillId in behavior.skillPreferences)
            {
                if (string.IsNullOrEmpty(skillId)) continue;

                var prefDecision = TryUseSkill(
                    self, skillId,
                    allEntities, behavior,
                    skillManager, $"skill preference: {skillId}");

                if (prefDecision.IsValid) return prefDecision;
            }

            // ── Priority 4: Fallback — Basic Attack ───────────────────
            var basicDecision = TryUseSkill(
                self, behavior.basicAttackSkillId,
                allEntities, behavior,
                skillManager, "fallback: basic attack");

            if (basicDecision.IsValid) return basicDecision;

            // Không có hành động nào khả thi
            DebugLogger.LogWarning(
                $"[AIController] '{self.entityId}': Không tìm được hành động hợp lệ nào!",
                LogCategory.AI);
            return AIDecision.None;
        }

        // ─── Decision Helpers ──────────────────────────────────────────

        /// <summary>Kiểm tra có nên ưu tiên heal không</summary>
        private static bool ShouldHeal(CombatEntitySnapshot self, AIBehavior behavior)
        {
            if (string.IsNullOrEmpty(behavior.healSkillId)) return false;

            // Heal bản thân nếu HP thấp
            if (self.HPPercent < behavior.hpThresholdHeal) return true;

            // Có thể heal ally nếu defensive cao (Sprint 2 extension)
            return false;
        }

        /// <summary>Kiểm tra có enemy nào HP thấp để thực hiện tấn công mạnh không</summary>
        private static bool HasWeakEnemy(
            CombatEntitySnapshot self,
            IEnumerable<CombatEntitySnapshot> allEntities,
            AIBehavior behavior)
        {
            foreach (var entity in allEntities)
            {
                // Enemy = ngược phe với caster
                if (entity.isAlly != self.isAlly && entity.IsAlive
                    && entity.HPPercent < behavior.hpThresholdAggressive)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Thử dùng skill với target được chọn tự động.
        /// Trả về AIDecision hợp lệ nếu skill khả dụng, Empty nếu không.
        /// </summary>
        private static AIDecision TryUseSkill(
            CombatEntitySnapshot self,
            string skillId,
            IEnumerable<CombatEntitySnapshot> allEntities,
            AIBehavior behavior,
            SkillManager skillManager,
            string reason)
        {
            if (string.IsNullOrEmpty(skillId)) return AIDecision.None;

            // Kiểm tra skill khả dụng
            if (skillManager != null && !skillManager.CanUseSkill(self.entityId, skillId))
                return AIDecision.None;

            // Lấy skill data để biết target rule
            var skillData = Core.Data.DataManager.Instance?.LoadSkill(skillId);
            string targetRuleType = skillData?.targetRule?.type ?? behavior.preferredTargetRule;
            int targetCount = skillData?.targetRule?.count ?? 1;

            // Chọn targets
            var targets = TargetSelector.SelectTargets(
                self.entityId, targetRuleType, targetCount, allEntities);

            if (targets == null || targets.Count == 0)
            {
                DebugLogger.LogWarning(
                    $"[AIController] TryUseSkill '{skillId}': Không tìm được target hợp lệ.",
                    LogCategory.AI);
                return AIDecision.None;
            }

            DebugLogger.Log(
                $"[AIController] '{self.entityId}' decided: {skillId} → [{string.Join(", ", targets)}] | Reason: {reason}",
                LogCategory.AI);

            return new AIDecision(skillId, targets, reason);
        }

        /// <summary>Fallback khi AIBehavior null hoặc không hợp lệ</summary>
        private static AIDecision FallbackBasicAttack(
            CombatEntitySnapshot self,
            IEnumerable<CombatEntitySnapshot> allEntities,
            string basicAttackSkillId)
        {
            if (string.IsNullOrEmpty(basicAttackSkillId)) return AIDecision.None;

            var targets = TargetSelector.SelectTargets(
                self.entityId, "single_enemy", 1, allEntities);

            if (targets.Count == 0) return AIDecision.None;

            return new AIDecision(basicAttackSkillId, targets, "fallback basic attack");
        }

        // ─── Utility ──────────────────────────────────────────────────

        /// <summary>
        /// Tạo snapshot từ raw data (tiện ích để test không cần CombatEntity thật).
        /// Dev B sẽ tạo method chính thức trên CombatEntity.
        /// </summary>
        public static CombatEntitySnapshot CreateSnapshot(
            string entityId, int currentHP, int maxHP, bool isAlly)
        {
            return new CombatEntitySnapshot(entityId, currentHP, maxHP, isAlly);
        }
    }
}

using System.Collections.Generic;
using TTCS.Combat.Entities;
using TTCS.Combat.Managers;
using TTCS.Data;
using TTCS.Debugging;
using static TTCS.Debugging.DebugLogger;

namespace TTCS.Combat.Actions
{
    /// <summary>
    /// 🟢 Dev B - Action Validator
    /// Static helper — kiểm tra tính hợp lệ của một action trước khi thực thi.
    ///
    /// Gọi từ IAction.Validate() (SkillAction delegate vào đây).
    /// Không sửa state — chỉ kiểm tra và trả về kết quả.
    ///
    /// Các điều kiện kiểm tra:
    ///   1. Actor tồn tại và còn sống
    ///   2. Skill data không null
    ///   3. SkillManager.CanUseSkill (cooldown + mana + limit)
    ///   4. Danh sách target hợp lệ (không rỗng, target còn sống cho attack)
    /// </summary>
    public static class ActionValidator
    {
        /// <summary>
        /// Validate đầy đủ một skill action.
        /// </summary>
        public static ActionValidationResult ValidateSkillAction(
            CombatEntity   actor,
            SkillDataModel skill,
            List<CombatEntity> targets,
            SkillManager   skillManager)
        {
            // ── 1. Actor check ────────────────────────────────────────────
            if (actor == null)
                return ActionValidationResult.Fail("Actor is null");

            if (actor.IsDead)
                return ActionValidationResult.Fail($"Actor '{actor.ID}' is dead");

            // ── 2. Skill data check ───────────────────────────────────────
            if (skill == null)
                return ActionValidationResult.Fail("SkillData is null");

            // ── 3. Resource / cooldown check (via SkillManager) ──────────
            if (skillManager != null && !skillManager.CanUseSkill(actor.ID, skill))
            {
                Log($"ActionValidator: '{actor.ID}' cannot use skill '{skill.id}' (cooldown/mana/limit)",
                    LogCategory.Combat);
                return ActionValidationResult.Fail($"Skill '{skill.id}' unavailable (cooldown/mana/limit)");
            }

            // ── 4. Target check ───────────────────────────────────────────
            if (targets == null || targets.Count == 0)
                return ActionValidationResult.Fail("No targets provided");

            string targetRule = skill.targetRule?.type ?? "single_enemy";

            // Nếu skill nhắm vào enemy, tất cả target phải còn sống
            bool isHostileSkill = (skill.type == "attack" || skill.type == "debuff");
            if (isHostileSkill)
            {
                foreach (var t in targets)
                {
                    if (t == null)
                        return ActionValidationResult.Fail("Target is null");
                    if (t.IsDead)
                        return ActionValidationResult.Fail($"Target '{t.ID}' is already dead");
                }
            }

            return ActionValidationResult.Success();
        }

        /// <summary>
        /// Validate target chỉ — không check skill/resource.
        /// Dùng khi cần re-validate target ngay trước Execute.
        /// </summary>
        public static bool AreTargetsValid(List<CombatEntity> targets, bool requireAlive = true)
        {
            if (targets == null || targets.Count == 0) return false;

            foreach (var t in targets)
            {
                if (t == null) return false;
                if (requireAlive && t.IsDead) return false;
            }
            return true;
        }
    }
}

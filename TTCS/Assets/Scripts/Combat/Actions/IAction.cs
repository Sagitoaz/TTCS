using System.Collections.Generic;
using TTCS.Combat.Entities;
using TTCS.Combat.Managers;
using TTCS.Data;

namespace TTCS.Combat.Actions
{
    /// <summary>
    /// 🟢 Dev B - IAction Interface
    /// Interface bắt buộc cho tất cả loại action trong combat.
    ///
    /// Action pipeline:
    ///   1. Validate  — kiểm tra thực thi được không
    ///   2. Execute   — commit cost + resolve outcome
    ///
    /// Usage (trong CombatFlowController):
    ///   var result = ActionValidator.Validate(action, actor, targets, skillManager);
    ///   if (result.IsValid) action.Execute(actor, targets, skillManager);
    /// </summary>
    public interface IAction
    {
        /// <summary>ID của action (thường = SkillId)</summary>
        string ActionId { get; }

        /// <summary>
        /// Timeline cost (SPD units) — dùng bởi TurnManager sau khi action hoàn thành.
        /// Giá trị chuẩn: Fast=80, Normal=100, Slow=120
        /// </summary>
        int TimelineCost { get; }

        /// <summary>
        /// Kiểm tra action có hợp lệ không (không commit cost hay tác động gì).
        /// </summary>
        ActionValidationResult Validate(CombatEntity actor, List<CombatEntity> targets,
            SkillManager skillManager);

        /// <summary>
        /// Thực thi action: commit cost (mana, cooldown) + resolve damage/effects.
        /// Chỉ gọi sau khi Validate trả về IsValid = true.
        /// </summary>
        void Execute(CombatEntity actor, List<CombatEntity> targets,
            SkillManager skillManager);
    }

    /// <summary>Kết quả validate action</summary>
    public struct ActionValidationResult
    {
        public bool   IsValid;
        public string FailReason;

        public static ActionValidationResult Success() =>
            new ActionValidationResult { IsValid = true };

        public static ActionValidationResult Fail(string reason) =>
            new ActionValidationResult { IsValid = false, FailReason = reason };
    }
}

using System.Collections.Generic;
using TTCS.Combat.Entities;
using TTCS.Combat.Managers;
using TTCS.Data;
using TTCS.Debugging;
using TTCS.Core.Events;
using static TTCS.Debugging.DebugLogger;
using TTCS.Combat.Timing;  // TimingSystem, TimingWindow
using TTCS.UI.Combat;    
namespace TTCS.Combat.Actions
{
    /// <summary>
    /// 🟢 Dev B - Skill Action
    /// Concrete IAction dùng để thực thi kỹ năng trong combat.
    ///
    /// Pipeline Execute:
    ///   1. SkillManager.UseSkill → commit mana và cooldown
    ///   2. ActionResolver.Resolve → damage / heal / effects trên tất cả target
    ///
    /// Khởi tạo:
    ///   var action = new SkillAction(skillDataModel);
    ///   var result = action.Validate(actor, targets, skillManager);
    ///   if (result.IsValid) action.Execute(actor, targets, skillManager);
    /// </summary>
    public class SkillAction : IAction
    {
        // ─── Data ─────────────────────────────────────────────────────────
        private readonly SkillDataModel _skillData;

        // ─── IAction Implementation ───────────────────────────────────────
        public string ActionId     => _skillData?.id ?? "unknown";
        public int    TimelineCost { get; private set; }

        // ─── Constructor ─────────────────────────────────────────────────
        /// <param name="skillData">Model data của skill (loaded từ DataManager)</param>
        public SkillAction(SkillDataModel skillData)
        {
            _skillData = skillData;

            // Parse timeline cost từ actionCost field (mặc định 100 = Normal)
            TimelineCost = skillData?.actionCost?.timelineUnits > 0
                ? skillData.actionCost.timelineUnits
                : 100;
        }

        // ─── Validate ─────────────────────────────────────────────────────
        /// <summary>
        /// Kiểm tra action hợp lệ: actor alive, resource OK, targets valid.
        /// Không thay đổi bất kỳ state nào.
        /// </summary>
        public ActionValidationResult Validate(
            CombatEntity       actor,
            List<CombatEntity> targets,
            SkillManager       skillManager)
        {
            return ActionValidator.ValidateSkillAction(actor, _skillData, targets, skillManager);
        }

        // ─── Execute ──────────────────────────────────────────────────────
        /// <summary>
        /// Commit cost + resolve skill.
        /// Chỉ gọi sau khi Validate trả về IsValid = true.
        /// </summary>
        public void Execute(
            CombatEntity       actor,
            List<CombatEntity> targets,
            SkillManager       skillManager,
            TimingGrade guard = TimingGrade.Miss)
        {
            if (actor == null || _skillData == null)
            {
                Log("SkillAction.Execute: null actor or skill data!", LogCategory.Combat);
                return;
            }

            // ── Step 1: Commit cost ──────────────────────────────────────
            skillManager?.UseSkill(actor.ID, _skillData);

            // ── Step 2: Trigger cast event (để visual layer play attack animation) ──
            EventBus.Instance.Publish(new SkillCastEvent(actor.ID, _skillData.id, GetTargetIds(targets)));

            // ── Step 3: Resolve outcome ──────────────────────────────────
            ActionResolver.Resolve(actor, targets, _skillData, guard);

            Log($"SkillAction.Execute: '{actor.ID}' → skill='{_skillData.id}' targets={targets?.Count ?? 0}",
                LogCategory.Combat);
        }

        public SkillDataModel GetSkillData() => _skillData;

        private static string[] GetTargetIds(List<CombatEntity> targets)
        {
            if (targets == null) return new string[0];

            var ids = new string[targets.Count];
            for (int i = 0; i < targets.Count; i++)
                ids[i] = targets[i]?.ID ?? string.Empty;

            return ids;
        }

        public override string ToString() =>
            $"[SkillAction: {ActionId}, cost={TimelineCost}]";
    }
}

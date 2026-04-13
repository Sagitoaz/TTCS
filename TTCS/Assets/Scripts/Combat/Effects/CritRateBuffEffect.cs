using System;
using TTCS.Combat.Entities;
using TTCS.Combat.Stats;

namespace TTCS.Combat.Effects
{
    /// <summary>
    /// Crit rate buff effect: +X% crit rate trong N turns.
    /// Intensity dùng dạng decimal (0.5 = +50%).
    /// </summary>
    public class CritRateBuffEffect : StatusEffect
    {
        private readonly string _modifierSourceId;

        public CritRateBuffEffect(float critBonusPercent = 0.5f, int duration = 3)
        {
            EffectId = "crit_up";
            TickTiming = TickTiming.EndTurn;
            Intensity = critBonusPercent;
            Duration = duration;
            StackCount = 1;
            _modifierSourceId = $"crit_up_{Guid.NewGuid():N}";
        }

        public override void OnApply(CombatEntity target)
        {
            if (target == null)
            {
                return;
            }

            target.Stats.AddModifier(new StatModifier(
                stat: StatType.CritRate,
                modType: ModifierType.Percentage,
                val: Intensity,
                dur: -1,
                src: _modifierSourceId));
        }

        public override void OnTick(CombatEntity target)
        {
            Duration--;
        }

        public override void OnRemove(CombatEntity target)
        {
            target?.Stats.RemoveModifiersBySource(_modifierSourceId);
        }

        public override void OnStack(StatusEffect incoming)
        {
            if (incoming == null)
            {
                return;
            }

            Duration = Math.Max(Duration, incoming.Duration);
            Intensity = Math.Max(Intensity, incoming.Intensity);
        }
    }
}

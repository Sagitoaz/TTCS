using UnityEngine;
using TTCS.Combat.Entities;

namespace TTCS.Combat.Effects
{
    /// <summary>
    /// 🟢 Dev B - Burn Effect
    /// Lửa DoT — gây sát thương cuối lượt. 
    /// Stack rule: Stack Value — mỗi stack nhân đôi intensity tổng.
    /// Vd: 1 stack = 40 dmg, 2 stacks = 60 dmg, 3 stacks = 80 dmg (cộng thêm 20 mỗi stack)
    ///
    /// EffectId : "burn"
    /// TickTiming: EndTurn
    /// </summary>
    public class BurnEffect : StatusEffect
    {
        private const int MAX_STACKS = 3;

        public BurnEffect(float baseIntensity = 40f, int duration = 3)
        {
            EffectId   = "burn";
            TickTiming = TickTiming.EndTurn;
            Intensity  = baseIntensity;
            Duration   = duration;
            StackCount = 1;
        }

        public override void OnApply(CombatEntity target) { }

        public override void OnTick(CombatEntity target)
        {
            if (target == null || target.IsDead) return;

            // Damage tăng theo stacks: base + (stack-1) * base * 0.5
            int damage = Mathf.Max(1, Mathf.RoundToInt(Intensity * (1f + (StackCount - 1) * 0.5f)));
            target.TakeDamage(damage, EffectId);

            Duration--;
        }

        public override void OnRemove(CombatEntity target) { }

        /// <summary>Stack burn — tăng stack value (intensity tổng), refresh duration</summary>
        public override void OnStack(StatusEffect incoming)
        {
            StackCount = Mathf.Min(StackCount + incoming.StackCount, MAX_STACKS);
            Duration   = Mathf.Max(Duration, incoming.Duration);
        }
    }
}

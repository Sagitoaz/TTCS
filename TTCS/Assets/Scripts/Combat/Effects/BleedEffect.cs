using UnityEngine;
using TTCS.Combat.Entities;
using TTCS.Core.Events;

namespace TTCS.Combat.Effects
{
    /// <summary>
    /// 🟢 Dev B - Bleed Effect
    /// Gây sát thương theo tick vào cuối lượt của target.
    ///
    /// Stack rule: Stack Count (max 5) — mỗi stack thêm intensity sát thương.
    ///
    /// EffectId : "bleed"
    /// TickTiming: EndTurn
    /// Intensity : damage per stack per tick
    /// </summary>
    public class BleedEffect : StatusEffect
    {
        private const int MAX_STACKS = 5;

        public BleedEffect(float damagePerStack = 30f, int duration = 3)
        {
            EffectId   = "bleed";
            TickTiming = TickTiming.EndTurn;
            Intensity  = damagePerStack;
            Duration   = duration;
            StackCount = 1;
        }

        public override void OnApply(CombatEntity target)
        {
            // Log nếu cần — visual indicator set ở StackCount
        }

        public override void OnTick(CombatEntity target)
        {
            if (target == null || target.IsDead) return;

            // Damage = intensity * stacks, bỏ qua defense (true damage from bleed)
            int damage = Mathf.Max(1, Mathf.RoundToInt(Intensity * StackCount));
            target.TakeDamage(damage, EffectId);

            Duration--;
        }

        public override void OnRemove(CombatEntity target)
        {
            // Hết bleed — không cần cleanup thêm
        }

        /// <summary>Stack thêm — tăng StackCount, không refresh duration</summary>
        public override void OnStack(StatusEffect incoming)
        {
            StackCount = Mathf.Min(StackCount + incoming.StackCount, MAX_STACKS);
            // duration: lấy max giữa hiện tại và incoming
            Duration   = Mathf.Max(Duration, incoming.Duration);
        }
    }
}

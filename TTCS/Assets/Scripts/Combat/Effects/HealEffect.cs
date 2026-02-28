using UnityEngine;
using TTCS.Combat.Entities;

namespace TTCS.Combat.Effects
{
    /// <summary>
    /// 🟢 Dev B - Heal Effect (HoT — Heal over Time)
    /// Hồi phục HP theo từng tick.
    ///
    /// Stack rule: Refresh duration, không tăng stack.
    ///
    /// EffectId : "heal_regen"
    /// TickTiming: StartTurn
    /// Intensity : lượng HP hồi mỗi tick
    /// </summary>
    public class HealEffect : StatusEffect
    {
        public HealEffect(float healPerTick = 50f, int duration = 3)
        {
            EffectId   = "heal_regen";
            TickTiming = TickTiming.StartTurn;
            Intensity  = healPerTick;
            Duration   = duration;
            StackCount = 1;
        }

        public override void OnApply(CombatEntity target) { }

        public override void OnTick(CombatEntity target)
        {
            if (target == null || target.IsDead) return;

            int healAmt = Mathf.Max(1, Mathf.RoundToInt(Intensity));
            target.Heal(healAmt, EffectId);

            Duration--;
        }

        public override void OnRemove(CombatEntity target) { }

        /// <summary>Refresh (lấy duration cao hơn)</summary>
        public override void OnStack(StatusEffect incoming)
        {
            Duration  = Mathf.Max(Duration, incoming.Duration);
            Intensity = Mathf.Max(Intensity, incoming.Intensity);
        }
    }
}

using TTCS.Combat.Entities;

namespace TTCS.Combat.Effects
{
    /// <summary>
    /// 🟢 Dev B - Stun Effect
    /// Bỏ qua lượt của entity bị stun.
    ///
    /// Stack rule: Refresh duration — không stack thêm.
    ///
    /// EffectId : "stun"
    /// TickTiming: StartTurn (giảm duration vào đầu lượt bị ảnh hưởng)
    ///
    /// CombatFlowController kiểm tra HasEffect("stun") trước khi cho entity action;
    /// nếu có → skip turn, gọi OnTurnStart để tick stun.
    /// </summary>
    public class StunEffect : StatusEffect
    {
        public StunEffect(int duration = 1)
        {
            EffectId   = "stun";
            TickTiming = TickTiming.StartTurn;
            Intensity  = 0f;
            Duration   = duration;
            StackCount = 1;
        }

        public override void OnApply(CombatEntity target)
        {
            // Visual cue sẽ handle ở Sprint 2
        }

        /// <summary>Giảm duration khi tick</summary>
        public override void OnTick(CombatEntity target)
        {
            Duration--;
        }

        public override void OnRemove(CombatEntity target)
        {
            // Entity thoát stun — không cần cleanup thêm
        }

        /// <summary>Refresh duration (không stack count)</summary>
        public override void OnStack(StatusEffect incoming)
        {
            if (incoming.Duration > Duration)
                Duration = incoming.Duration;
        }
    }
}

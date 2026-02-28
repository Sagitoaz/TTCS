using UnityEngine;
using TTCS.Combat.Entities;

namespace TTCS.Combat.Effects
{
    /// <summary>
    /// 🟢 Dev B - Shield Effect
    /// Tạo lớp khiên hấp thụ damage.
    ///
    /// Stack rule: Stack Value — thêm shield amount.
    ///
    /// EffectId : "shield"
    /// TickTiming: Immediate (áp dụng khi OnApply, không tick sát thương)
    /// Intensity : lượng shield mỗi lần apply
    ///
    /// Duration theo turn — khi hết turn thì remove shield còn lại.
    /// </summary>
    public class ShieldEffect : StatusEffect
    {
        public ShieldEffect(float shieldAmount = 100f, int duration = 2)
        {
            EffectId   = "shield";
            TickTiming = TickTiming.EndTurn;  // Giảm duration mỗi cuối lượt
            Intensity  = shieldAmount;
            Duration   = duration;
            StackCount = 1;
        }

        public override void OnApply(CombatEntity target)
        {
            if (target == null) return;
            // Thêm shield vào HealthComponent
            target.Health.AddShield(Mathf.RoundToInt(Intensity));
        }

        public override void OnTick(CombatEntity target)
        {
            // Chỉ đếm ngược duration, không tick damage/heal
            Duration--;
        }

        public override void OnRemove(CombatEntity target)
        {
            // Khi shield effect hết hạn, xóa shield còn lại trong HealthComponent
            target?.Health.ClearShield();
        }

        /// <summary>Stack shield — cộng thêm shield amount</summary>
        public override void OnStack(StatusEffect incoming)
        {
            // Thêm shield amount của incoming vào entity nếu có reference
            // Target reference không lưu ở đây — CombatFlowController / EffectComponent sẽ
            // gọi OnApply cho incoming trước khi gọi OnStack, hoặc gọi OnStack với incoming
            // Để đơn giản, chỉ refresh duration và cộng intensity:
            Intensity += incoming.Intensity;
            Duration   = Mathf.Max(Duration, incoming.Duration);
        }
    }
}

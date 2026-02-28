using System;
using UnityEngine;

namespace TTCS.Combat.Stats
{
    /// <summary>Loại stat có thể bị modify</summary>
    public enum StatType
    {
        HP,
        ATK,
        DEF,
        SPD,
        CritRate,
        Resist
    }

    /// <summary>Cách tính modifier</summary>
    public enum ModifierType
    {
        Flat,       // Cộng thẳng vào stat
        Percentage  // Nhân theo % (0.1 = +10%)
    }

    /// <summary>
    /// 🟢 Dev B - Stat Modifier
    /// Buff / Debuff thay đổi 1 stat trong thời gian giới hạn.
    /// duration = -1 : vĩnh viễn (đến khi manually remove)
    /// duration = 0  : đã hết hạn
    /// duration > 0  : còn N turn
    /// </summary>
    [Serializable]
    public class StatModifier
    {
        [Header("Target")]
        public StatType targetStat;
        public ModifierType type;
        public float value;

        [Header("Duration")]
        [Tooltip("-1 = vĩnh viễn, >0 = số turn còn lại")]
        public int duration;

        [Header("Source")]
        [Tooltip("ID của skill / effect đã tạo ra modifier này")]
        public string sourceId;

        /// <summary>Modifier đã hết hạn chưa (chỉ check nếu không phải permanent)</summary>
        public bool IsExpired => duration == 0;

        /// <summary>Modifier tồn tại vĩnh viễn (đến khi bị remove thủ công)</summary>
        public bool IsPermanent => duration < 0;

        /// <summary>Constructor chính</summary>
        public StatModifier(StatType stat, ModifierType modType, float val, int dur, string src = "")
        {
            targetStat = stat;
            type       = modType;
            value      = val;
            duration   = dur;
            sourceId   = src;
        }

        /// <summary>Giảm duration 1 tick (bỏ qua nếu permanent)</summary>
        public void TickDuration()
        {
            if (!IsPermanent && duration > 0)
                duration--;
        }

        public override string ToString() =>
            $"[Mod] {targetStat} {(type == ModifierType.Flat ? "+" : "x")}{value} ({duration} turns, src={sourceId})";
    }
}

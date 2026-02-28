namespace TTCS.Combat.Stats
{
    /// <summary>
    /// 🟢 Dev B - Damage & Element Enums
    /// Phân loại sát thương và nguyên tố trong combat
    /// </summary>

    /// <summary>Loại sát thương</summary>
    public enum DamageType
    {
        Normal,     // Đòn thường
        Skill,      // Kỹ năng
        Effect,     // Hiệu ứng (DoT, bleed, burn...)
        True        // True damage (bỏ qua defense)
    }

    /// <summary>Nguyên tố — dùng cho elemental multiplier</summary>
    public enum Element
    {
        None,
        Fire,
        Ice,
        Lightning,
        Dark,
        Light,
        Physical
    }

    /// <summary>Hành động ST/LAN theo timeline cost</summary>
    public enum ActionSpeed
    {
        Fast   = 80,
        Normal = 100,
        Slow   = 120
    }
}

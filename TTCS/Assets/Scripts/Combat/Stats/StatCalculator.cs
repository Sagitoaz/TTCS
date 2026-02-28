using UnityEngine;
using TTCS.Combat.Entities;
using TTCS.Core.Utilities;

namespace TTCS.Combat.Stats
{
    /// <summary>
    /// 🟢 Dev B - Stat Calculator
    /// Static utility — tất cả công thức tính sát thương và stat trong game.
    ///
    /// Damage Pipeline (cơ bản — Sprint 1):
    ///   1. baseDmg     = attacker.ATK * skillMultiplier
    ///   2. defFactor   = def / (def + 100)  →  capped 75%
    ///   3. netDamage   = baseDmg * (1 - defFactor)
    ///   4. critBonus   = RNG check → x critDmg multiplier (mặc định 1.5)
    ///   5. elementMult = GetElementMultiplier (stub = 1.0)
    ///   6. finalDamage = Mathf.Max(1, netDamage * critBonus * elementMult)
    ///
    /// Resistance ảnh hưởng lên Effect damage riêng (không phải direct damage).
    /// </summary>
    public static class StatCalculator
    {
        // ─── Constants ────────────────────────────────────────────────────
        /// <summary>Giới hạn trên của defense reduction (%)</summary>
        public const float MAX_DEF_REDUCTION  = 0.75f;

        /// <summary>Damage tối thiểu (luôn gây ít nhất 1 damage)</summary>
        public const int   MIN_DAMAGE         = 1;

        /// <summary>Hệ số nhân critical hit mặc định</summary>
        public const float DEFAULT_CRIT_MULT  = 1.5f;

        // ─── Damage Calculation ──────────────────────────────────────────
        /// <summary>
        /// Tính sát thương từ attacker → defender với skill multiplier.
        /// Trả về (finalDamage, isCrit).
        /// </summary>
        public static (int damage, bool isCrit) CalculateDamage(
            CombatEntity attacker,
            CombatEntity defender,
            float        skillMultiplier = 1f,
            Element      element         = Element.Physical,
            float        critMultiplier  = DEFAULT_CRIT_MULT)
        {
            if (attacker == null || defender == null)
                return (MIN_DAMAGE, false);

            // 1. Base damage
            float baseDmg = attacker.ATK * skillMultiplier;

            // 2. Defense reduction: def / (def + 100), capped 75%
            float defVal      = Mathf.Max(0, defender.DEF);
            float defFactor   = defVal / (defVal + 100f);
            defFactor         = Mathf.Min(MAX_DEF_REDUCTION, defFactor);

            float netDamage   = baseDmg * (1f - defFactor);

            // 3. Crit check
            bool  isCrit      = CheckCrit(attacker.CritRate);
            float critBonus   = isCrit ? critMultiplier : 1f;

            // 4. Element multiplier (stub — Sprint 2 sẽ implement đầy đủ)
            float elementMult = GetElementMultiplier(element, Element.None);

            // 5. Final damage
            int finalDmg = Mathf.Max(MIN_DAMAGE, Mathf.RoundToInt(netDamage * critBonus * elementMult));

            return (finalDmg, isCrit);
        }

        // ─── Healing Calculation ──────────────────────────────────────────
        /// <summary>
        /// Tính lượng heal từ caster với skill heal multiplier.
        /// Heal không bị giảm bởi target's defense.
        /// </summary>
        public static int CalculateHeal(CombatEntity caster, float skillMultiplier = 1f)
        {
            if (caster == null) return 0;

            // Heal base từ ATK * multiplier (game design: ATK cũng ảnh hưởng heal)
            int healAmount = Mathf.Max(1, Mathf.RoundToInt(caster.ATK * skillMultiplier));
            return healAmount;
        }

        // ─── Crit Check ──────────────────────────────────────────────────
        /// <summary>
        /// Kiểm tra có critical hit không dựa trên critRate.
        /// Sử dụng RNGService để đảm bảo deterministic với seeded RNG.
        /// </summary>
        public static bool CheckCrit(float critRate)
        {
            return RNGService.Instance.RollCrit(critRate);
        }

        // ─── Element Multiplier ──────────────────────────────────────────
        /// <summary>
        /// Trả về elemental damage multiplier.
        /// Sprint 1: Stub luôn trả về 1.0. Sprint 2 sẽ implement bảng điểm yếu.
        /// </summary>
        public static float GetElementMultiplier(Element attackElement, Element defenseElement)
        {
            // TODO Sprint 2: implement elemental advantage table
            return 1.0f;
        }

        // ─── Defense Utility ─────────────────────────────────────────────
        /// <summary>
        /// Tính defense reduction % đơn thuần (không gây damage).
        /// Dùng để hiển thị thông tin / debug.
        /// </summary>
        public static float CalculateDefenseReduction(int defense)
        {
            float defVal = Mathf.Max(0, defense);
            return Mathf.Min(MAX_DEF_REDUCTION, defVal / (defVal + 100f));
        }

        /// <summary>Tính effective HP từ entity (HP * defense factor — dùng để AI đánh giá độ bền)</summary>
        public static float CalculateEffectiveHP(CombatEntity entity)
        {
            if (entity == null) return 0f;
            float ehpMultiplier = 1f / (1f - CalculateDefenseReduction(entity.DEF));
            return entity.Health.CurrentHP * ehpMultiplier;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using TTCS.Combat.Stats;

namespace TTCS.Combat.Components
{
    /// <summary>
    /// 🟢 Dev B - Stats Component
    /// Quản lý stats gốc (BaseStats) + danh sách StatModifier đang active.
    /// GetEffectiveStat() trả về giá trị sau khi tính tất cả modifier.
    ///
    /// Thứ tự tính:
    ///   effective = (base + sumFlat) * (1 + sumPercentage)
    ///   Giá trị tối thiểu: 0f (không âm)
    /// </summary>
    public class StatsComponent : IEntityComponent
    {
        // ─── State ────────────────────────────────────────────────────────
        private string              _entityId;
        private EntityStats         _baseStats;
        private List<StatModifier>  _modifiers = new List<StatModifier>();

        // ─── IEntityComponent ─────────────────────────────────────────────
        public string EntityId => _entityId;

        public void Initialize(string entityId)
        {
            _entityId = entityId;
        }

        public void Reset()
        {
            _modifiers.Clear();
        }

        // ─── Setup ───────────────────────────────────────────────────────
        public void SetBaseStats(EntityStats stats)
        {
            _baseStats = stats;
        }

        public EntityStats BaseStats => _baseStats;

        // ─── Effective Stats ─────────────────────────────────────────────
        /// <summary>
        /// Tính effective value của stat sau tất cả modifier.
        /// Trả về float (caller tự cast về int nếu cần).
        /// </summary>
        public float GetEffectiveStat(StatType type)
        {
            float baseVal      = GetBaseStat(type);
            float flatBonus    = 0f;
            float percentBonus = 0f;

            foreach (var mod in _modifiers)
            {
                if (mod.targetStat != type || mod.IsExpired) continue;
                if (mod.type == ModifierType.Flat)
                    flatBonus += mod.value;
                else
                    percentBonus += mod.value;
            }

            float result = (baseVal + flatBonus) * (1f + percentBonus);
            return Mathf.Max(0f, result);
        }

        private float GetBaseStat(StatType type)
        {
            if (_baseStats == null) return 0f;

            return type switch
            {
                StatType.HP       => _baseStats.MaxHP,
                StatType.ATK      => _baseStats.Attack,
                StatType.DEF      => _baseStats.Defense,
                StatType.SPD      => _baseStats.Speed,
                StatType.CritRate => _baseStats.CritRate,
                StatType.Resist   => _baseStats.Resist,
                _                 => 0f
            };
        }

        // ─── Modifier Management ─────────────────────────────────────────
        public void AddModifier(StatModifier mod)
        {
            if (mod != null) _modifiers.Add(mod);
        }

        public void RemoveModifier(StatModifier mod)
        {
            _modifiers.Remove(mod);
        }

        public void RemoveModifiersBySource(string sourceId)
        {
            _modifiers.RemoveAll(m => m.sourceId == sourceId);
        }

        public void ClearModifiers()
        {
            _modifiers.Clear();
        }

        /// <summary>Tick duration tất cả modifier, xóa cái đã hết hạn (không phải permanent)</summary>
        public void TickModifiers()
        {
            foreach (var mod in _modifiers)
                mod.TickDuration();

            _modifiers.RemoveAll(m => !m.IsPermanent && m.IsExpired);
        }

        public List<StatModifier> GetActiveModifiers() => new List<StatModifier>(_modifiers);
    }
}

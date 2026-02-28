using UnityEngine;
using TTCS.Combat.Components;
using TTCS.Core.Events;

namespace TTCS.Combat.Components
{
    /// <summary>
    /// 🟢 Dev B - Health Component
    /// Quản lý HP + Shield của một CombatEntity.
    /// Publish DamageTakenEvent, HealingReceivedEvent, EntityDeathEvent qua EventBus.
    ///
    /// Thứ tự xử lý damage:
    ///   1. Trừ Shield trước
    ///   2. Phần damage còn lại mới trừ HP
    /// </summary>
    public class HealthComponent : IEntityComponent
    {
        // ─── State ────────────────────────────────────────────────────────
        private string _entityId;
        private int    _maxHP;
        private int    _currentHP;
        private int    _shield;

        // ─── IEntityComponent ─────────────────────────────────────────────
        public string EntityId => _entityId;

        public void Initialize(string entityId)
        {
            _entityId = entityId;
        }

        public void Reset()
        {
            _currentHP = _maxHP;
            _shield    = 0;
        }

        // ─── Setup ───────────────────────────────────────────────────────
        /// <summary>Gọi sau Initialize để thiết lập max HP lần đầu</summary>
        public void SetMaxHP(int max)
        {
            _maxHP     = Mathf.Max(1, max);
            _currentHP = _maxHP;
        }

        // ─── Properties ──────────────────────────────────────────────────
        public int   CurrentHP => _currentHP;
        public int   MaxHP     => _maxHP;
        public int   Shield    => _shield;
        public bool  IsDead    => _currentHP <= 0;
        public float HPPercent => _maxHP > 0 ? (float)_currentHP / _maxHP : 0f;

        // ─── Damage ──────────────────────────────────────────────────────
        /// <summary>
        /// Nhận sát thương. Shield hấp thụ trước, sau đó mới giảm HP.
        /// Trả về lượng HP thực sự bị mất.
        /// </summary>
        public int TakeDamage(int rawDamage, string sourceId = "")
        {
            if (rawDamage <= 0 || IsDead) return 0;

            int remaining = rawDamage;

            // Hấp thụ qua shield trước
            if (_shield > 0)
            {
                int absorbed = Mathf.Min(_shield, remaining);
                _shield    -= absorbed;
                remaining  -= absorbed;
            }

            if (remaining <= 0) return 0;

            // Trừ HP
            int actual = Mathf.Min(_currentHP, remaining);
            _currentHP -= actual;

            EventBus.Instance.Publish(new DamageTakenEvent(_entityId, sourceId, actual, false));

            if (IsDead)
                EventBus.Instance.Publish(new EntityDeathEvent(_entityId, sourceId));

            return actual;
        }

        // ─── Heal ────────────────────────────────────────────────────────
        /// <summary>
        /// Hồi HP. Không vượt quá MaxHP. Trả về lượng HP thực sự được hồi.
        /// </summary>
        public int Heal(int amount, string sourceId = "")
        {
            if (amount <= 0 || IsDead) return 0;

            int before    = _currentHP;
            _currentHP    = Mathf.Min(_maxHP, _currentHP + amount);
            int actual    = _currentHP - before;

            if (actual > 0)
                EventBus.Instance.Publish(new HealingReceivedEvent(_entityId, sourceId, actual));

            return actual;
        }

        // ─── Shield ──────────────────────────────────────────────────────
        public void AddShield(int amount)
        {
            if (amount > 0) _shield += amount;
        }

        public void RemoveShield(int amount)
        {
            _shield = Mathf.Max(0, _shield - amount);
        }

        public void ClearShield()
        {
            _shield = 0;
        }
    }
}

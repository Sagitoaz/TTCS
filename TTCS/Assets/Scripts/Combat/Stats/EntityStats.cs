using UnityEngine;

namespace TTCS.Combat.Stats
{
    /// <summary>
    /// 🟢 Dev B - Stats Container
    /// Container chứa các stats cơ bản của combat entity
    /// </summary>
    [System.Serializable]
    public class EntityStats
    {
        [Header("Health")]
        [SerializeField] private int maxHP;
        [SerializeField] private int currentHP;

        [Header("Offense")]
        [SerializeField] private int attack;
        [SerializeField] private float critRate;

        [Header("Defense")]
        [SerializeField] private int defense;
        [SerializeField] private float resist;

        [Header("Speed")]
        [SerializeField] private int speed;

        // Properties
        public int MaxHP => maxHP;
        public int CurrentHP => currentHP;
        public int Attack => attack;
        public float CritRate => critRate;
        public int Defense => defense;
        public float Resist => resist;
        public int Speed => speed;

        public bool IsDead => currentHP <= 0;
        public float HPPercent => maxHP > 0 ? (float)currentHP / maxHP : 0f;

        /// <summary>Constructor</summary>
        public EntityStats(int hp, int atk, int def, int spd, float crit = 0.05f, float res = 0f)
        {
            maxHP = hp;
            currentHP = hp;
            attack = atk;
            defense = def;
            speed = spd;
            critRate = crit;
            resist = res;
        }

        /// <summary>Set HP hiện tại (dùng khi load save hoặc heal)</summary>
        public void SetCurrentHP(int value)
        {
            currentHP = Mathf.Clamp(value, 0, maxHP);
        }

        /// <summary>Modify HP (+ để heal, - để damage)</summary>
        public int ModifyHP(int amount)
        {
            int oldHP = currentHP;
            currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
            return currentHP - oldHP; // Actual change
        }

        /// <summary>Heal một lượng HP</summary>
        public int Heal(int amount)
        {
            return ModifyHP(amount);
        }

        /// <summary>Nhận damage</summary>
        public int TakeDamage(int damage)
        {
            return -ModifyHP(-damage);
        }

        /// <summary>Full heal</summary>
        public void FullHeal()
        {
            currentHP = maxHP;
        }

        /// <summary>Reset stats về giá trị ban đầu</summary>
        public void Reset()
        {
            currentHP = maxHP;
        }

        /// <summary>Clone stats</summary>
        public EntityStats Clone()
        {
            return new EntityStats(maxHP, attack, defense, speed, critRate, resist)
            {
                currentHP = this.currentHP
            };
        }
    }
}

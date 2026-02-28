using System.Collections.Generic;
using TTCS.Combat.AI;
using TTCS.Combat.Components;
using TTCS.Combat.Effects;
using TTCS.Combat.Stats;

namespace TTCS.Combat.Entities
{
    /// <summary>
    /// 🟢 Dev B - Combat Entity (Abstract Base)
    /// 
    /// Base class cho tất cả thực thể tham chiến (Character, Enemy).
    /// Chứa 3 component cốt lõi:
    ///   HealthComponent — HP + Shield management
    ///   StatsComponent  — Base stats + modifier tracking
    ///   EffectComponent — Status effect container
    ///
    /// Lifecycle trong một lượt:
    ///   1. CombatFlowController gọi OnTurnStart() → tick StartTurn effects + stat mods
    ///   2. Entity thực hiện action qua ActionResolver
    ///   3. CombatFlowController gọi OnTurnEnd() → tick EndTurn effects
    ///
    /// Tạo entity qua EntityFactory, không khởi tạo trực tiếp.
    /// </summary>
    public abstract class CombatEntity
    {
        // ─── Identity ─────────────────────────────────────────────────────
        public string ID          { get; protected set; }
        public string DisplayName { get; protected set; }

        /// <summary>True = nhân vật player, False = enemy</summary>
        public bool IsPlayer      { get; protected set; }

        // ─── Components ──────────────────────────────────────────────────
        public HealthComponent Health  { get; private set; }
        public StatsComponent  Stats   { get; private set; }
        public EffectComponent Effects { get; private set; }

        // ─── Convenience Properties ──────────────────────────────────────
        public bool  IsDead    => Health.IsDead;
        public float HPPercent => Health.HPPercent;

        /// <summary>ATK sau modifier</summary>
        public int   ATK       => (int)Stats.GetEffectiveStat(StatType.ATK);
        /// <summary>DEF sau modifier</summary>
        public int   DEF       => (int)Stats.GetEffectiveStat(StatType.DEF);
        /// <summary>SPD sau modifier — dùng bởi TurnManager</summary>
        public int   SPD       => (int)Stats.GetEffectiveStat(StatType.SPD);
        /// <summary>CritRate sau modifier (0.0–1.0)</summary>
        public float CritRate  => Stats.GetEffectiveStat(StatType.CritRate);

        // ─── Initialization ──────────────────────────────────────────────
        /// <summary>
        /// Khởi tạo entity với stats và tên.
        /// Gọi từ subclass constructor hoặc EntityFactory.
        /// </summary>
        protected void Initialize(string id, string displayName, EntityStats stats, bool isPlayer)
        {
            ID          = id;
            DisplayName = displayName;
            IsPlayer    = isPlayer;

            Health  = new HealthComponent();
            Stats   = new StatsComponent();
            Effects = new EffectComponent();

            Health.Initialize(id);
            Stats.Initialize(id);
            Effects.Initialize(id);

            Health.SetMaxHP(stats.MaxHP);
            Stats.SetBaseStats(stats);
        }

        // ─── Combat Actions ──────────────────────────────────────────────
        /// <summary>
        /// Nhận sát thương. Delegate xuống HealthComponent.
        /// sourceId dùng để publish event (thường là attacker ID hoặc effect ID).
        /// </summary>
        public virtual void TakeDamage(int amount, string sourceId = "")
        {
            Health.TakeDamage(amount, sourceId);
        }

        /// <summary>Hồi HP. Delegate xuống HealthComponent.</summary>
        public virtual void Heal(int amount, string sourceId = "")
        {
            Health.Heal(amount, sourceId);
        }

        /// <summary>Áp dụng status effect lên entity này</summary>
        public void ApplyEffect(StatusEffect effect)
        {
            Effects.AddEffect(effect, this);
        }

        /// <summary>Gỡ status effect khỏi entity này</summary>
        public void RemoveEffect(StatusEffect effect)
        {
            Effects.RemoveEffect(effect, this);
        }

        // ─── Turn Hooks ──────────────────────────────────────────────────
        /// <summary>Gọi ở đầu lượt — tick StartTurn effects + stat modifiers</summary>
        public virtual void OnTurnStart()
        {
            Effects.TickEffects(this, TickTiming.StartTurn);
            Stats.TickModifiers();
        }

        /// <summary>Gọi ở cuối lượt — tick EndTurn effects</summary>
        public virtual void OnTurnEnd()
        {
            Effects.TickEffects(this, TickTiming.EndTurn);
        }

        // ─── Battle Reset ────────────────────────────────────────────────
        /// <summary>Reset về trạng thái ban đầu để dùng lại entity (nếu cần)</summary>
        public virtual void ResetForBattle()
        {
            Health.Reset();
            Stats.Reset();
            Effects.Reset();
        }

        // ─── AI Integration ──────────────────────────────────────────────
        /// <summary>
        /// Tạo CombatEntitySnapshot để truyền vào AIController / TargetSelector.
        /// isAlly = !IsPlayer (enemy POV: player-side là enemy của AI)
        /// </summary>
        public CombatEntitySnapshot ToSnapshot()
        {
            return new CombatEntitySnapshot(ID, Health.CurrentHP, Health.MaxHP, !IsPlayer);
        }

        public override string ToString() =>
            $"[{(IsPlayer ? "Player" : "Enemy")}:{ID}] HP={Health.CurrentHP}/{Health.MaxHP}";
    }
}

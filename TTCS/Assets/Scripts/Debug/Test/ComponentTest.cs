using UnityEngine;
using TTCS.Combat.Components;
using TTCS.Combat.Effects;
using TTCS.Combat.Entities;
using TTCS.Combat.Stats;
using TTCS.Data;

public class ComponentTest : MonoBehaviour
{
    void Start()
    {
        // ── Test 1: HealthComponent ──────────────────────────────────
        var hc = new HealthComponent();
        hc.Initialize("test_entity");
        hc.SetMaxHP(1000);
        Debug.Assert(hc.CurrentHP == 1000, "Initial HP should be MaxHP");

        hc.TakeDamage(250);
        Debug.Assert(hc.CurrentHP == 750, "HP after 250 damage");

        hc.Heal(100);
        Debug.Assert(hc.CurrentHP == 850, "HP after 100 heal");

        hc.TakeDamage(9999);
        Debug.Assert(hc.CurrentHP == 0, "HP should not go below 0");
        Debug.Assert(hc.IsDead, "IsDead should be true");

        Debug.Log("✅ HealthComponent tests PASSED");

        // ── Test 2: Shield absorb ────────────────────────────────────
        var hc2 = new HealthComponent();
        hc2.Initialize("shield_test");
        hc2.SetMaxHP(500);
        hc2.AddShield(200);

        hc2.TakeDamage(150);  // Shield absorbs all
        Debug.Assert(hc2.CurrentHP == 500, "HP should not change when shield absorbs");
        Debug.Assert(hc2.Shield == 50, "Shield should be 50 after 150 absorbed");

        hc2.TakeDamage(100);  // 50 shield left + 50 HP
        Debug.Assert(hc2.CurrentHP == 450, "HP should reduce by 50 after shield depleted");

        Debug.Log("✅ Shield absorption tests PASSED");

        // ── Test 3: StatsComponent + Modifier ────────────────────────
        var sc = new StatsComponent();
        sc.Initialize("stat_test");
        var baseStats = new EntityStats(1000, 100, 80, 100);
        sc.SetBaseStats(baseStats);

        float baseAtk = sc.GetEffectiveStat(StatType.ATK);
        Debug.Assert(baseAtk == 100, "Base ATK should be 100");

        // Add +50 flat modifier
        sc.AddModifier(new StatModifier(StatType.ATK, ModifierType.Flat, 50f, 2, "buff_test"));
        float buffedAtk = sc.GetEffectiveStat(StatType.ATK);
        Debug.Assert(buffedAtk == 150, $"Buffed ATK should be 150, got {buffedAtk}");

        Debug.Log("✅ StatsComponent + Modifier tests PASSED");

        // ── Test 4: EffectComponent + Bleed ──────────────────────────
        // Create a simple CombatEntity to test effects
        var model = new EnemyDataModel
        {
            id = "effect_test_enemy",
            nameKey = "TestEnemy",
            baseStats = new EnemyBaseStats { hp = 1000, atk = 50, def = 30, spd = 80 }
        };
        var enemy = EntityFactory.CreateEnemy(model);
        var effect = new BleedEffect(damagePerStack: 60f, duration: 3);

        enemy.ApplyEffect(effect);
        Debug.Assert(enemy.Effects.HasEffect("bleed"), "Enemy should have bleed effect");

        int hpBefore = enemy.Health.CurrentHP;
        enemy.OnTurnEnd(); // Triggers EndTurn effects (bleed ticks)
        int hpAfter = enemy.Health.CurrentHP;
        Debug.Log($"Bleed tick: HP {hpBefore} → {hpAfter} (expected -{60})");
        Debug.Assert(hpBefore - hpAfter == 60, "Bleed should deal 60 damage per tick");

        Debug.Log("✅ EffectComponent + BleedEffect tests PASSED");
        Debug.Log("🎉 ALL COMPONENT TESTS PASSED");
    }
}
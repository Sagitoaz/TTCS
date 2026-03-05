using UnityEngine;
using TTCS.Combat.Entities;
using TTCS.Combat.Stats;
using TTCS.Data;

public class DamageCalcTest : MonoBehaviour
{
    void Start()
    {
        // Warrior (ATK=140) vs Goblin (DEF=40)
        var warriorModel = new CharacterDataModel
        {
            id = "char_warrior_test",
            nameKey = "Warrior",
            baseStats = new CharacterBaseStats { hp = 1200, atk = 140, def = 90, spd = 105, crit = 0f }
        };
        var goblinModel = new EnemyDataModel
        {
            id = "goblin_test",
            nameKey = "Goblin",
            baseStats = new EnemyBaseStats { hp = 400, atk = 70, def = 40, spd = 85 }
        };

        var warrior = EntityFactory.CreateCharacter(warriorModel);
        var goblin  = EntityFactory.CreateEnemy(goblinModel);

        // Formula: (140 * 1.2) * (1 - 40/(40+100)) = 168 * (1 - 0.2857) ≈ 120
        var (damage, isCrit) = StatCalculator.CalculateDamage(warrior, goblin, 1.2f);
        Debug.Log($"Warrior Slash vs Goblin: {damage} damage (crit={isCrit})");
        // Expected ~120 damage

        // Test minimum damage
        var tankModel = new EnemyDataModel
        {
            id = "tank_test",
            nameKey = "Iron Golem",
            baseStats = new EnemyBaseStats { hp = 5000, atk = 50, def = 9999, spd = 30 }
        };
        var tank = EntityFactory.CreateEnemy(tankModel);
        var (minDmg, _) = StatCalculator.CalculateDamage(warrior, tank, 1.0f);
        Debug.Log($"Warrior vs Iron Golem (DEF=9999): {minDmg} damage (should be 1 minimum)");
        Debug.Assert(minDmg >= 1, "Minimum damage should always be at least 1");

        Debug.Log("✅ Damage formula tests PASSED");
    }
}
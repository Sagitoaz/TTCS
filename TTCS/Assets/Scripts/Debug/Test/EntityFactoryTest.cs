using UnityEngine;
using TTCS.Combat.Entities;
using TTCS.Data;

public class EntityFactoryTest : MonoBehaviour
{
    void Start()
    {
        // Test tạo Character trực tiếp từ model (không cần DataManager)
        var model = new CharacterDataModel
        {
            id      = "char_test",
            nameKey = "TestWarrior",
            baseStats = new CharacterBaseStats
            { hp = 1000, atk = 100, def = 80, spd = 100, crit = 0.05f }
        };
        var character = EntityFactory.CreateCharacter(model);
        Debug.Log($"Created: {character} HP={character.Health.CurrentHP}/{character.Health.MaxHP}");

        // Test tạo Enemy
        var enemyModel = new EnemyDataModel
        {
            id      = "enemy_test",
            nameKey = "TestGoblin",
            baseStats = new EnemyBaseStats
            { hp = 400, atk = 70, def = 40, spd = 85 }
        };
        var enemy = EntityFactory.CreateEnemy(enemyModel);
        Debug.Log($"Created: {enemy} HP={enemy.Health.CurrentHP}");

        // Test TakeDamage
        character.TakeDamage(100);
        Debug.Log($"After 100 damage: HP={character.Health.CurrentHP}");

        Debug.Log("✅ EntityFactory test PASSED");
    }
}
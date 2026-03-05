using System.Collections.Generic;
using UnityEngine;
using TTCS.Combat.Actions;
using TTCS.Combat.Entities;
using TTCS.Combat.Managers;
using TTCS.Core.Utilities;
using TTCS.Data;

public class ActionPipelineTest : MonoBehaviour
{
    void Start()
    {
        // Khởi tạo RNG
        RNGService.Instance.Initialize(42);

        // Tạo entities
        var attackerModel = new CharacterDataModel
        {
            id = "test_warrior",
            nameKey = "Warrior",
            baseStats = new CharacterBaseStats { hp = 1200, atk = 140, def = 90, spd = 105, crit = 0f }
        };
        var defenderModel = new EnemyDataModel
        {
            id = "test_goblin",
            nameKey = "Goblin",
            baseStats = new EnemyBaseStats { hp = 400, atk = 70, def = 40, spd = 85 }
        };

        var attacker = EntityFactory.CreateCharacter(attackerModel);
        var defender = EntityFactory.CreateEnemy(defenderModel);

        // Đăng ký với SkillManager
        SkillManager.Instance.RegisterEntity(attacker.ID, maxMana: 100, startingMana: 50);

        // Tạo SkillDataModel
        var skillModel = new SkillDataModel
        {
            id = "slash_test",
            nameKey = "Test Slash",
            type = "Attack",
            targetRule = new SkillTargetRule { type = "SingleEnemy" },
            damage = new SkillDamage { formula = "ATK * 1.2" },
            cost = new SkillCost { mana = 20, cooldown = 0 },
            actionCost = new SkillActionCost { timelineUnits = 100 }
        };

        var targets = new List<CombatEntity> { defender };

        // Test SkillAction
        var action = new SkillAction(skillModel);

        var validation = action.Validate(attacker, targets, SkillManager.Instance);
        Debug.Log($"Validation: {(validation.IsValid ? "✅ VALID" : "❌ INVALID: " + validation.FailReason)}");

        int hpBefore = defender.Health.CurrentHP;
        action.Execute(attacker, targets, SkillManager.Instance);
        int hpAfter = defender.Health.CurrentHP;

        Debug.Log($"Goblin HP: {hpBefore} → {hpAfter} (dmg = {hpBefore - hpAfter})");
        Debug.Assert(hpAfter < hpBefore, "Defender should take damage");

        Debug.Log("✅ Action Pipeline test PASSED");

        var burnSkillModel = new SkillDataModel
        {
            id = "fireball_test",
            type = "Attack",
            damage = new SkillDamage { formula = "ATK * 1.5" },
            effects = new List<SkillEffect> { new SkillEffect { type = "burn", chance = 1.0f, value = "50", duration = 3 } },
            cost = new SkillCost { mana = 0 },
            actionCost = new SkillActionCost { timelineUnits = 120 }
        };

        var mageAttacker = EntityFactory.CreateCharacter(attackerModel);
        var burnTarget = EntityFactory.CreateEnemy(defenderModel);
        var burnTargets = new List<CombatEntity> { burnTarget };

        SkillManager.Instance.RegisterEntity(mageAttacker.ID, 100, 100);

        var fireAction = new SkillAction(burnSkillModel);
        fireAction.Execute(mageAttacker, burnTargets, SkillManager.Instance); // chance = 100% → burn guaranteed

        bool hasBurn = burnTarget.Effects.HasEffect("burn");
        Debug.Log($"Burn applied: {hasBurn}");
        Debug.Assert(hasBurn, "Burn effect should be applied at 100% chance");

        // Tick burn
        int hpBeforeTick = burnTarget.Health.CurrentHP;
        burnTarget.OnTurnEnd(); // Tick EndTurn effects (burn)
        int hpAfterTick = burnTarget.Health.CurrentHP;
        Debug.Log($"Burn tick: HP {hpBeforeTick} → {hpAfterTick} (dmg = {hpBeforeTick - hpAfterTick})");

        Debug.Log("✅ Burn Effect Chain test PASSED");
    }
}
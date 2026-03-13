using System.Collections.Generic;
using UnityEngine;
using TTCS.Combat.AI;
using TTCS.Combat.Managers;

public class AIControllerTest : MonoBehaviour
{
    [Header("AI Behavior to Test")]
    public AIBehavior BanditBehavior;   // Kéo BanditAI asset vào đây

    private void Start()
    {
        var sm = SkillManager.Instance;

        // Setup
        sm.RegisterEntity("enemy_bandit", maxMana: 50, startingMana: 50);
        sm.RegisterEntity("char_warrior", maxMana: 100, startingMana: 80);

        // Tạo snapshot entities
        var allEntities = new List<CombatEntitySnapshot>
        {
            AIController.CreateSnapshot("enemy_bandit", currentHP: 800, maxHP: 1500, isAlly: false),
            AIController.CreateSnapshot("char_warrior", currentHP: 2800, maxHP: 3000, isAlly: true)
        };

        var self = allEntities[0]; // enemy_bandit là AI

        // AI quyết định
        if (BanditBehavior == null)
        {
            Debug.LogWarning("AIControllerTest: Kéo BanditAI ScriptableObject vào Inspector trước!");
            return;
        }

        AIDecision decision = AIController.DecideAction(self, allEntities, BanditBehavior, sm);

        Debug.Log($"AI Decision: skill={decision.skillId}, targets=[{string.Join(", ", decision.targetIds)}], reason={decision.reason}");

        // Bandit HP thấp? Test heal threshold
        var lowHPEntities = new List<CombatEntitySnapshot>
        {
            AIController.CreateSnapshot("enemy_bandit", currentHP: 300, maxHP: 1500, isAlly: false), // 20% HP → should heal
            AIController.CreateSnapshot("char_warrior", currentHP: 1200, maxHP: 3000, isAlly: true)
        };

        AIDecision lowHPDecision = AIController.DecideAction(lowHPEntities[0], lowHPEntities, BanditBehavior, sm);
        Debug.Log($"Low HP Decision: skill={lowHPDecision.skillId} (should attempt heal if configured) | reason={lowHPDecision.reason}");

        sm.ResetCombat();
    }
}
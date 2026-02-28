using UnityEngine;
using TTCS.Combat.Managers;
using TTCS.Combat;

public class SkillManagerTest : MonoBehaviour
{
    private void Start()
    {
        var sm = SkillManager.Instance;

        // Đăng ký 2 entities
        sm.RegisterEntity("char_warrior", maxMana: 100, startingMana: 80);
        sm.RegisterEntity("char_mage", maxMana: 120, startingMana: 100);

        // Test CanUseSkill
        bool canSlash = sm.CanUseSkill("char_warrior", "skill_warrior_slash");
        Debug.Log($"Can warrior use slash? {canSlash}"); // Phải là True

        // Test UseSkill
        bool used = sm.UseSkill("char_warrior", "skill_warrior_slash");
        Debug.Log($"Warrior used slash: {used}"); // True
        Debug.Log($"Warrior mana after: {sm.GetMana("char_warrior")}/{sm.GetMaxMana("char_warrior")}");

        // Test TickCooldowns
        sm.TickCooldowns("char_warrior");
        sm.TickCooldowns("char_mage");

        // Test mana restore
        sm.RestoreMana("char_warrior", 20);
        Debug.Log($"Warrior mana after restore: {sm.GetMana("char_warrior")}");

        // Cleanup
        sm.ResetCombat();
        Debug.Log("SkillManager test complete.");
    }
}
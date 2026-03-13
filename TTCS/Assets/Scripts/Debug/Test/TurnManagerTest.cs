using System.Collections.Generic;
using UnityEngine;
using TTCS.Combat.Managers;
using TTCS.Combat; // CombatLogger

public class TurnManagerTest : MonoBehaviour
{
    private void Start()
    {
        var tm = TurnManager.Instance;

        // 1. Khởi tạo combat với 4 entities
        tm.InitializeCombat(new List<(string, int)>
        {
            ("char_warrior", 120),  // Warrior: SPD 120 — đi trước
            ("char_mage",    95),   // Mage: SPD 95
            ("enemy_bandit", 110),  // Bandit: SPD 110
            ("enemy_henry", 85) // Henry: SPD 85 — chậm nhất
        });

        // 2. Chạy thử 8 lượt
        Debug.Log("=== COMBAT SIMULATION (8 turns) ===");
        for (int i = 0; i < 8; i++)
        {
            string actorId = tm.GetNextActor();
            tm.StartTurn(actorId);

            // Hiển thị timeline preview
            var preview = tm.GetTimelinePreview(3);
            CombatLogger.LogTimeline(tm.TurnCounter, preview);

            // Kết thúc lượt (chi phí mặc định = 100)
            tm.EndTurn(actorId);
        }

        // 3. Reset
        tm.ResetCombat();
        Debug.Log("=== COMBAT END ===");
    }
}
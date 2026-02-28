using System.Collections.Generic;
using System.Linq;
using TTCS.Combat.AI;
using TTCS.Combat.Stats;
using TTCS.Data;

namespace TTCS.Combat.Entities
{
    /// <summary>
    /// 🟢 Dev B - Enemy (AI-Controlled Entity)
    /// Đại diện cho kẻ địch trong combat.
    ///
    /// Khởi tạo thông qua EntityFactory.CreateEnemy(EnemyDataModel).
    /// Giữ tham chiếu đến AIBehavior và danh sách skillIds (từ moveSet của model).
    /// </summary>
    public class Enemy : CombatEntity
    {
        // ─── Data Reference ──────────────────────────────────────────────
        /// <summary>ID của EnemyDataModel gốc</summary>
        public string EnemyTemplateId { get; private set; }

        /// <summary>Danh sách skill ID enemy này có thể dùng (trích từ moveSet)</summary>
        public List<string> SkillIds { get; private set; } = new List<string>();

        /// <summary>
        /// AIBehavior asset quy định hành vi AI của enemy này.
        /// Có thể null — trong trường hợp đó AIController sẽ dùng default behavior.
        /// Gán từ EnemyData SO hoặc set sau khi tạo.
        /// </summary>
        public AIBehavior Behavior { get; set; }

        // ─── Rewards ─────────────────────────────────────────────────────
        public int RewardGold { get; private set; }
        public int RewardXP   { get; private set; }

        // ─── Factory Constructor ─────────────────────────────────────────
        /// <summary>
        /// Khởi tạo Enemy từ EnemyDataModel (loaded từ JSON bởi DataManager).
        /// spawnIndex dùng để tạo unique ID khi spawn nhiều cùng loại.
        /// </summary>
        public Enemy(EnemyDataModel model, int spawnIndex = 0)
        {
            EnemyTemplateId = model.id;

            string combatId = spawnIndex > 0 ? $"{model.id}_{spawnIndex}" : model.id;

            var stats = new EntityStats(
                hp:   model.baseStats?.hp  ?? 500,
                atk:  model.baseStats?.atk ?? 80,
                def:  model.baseStats?.def ?? 50,
                spd:  model.baseStats?.spd ?? 90,
                crit: model.baseStats?.crit ?? 0.05f,
                res:  model.baseStats?.resist ?? 0f
            );

            Initialize(combatId, model.nameKey ?? model.id, stats, isPlayer: false);

            // Trích danh sách skillId từ moveSet
            if (model.moveSet != null)
                SkillIds.AddRange(model.moveSet.Select(m => m.skillId).Where(s => s != null));

            // Rewards (EnemyRewards uses goldBase / expBase)
            RewardGold = model.rewards?.goldBase ?? 10;
            RewardXP   = model.rewards?.expBase  ?? 20;
        }
    }
}

using System.Collections.Generic;
using TTCS.Combat.Stats;
using TTCS.Data;

namespace TTCS.Combat.Entities
{
    /// <summary>
    /// 🟢 Dev B - Character (Player-Side Entity)
    /// Đại diện cho nhân vật do người chơi điều khiển trong combat.
    ///
    /// Khởi tạo thông qua EntityFactory.CreateCharacter(CharacterDataModel).
    /// Giữ danh sách skillIds để SkillManager tra cứu khi dùng kỹ năng.
    /// </summary>
    public class Character : CombatEntity
    {
        // ─── Data Reference ──────────────────────────────────────────────
        /// <summary>ID của CharacterDataModel gốc (để tra cứu DataManager)</summary>
        public string CharacterId { get; private set; }

        /// <summary>Danh sách skill ID nhân vật này sở hữu</summary>
        public List<string> SkillIds { get; private set; } = new List<string>();

        // ─── Factory Constructor ─────────────────────────────────────────
        /// <summary>
        /// Khởi tạo Character từ CharacterDataModel (loaded từ JSON bởi DataManager).
        /// Tạo EntityStats từ baseStats của model.
        /// </summary>
        public Character(CharacterDataModel model, int instanceIndex = 0)
        {
            CharacterId = model.id;

            // Tạo unique combat ID (model id + index để tránh duplicate khi có 2 cùng char)
            string combatId = instanceIndex > 0 ? $"{model.id}_{instanceIndex}" : model.id;

            var stats = new EntityStats(
                hp:   model.baseStats?.hp  ?? 1000,
                atk:  model.baseStats?.atk ?? 100,
                def:  model.baseStats?.def ?? 80,
                spd:  model.baseStats?.spd ?? 100,
                crit: model.baseStats?.crit ?? 0.05f,
                res:  model.baseStats?.resist ?? 0f
            );

            Initialize(combatId, model.nameKey ?? model.id, stats, isPlayer: true);

            // Copy skill list
            if (model.skills != null)
                SkillIds.AddRange(model.skills);
        }

        /// <summary>Kiểm tra nhân vật có sở hữu skill không</summary>
        public bool HasSkill(string skillId) => SkillIds.Contains(skillId);
    }
}

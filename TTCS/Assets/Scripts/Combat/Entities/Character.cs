using System.Collections.Generic;
using TTCS.Core.Data;
using TTCS.Core.Save;
using TTCS.Combat.Stats;
using TTCS.Data;
using TTCS.Meta.Inventory;

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

            var savedLevel = SaveManager.Instance?.CurrentSave?.GetCharacterLevel(CharacterId) ?? 0;
            var baseLevel = model.baseStats?.level ?? 1;
            var level = System.Math.Max(1, savedLevel > 0 ? savedLevel : baseLevel);

            var hp = model.baseStats?.hp ?? 1000;
            var atk = model.baseStats?.atk ?? 100;
            var def = model.baseStats?.def ?? 80;
            var spd = model.baseStats?.spd ?? 100;
            var crit = model.baseStats?.crit ?? 0.05f;
            var res = model.baseStats?.resist ?? 0f;

            // Scale stats theo level (giống logic progression UI).
            hp += (model.growthCurve?.hpPerLevel ?? 0) * (level - 1);
            atk += (model.growthCurve?.atkPerLevel ?? 0) * (level - 1);
            def += (model.growthCurve?.defPerLevel ?? 0) * (level - 1);
            spd += (model.growthCurve?.spdPerLevel ?? 0) * (level - 1);

            ApplyAccessoryBonuses(CharacterId, ref hp, ref atk, ref def, ref spd, ref crit, ref res);

            var stats = new EntityStats(
                hp:   hp,
                atk:  atk,
                def:  def,
                spd:  spd,
                crit: crit,
                res:  res
            );

            Initialize(combatId, model.nameKey ?? model.id, stats, isPlayer: true);

            // Copy skill list
            if (model.skills != null)
                SkillIds.AddRange(model.skills);
        }

        /// <summary>Kiểm tra nhân vật có sở hữu skill không</summary>
        public bool HasSkill(string skillId) => SkillIds.Contains(skillId);

        private static void ApplyAccessoryBonuses(string characterId, ref int hp, ref int atk, ref int def, ref int spd, ref float crit, ref float res)
        {
            var save = SaveManager.Instance?.CurrentSave;
            if (save == null)
            {
                return;
            }

            var accessoryItemId = save.GetEquippedAccessory(characterId);
            if (string.IsNullOrWhiteSpace(accessoryItemId))
            {
                return;
            }

            var item = DataManager.Instance?.LoadItem(accessoryItemId);
            if (item == null || !string.Equals(item.itemType, "accessory", System.StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var bonuses = AccessoryStatUtility.GetBonuses(item);
            for (var i = 0; i < bonuses.Count; i++)
            {
                var bonus = bonuses[i];
                switch (bonus.StatKey)
                {
                    case "HP":
                        hp += bonus.Amount;
                        break;
                    case "ATK":
                        atk += bonus.Amount;
                        break;
                    case "DEF":
                        def += bonus.Amount;
                        break;
                    case "SPD":
                        spd += bonus.Amount;
                        break;
                    case "CRIT":
                        crit += bonus.Amount * 0.01f;
                        break;
                    case "RES":
                        res += bonus.Amount * 0.01f;
                        break;
                }
            }

            hp = UnityEngine.Mathf.Max(1, hp);
            atk = UnityEngine.Mathf.Max(1, atk);
            def = UnityEngine.Mathf.Max(0, def);
            spd = UnityEngine.Mathf.Max(1, spd);
        }
    }
}

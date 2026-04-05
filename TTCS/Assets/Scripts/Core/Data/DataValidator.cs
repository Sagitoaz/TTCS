using TTCS.Data;
using TTCS.Debugging;
using TTCS.Meta.Gacha;
using TTCS.Meta.Inventory;
using TTCS.Meta.Progression;

namespace TTCS.Core.Data
{
    public static class DataValidator
    {
        public static bool ValidateCharacter(CharacterDataModel data)
        {
            if (data == null)
            {
                DebugLogger.LogWarning("[DataValidator] CharacterDataModel is null", DebugLogger.LogCategory.Data);
                return false;
            }

            if (string.IsNullOrEmpty(data.id))
            {
                DebugLogger.LogWarning("[DataValidator] Character missing 'id'", DebugLogger.LogCategory.Data);
                return false;
            }

            if (string.IsNullOrEmpty(data.nameKey))
            {
                DebugLogger.LogWarning($"[DataValidator] Character '{data.id}' missing 'nameKey'", DebugLogger.LogCategory.Data);
                return false;
            }

            if (data.baseStats == null || data.baseStats.hp <= 0)
            {
                DebugLogger.LogWarning($"[DataValidator] Character '{data.id}' has invalid hp", DebugLogger.LogCategory.Data);
                return false;
            }

            if (data.baseStats.atk <= 0)
            {
                DebugLogger.LogWarning($"[DataValidator] Character '{data.id}' has invalid atk ({data.baseStats.atk})", DebugLogger.LogCategory.Data);
                return false;
            }

            return true;
        }

        public static bool ValidateSkill(SkillDataModel data)
        {
            if (data == null)
            {
                DebugLogger.LogWarning("[DataValidator] SkillDataModel is null", DebugLogger.LogCategory.Data);
                return false;
            }

            if (string.IsNullOrEmpty(data.id))
            {
                DebugLogger.LogWarning("[DataValidator] Skill missing 'id'", DebugLogger.LogCategory.Data);
                return false;
            }

            if (string.IsNullOrEmpty(data.nameKey))
            {
                DebugLogger.LogWarning($"[DataValidator] Skill '{data.id}' missing 'nameKey'", DebugLogger.LogCategory.Data);
                return false;
            }

            if (string.IsNullOrEmpty(data.type))
            {
                DebugLogger.LogWarning($"[DataValidator] Skill '{data.id}' missing 'type'", DebugLogger.LogCategory.Data);
                return false;
            }

            return true;
        }

        public static bool ValidateEnemy(EnemyDataModel data)
        {
            if (data == null)
            {
                DebugLogger.LogWarning("[DataValidator] EnemyDataModel is null", DebugLogger.LogCategory.Data);
                return false;
            }

            if (string.IsNullOrEmpty(data.id))
            {
                DebugLogger.LogWarning("[DataValidator] Enemy missing 'id'", DebugLogger.LogCategory.Data);
                return false;
            }

            if (string.IsNullOrEmpty(data.nameKey))
            {
                DebugLogger.LogWarning($"[DataValidator] Enemy '{data.id}' missing 'nameKey'", DebugLogger.LogCategory.Data);
                return false;
            }

            if (data.baseStats == null || data.baseStats.hp <= 0)
            {
                DebugLogger.LogWarning($"[DataValidator] Enemy '{data.id}' has invalid hp", DebugLogger.LogCategory.Data);
                return false;
            }

            return true;
        }

        public static bool ValidateStage(StageDataModel data)
        {
            if (data == null)
            {
                DebugLogger.LogWarning("[DataValidator] StageDataModel is null", DebugLogger.LogCategory.Data);
                return false;
            }

            if (string.IsNullOrEmpty(data.id))
            {
                DebugLogger.LogWarning("[DataValidator] Stage missing 'id'", DebugLogger.LogCategory.Data);
                return false;
            }

            if (string.IsNullOrEmpty(data.nameKey))
            {
                DebugLogger.LogWarning($"[DataValidator] Stage '{data.id}' missing 'nameKey'", DebugLogger.LogCategory.Data);
                return false;
            }

            if (data.encounters == null || data.encounters.Count == 0)
            {
                DebugLogger.LogWarning($"[DataValidator] Stage '{data.id}' has no encounters", DebugLogger.LogCategory.Data);
                return false;
            }

            return true;
        }

        public static bool ValidateChapter(ChapterDataModel data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.id))
            {
                DebugLogger.LogWarning("[DataValidator] Chapter invalid or missing id", DebugLogger.LogCategory.Data);
                return false;
            }

            if (data.order <= 0)
            {
                DebugLogger.LogWarning($"[DataValidator] Chapter '{data.id}' has invalid order", DebugLogger.LogCategory.Data);
                return false;
            }

            return true;
        }

        public static bool ValidateLevel(LevelDataModel data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.id))
            {
                DebugLogger.LogWarning("[DataValidator] Level invalid or missing id", DebugLogger.LogCategory.Data);
                return false;
            }

            if (string.IsNullOrWhiteSpace(data.chapterId))
            {
                DebugLogger.LogWarning($"[DataValidator] Level '{data.id}' missing chapterId", DebugLogger.LogCategory.Data);
                return false;
            }

            return true;
        }

        public static bool ValidateGachaPool(GachaPoolDataModel data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.id))
            {
                DebugLogger.LogWarning("[DataValidator] Gacha pool invalid or missing id", DebugLogger.LogCategory.Data);
                return false;
            }

            if (data.entries == null || data.entries.Count == 0)
            {
                DebugLogger.LogWarning($"[DataValidator] Gacha pool '{data.id}' has no entries", DebugLogger.LogCategory.Data);
                return false;
            }

            return true;
        }

        public static bool ValidateItem(ItemDataModel data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.id))
            {
                DebugLogger.LogWarning("[DataValidator] Item invalid or missing id", DebugLogger.LogCategory.Data);
                return false;
            }

            if (data.maxStack <= 0)
            {
                DebugLogger.LogWarning($"[DataValidator] Item '{data.id}' has invalid maxStack", DebugLogger.LogCategory.Data);
                return false;
            }
            
            if (string.Equals(data.itemType, "consumable",System.StringComparison.OrdinalIgnoreCase)
                && data.effectAmount <= 0
                && data.healAmount <= 0)
            {
                DebugLogger.LogWarning($"[DataValidator] Consumable item '{data.id}' has no effectAmount/healAmount", DebugLogger.LogCategory.Data);
            }

            return true;
        }

        public static bool ValidateSkillIconMap(SkillIconMapDataModel data)
        {
            if (data == null)
            {
                DebugLogger.LogWarning("[DataValidator] SkillIconMap is null", DebugLogger.LogCategory.Data);
                return false;
            }

            if (string.IsNullOrWhiteSpace(data.fallbackIconPath))
            {
                DebugLogger.LogWarning("[DataValidator] SkillIconMap missing fallbackIconPath", DebugLogger.LogCategory.Data);
                return false;
            }

            return true;
        }
    }
}

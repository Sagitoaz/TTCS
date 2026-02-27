using TTCS.Data;
using TTCS.Debugging;

namespace TTCS.Core.Data
{
    /// <summary>
    /// 🔵 Dev A - Static validator cho các data model sau khi deserialize.
    /// Trả về false và log warning nếu data không hợp lệ.
    /// </summary>
    public static class DataValidator
    {
        /// <summary>Validate CharacterDataModel — kiểm tra fields bắt buộc</summary>
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
                DebugLogger.LogWarning($"[DataValidator] Character '{data.id}' has invalid hp ({data.baseStats?.hp})", DebugLogger.LogCategory.Data);
                return false;
            }
            if (data.baseStats.atk <= 0)
            {
                DebugLogger.LogWarning($"[DataValidator] Character '{data.id}' has invalid atk ({data.baseStats.atk})", DebugLogger.LogCategory.Data);
                return false;
            }
            return true;
        }

        /// <summary>Validate SkillDataModel</summary>
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

        /// <summary>Validate EnemyDataModel</summary>
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

        /// <summary>Validate StageDataModel</summary>
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
    }
}

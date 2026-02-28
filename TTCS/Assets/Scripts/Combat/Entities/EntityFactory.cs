using System.Collections.Generic;
using TTCS.Core.Data;
using TTCS.Data;
using TTCS.Debugging;
using static TTCS.Debugging.DebugLogger;

namespace TTCS.Combat.Entities
{
    /// <summary>
    /// 🟢 Dev B - Entity Factory
    /// Tạo Character và Enemy từ data model (JSON-loaded) hoặc ID (qua DataManager).
    ///
    /// API chính:
    ///   EntityFactory.CreateCharacter(model)     — từ model trực tiếp
    ///   EntityFactory.CreateCharacter(id)        — từ ID, yêu cầu DataManager.Instance
    ///   EntityFactory.CreateEnemy(model, index)  — từ model trực tiếp
    ///   EntityFactory.CreateEnemy(id, index)     — từ ID
    ///   EntityFactory.CreateWave(enemyIds)       — tạo nhiều enemy từ danh sách ID
    ///   EntityFactory.CreateParty(characterIds)  — tạo nhiều character từ danh sách ID
    /// </summary>
    public static class EntityFactory
    {
        // ─── Character ────────────────────────────────────────────────────

        /// <summary>Tạo Character từ CharacterDataModel trực tiếp</summary>
        public static Character CreateCharacter(CharacterDataModel model, int instanceIndex = 0)
        {
            if (model == null)
            {
                Log("EntityFactory: CharacterDataModel is null", LogCategory.Combat);
                return null;
            }

            var character = new Character(model, instanceIndex);
            Log($"EntityFactory: Created Character [{character.ID}] HP={character.Health.MaxHP} ATK={character.ATK}", LogCategory.Combat);
            return character;
        }

        /// <summary>Tạo Character bằng ID — DataManager phải sẵn sàng</summary>
        public static Character CreateCharacter(string characterId, int instanceIndex = 0)
        {
            if (DataManager.Instance == null)
            {
                Log("EntityFactory: DataManager.Instance is null!", LogCategory.Combat);
                return null;
            }

            var model = DataManager.Instance.LoadCharacter(characterId);
            if (model == null)
            {
                Log($"EntityFactory: No CharacterDataModel found for id='{characterId}'", LogCategory.Combat);
                return null;
            }

            return CreateCharacter(model, instanceIndex);
        }

        /// <summary>Tạo danh sách Character từ danh sách ID</summary>
        public static List<Character> CreateParty(IEnumerable<string> characterIds)
        {
            var party = new List<Character>();
            int idx   = 0;

            foreach (var id in characterIds)
            {
                var c = CreateCharacter(id, idx);
                if (c != null)
                {
                    party.Add(c);
                    idx++;
                }
            }

            Log($"EntityFactory: Created party of {party.Count} characters", LogCategory.Combat);
            return party;
        }

        // ─── Enemy ────────────────────────────────────────────────────────

        /// <summary>Tạo Enemy từ EnemyDataModel trực tiếp</summary>
        public static Enemy CreateEnemy(EnemyDataModel model, int spawnIndex = 0)
        {
            if (model == null)
            {
                Log("EntityFactory: EnemyDataModel is null", LogCategory.Combat);
                return null;
            }

            var enemy = new Enemy(model, spawnIndex);
            Log($"EntityFactory: Created Enemy [{enemy.ID}] HP={enemy.Health.MaxHP} ATK={enemy.ATK}", LogCategory.Combat);
            return enemy;
        }

        /// <summary>Tạo Enemy bằng ID — DataManager phải sẵn sàng</summary>
        public static Enemy CreateEnemy(string enemyId, int spawnIndex = 0)
        {
            if (DataManager.Instance == null)
            {
                Log("EntityFactory: DataManager.Instance is null!", LogCategory.Combat);
                return null;
            }

            var model = DataManager.Instance.LoadEnemy(enemyId);
            if (model == null)
            {
                Log($"EntityFactory: No EnemyDataModel found for id='{enemyId}'", LogCategory.Combat);
                return null;
            }

            return CreateEnemy(model, spawnIndex);
        }

        /// <summary>Tạo danh sách Enemy từ danh sách ID (ví dụ: một wave)</summary>
        public static List<Enemy> CreateWave(IEnumerable<string> enemyIds)
        {
            var enemies = new List<Enemy>();
            int idx     = 0;

            foreach (var id in enemyIds)
            {
                var e = CreateEnemy(id, idx);
                if (e != null)
                {
                    enemies.Add(e);
                    idx++;
                }
            }

            Log($"EntityFactory: Created wave of {enemies.Count} enemies", LogCategory.Combat);
            return enemies;
        }
    }
}

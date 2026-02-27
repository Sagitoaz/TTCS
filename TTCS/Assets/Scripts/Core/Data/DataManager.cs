using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TTCS.Core.Events;
using TTCS.Data;
using TTCS.Debugging;

namespace TTCS.Core.Data
{
    /// <summary>
    /// 🔵 Dev A - Data Manager Singleton.
    /// Tải, cache và cung cấp tất cả game data từ JSON files.
    /// Đặt GameObject này vào scene với tag "DataManager".
    /// </summary>
    public class DataManager : MonoBehaviour
    {
        private static DataManager _instance;
        public static DataManager Instance
        {
            get
            {
                if (_instance == null)
                    Debug.LogWarning("[DataManager] Instance accessed before Awake(). Make sure DataManager is in scene.");
                return _instance;
            }
        }

        // ─── Caches ───────────────────────────────────────────────────────────
        private readonly DataCache<CharacterDataModel> _characterCache = new DataCache<CharacterDataModel>();
        private readonly DataCache<SkillDataModel>     _skillCache     = new DataCache<SkillDataModel>();
        private readonly DataCache<EnemyDataModel>     _enemyCache     = new DataCache<EnemyDataModel>();
        private readonly DataCache<StageDataModel>     _stageCache     = new DataCache<StageDataModel>();

        /// <summary>True sau khi LoadAllData() hoàn thành</summary>
        public bool IsLoaded { get; private set; }

        // Thư mục gốc của data files (relative to Application.dataPath)
        private const string DATA_ROOT = "Data";

        // ─── Unity Lifecycle ──────────────────────────────────────────────────

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            LoadAllData();
        }

        // ─── Public API ───────────────────────────────────────────────────────

        /// <summary>Tải toàn bộ data từ Assets/Data/. Gọi tự động trong Awake().</summary>
        public void LoadAllData()
        {
            DebugLogger.Log("[DataManager] Loading all data...", DebugLogger.LogCategory.Data);

            LoadFolder<CharacterDataModel>("Characters", _characterCache, DataValidator.ValidateCharacter);
            LoadFolder<SkillDataModel>    ("Skills",     _skillCache,     DataValidator.ValidateSkill);
            LoadFolder<EnemyDataModel>    ("Enemies",    _enemyCache,     DataValidator.ValidateEnemy);
            LoadFolder<StageDataModel>    ("Stages",     _stageCache,     DataValidator.ValidateStage);

            IsLoaded = true;

            DebugLogger.Log(
                $"[DataManager] Load complete — Characters:{_characterCache.Count} " +
                $"Skills:{_skillCache.Count} Enemies:{_enemyCache.Count} Stages:{_stageCache.Count}",
                DebugLogger.LogCategory.Data);

            EventBus.Instance.Publish(new DataLoadedEvent(
                _characterCache.Count, _skillCache.Count,
                _enemyCache.Count,     _stageCache.Count));
        }

        // ─── Character ───

        /// <summary>Lấy character theo id. Trả về null nếu không tìm thấy.</summary>
        public CharacterDataModel LoadCharacter(string id)
        {
            return GetOrLoad<CharacterDataModel>(id, "Characters", _characterCache, DataValidator.ValidateCharacter);
        }

        /// <summary>Trả về tất cả characters đã load</summary>
        public IReadOnlyCollection<CharacterDataModel> GetAllCharacters() => _characterCache.GetAll();

        // ─── Skill ───

        /// <summary>Lấy skill theo id. Trả về null nếu không tìm thấy.</summary>
        public SkillDataModel LoadSkill(string id)
        {
            return GetOrLoad<SkillDataModel>(id, "Skills", _skillCache, DataValidator.ValidateSkill);
        }

        /// <summary>Trả về tất cả skills đã load</summary>
        public IReadOnlyCollection<SkillDataModel> GetAllSkills() => _skillCache.GetAll();

        // ─── Enemy ───

        /// <summary>Lấy enemy theo id. Trả về null nếu không tìm thấy.</summary>
        public EnemyDataModel LoadEnemy(string id)
        {
            return GetOrLoad<EnemyDataModel>(id, "Enemies", _enemyCache, DataValidator.ValidateEnemy);
        }

        /// <summary>Trả về tất cả enemies đã load</summary>
        public IReadOnlyCollection<EnemyDataModel> GetAllEnemies() => _enemyCache.GetAll();

        // ─── Stage ───

        /// <summary>Lấy stage theo id. Trả về null nếu không tìm thấy.</summary>
        public StageDataModel LoadStage(string id)
        {
            return GetOrLoad<StageDataModel>(id, "Stages", _stageCache, DataValidator.ValidateStage);
        }

        /// <summary>Trả về tất cả stages đã load</summary>
        public IReadOnlyCollection<StageDataModel> GetAllStages() => _stageCache.GetAll();

        // ─── Private Helpers ─────────────────────────────────────────────────

        /// <summary>
        /// Tải tất cả .json files trong subfolder và lưu vào cache.
        /// </summary>
        private void LoadFolder<T>(
            string subfolder,
            DataCache<T> cache,
            Func<T, bool> validator) where T : class
        {
            string folderPath = Path.Combine(Application.dataPath, DATA_ROOT, subfolder);

            if (!Directory.Exists(folderPath))
            {
                DebugLogger.LogWarning($"[DataManager] Data folder not found: {folderPath}", DebugLogger.LogCategory.Data);
                return;
            }

            string[] files = Directory.GetFiles(folderPath, "*.json");

            foreach (string filePath in files)
            {
                T data = ReadJsonFile<T>(filePath);
                if (data == null) continue;
                if (!validator(data)) continue;

                // Lấy id qua reflection để dùng làm cache key
                string id = GetIdField(data);
                if (string.IsNullOrEmpty(id))
                {
                    DebugLogger.LogWarning($"[DataManager] Could not read 'id' from {Path.GetFileName(filePath)}", DebugLogger.LogCategory.Data);
                    continue;
                }

                cache.Set(id, data);
            }
        }

        /// <summary>
        /// Lấy object từ cache; nếu chưa có thì load file theo pattern {subfolder}/{id}.json
        /// </summary>
        private T GetOrLoad<T>(
            string id,
            string subfolder,
            DataCache<T> cache,
            Func<T, bool> validator) where T : class
        {
            if (cache.Has(id)) return cache.Get(id);

            string filePath = Path.Combine(Application.dataPath, DATA_ROOT, subfolder, $"{id}.json");
            T data = ReadJsonFile<T>(filePath);

            if (data == null || !validator(data)) return null;

            cache.Set(id, data);
            return data;
        }

        /// <summary>Đọc và deserialize một file JSON. Trả về null nếu lỗi.</summary>
        private T ReadJsonFile<T>(string filePath) where T : class
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    DebugLogger.LogWarning($"[DataManager] File not found: {filePath}", DebugLogger.LogCategory.Data);
                    return null;
                }

                string json = File.ReadAllText(filePath);
                T result = JsonUtility.FromJson<T>(json);
                return result;
            }
            catch (Exception e)
            {
                DebugLogger.LogError($"[DataManager] Failed to parse {Path.GetFileName(filePath)}: {e.Message}", DebugLogger.LogCategory.Data);
                return null;
            }
        }

        /// <summary>Đọc field 'id' từ object bằng reflection (tránh dùng interface).</summary>
        private string GetIdField<T>(T obj) where T : class
        {
            var field = typeof(T).GetField("id");
            if (field == null) return null;
            return field.GetValue(obj) as string;
        }
    }
}

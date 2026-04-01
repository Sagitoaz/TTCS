using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TTCS.Core.Events;
using TTCS.Data;
using TTCS.Debugging;
using TTCS.Meta.Gacha;
using TTCS.Meta.Inventory;
using TTCS.Meta.Progression;

namespace TTCS.Core.Data
{
    public class DataManager : MonoBehaviour
    {
        private static DataManager _instance;
        public static DataManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogWarning("[DataManager] Instance accessed before Awake(). Make sure DataManager is in scene.");
                }

                return _instance;
            }
        }

        private readonly DataCache<CharacterDataModel> _characterCache = new DataCache<CharacterDataModel>();
        private readonly DataCache<SkillDataModel> _skillCache = new DataCache<SkillDataModel>();
        private readonly DataCache<EnemyDataModel> _enemyCache = new DataCache<EnemyDataModel>();
        private readonly DataCache<StageDataModel> _stageCache = new DataCache<StageDataModel>();

        private readonly DataCache<ChapterDataModel> _chapterCache = new DataCache<ChapterDataModel>();
        private readonly DataCache<LevelDataModel> _levelCache = new DataCache<LevelDataModel>();
        private readonly DataCache<GachaPoolDataModel> _gachaPoolCache = new DataCache<GachaPoolDataModel>();
        private readonly DataCache<ItemDataModel> _itemCache = new DataCache<ItemDataModel>();
        private SkillIconMapDataModel _skillIconMap;

        public bool IsLoaded { get; private set; }
        private const string DataRoot = "Data";

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

        public void LoadAllData()
        {
            DebugLogger.Log("[DataManager] Loading all data...", DebugLogger.LogCategory.Data);

            LoadFolder("Characters", _characterCache, DataValidator.ValidateCharacter);
            LoadFolder("Skills", _skillCache, DataValidator.ValidateSkill);
            LoadFolder("Enemies", _enemyCache, DataValidator.ValidateEnemy);
            LoadFolder("Stages", _stageCache, DataValidator.ValidateStage);

            LoadFolder("Chapters", _chapterCache, DataValidator.ValidateChapter);
            LoadFolder("Levels", _levelCache, DataValidator.ValidateLevel);
            LoadFolder("Gacha", _gachaPoolCache, DataValidator.ValidateGachaPool);
            LoadFolder("Items", _itemCache, DataValidator.ValidateItem);
            LoadSkillIconMap();

            IsLoaded = true;

            DebugLogger.Log(
                $"[DataManager] Load complete - Characters:{_characterCache.Count} Skills:{_skillCache.Count} Enemies:{_enemyCache.Count} Stages:{_stageCache.Count} Chapters:{_chapterCache.Count} Levels:{_levelCache.Count} GachaPools:{_gachaPoolCache.Count} Items:{_itemCache.Count}",
                DebugLogger.LogCategory.Data);

            EventBus.Instance.Publish(new DataLoadedEvent(
                _characterCache.Count,
                _skillCache.Count,
                _enemyCache.Count,
                _stageCache.Count));
        }

        public CharacterDataModel LoadCharacter(string id) => GetOrLoad(id, "Characters", _characterCache, DataValidator.ValidateCharacter);
        public IReadOnlyCollection<CharacterDataModel> GetAllCharacters() => _characterCache.GetAll();

        public SkillDataModel LoadSkill(string id) => GetOrLoad(id, "Skills", _skillCache, DataValidator.ValidateSkill);
        public IReadOnlyCollection<SkillDataModel> GetAllSkills() => _skillCache.GetAll();

        public EnemyDataModel LoadEnemy(string id) => GetOrLoad(id, "Enemies", _enemyCache, DataValidator.ValidateEnemy);
        public IReadOnlyCollection<EnemyDataModel> GetAllEnemies() => _enemyCache.GetAll();

        public StageDataModel LoadStage(string id) => GetOrLoad(id, "Stages", _stageCache, DataValidator.ValidateStage);
        public IReadOnlyCollection<StageDataModel> GetAllStages() => _stageCache.GetAll();

        public ChapterDataModel LoadChapter(string id) => GetOrLoad(id, "Chapters", _chapterCache, DataValidator.ValidateChapter);
        public IReadOnlyCollection<ChapterDataModel> GetAllChapters() => _chapterCache.GetAll();

        public LevelDataModel LoadLevel(string id) => GetOrLoad(id, "Levels", _levelCache, DataValidator.ValidateLevel);
        public IReadOnlyCollection<LevelDataModel> GetAllLevels() => _levelCache.GetAll();

        public GachaPoolDataModel LoadGachaPool(string id) => GetOrLoad(id, "Gacha", _gachaPoolCache, DataValidator.ValidateGachaPool);
        public IReadOnlyCollection<GachaPoolDataModel> GetAllGachaPools() => _gachaPoolCache.GetAll();

        public ItemDataModel LoadItem(string id) => GetOrLoad(id, "Items", _itemCache, DataValidator.ValidateItem);
        public IReadOnlyCollection<ItemDataModel> GetAllItems() => _itemCache.GetAll();

        public string ResolveSkillIcon(string skillId)
        {
            if (_skillIconMap?.entries == null)
            {
                return string.Empty;
            }

            for (var i = 0; i < _skillIconMap.entries.Count; i++)
            {
                var entry = _skillIconMap.entries[i];
                if (entry.skillId == skillId)
                {
                    return entry.iconPath;
                }
            }

            return _skillIconMap.fallbackIconPath;
        }

        public string ResolveLevelIdByStageId(string stageId)
        {
            if (string.IsNullOrWhiteSpace(stageId))
            {
                return string.Empty;
            }

            var levels = GetAllLevels();
            foreach (var level in levels)
            {
                if (level != null && level.stageId == stageId)
                {
                    return level.id;
                }
            }

            return stageId;
        }

        private void LoadSkillIconMap()
        {
            string filePath = Path.Combine(Application.dataPath, DataRoot, "Meta", "skill_icon_map.json");
            _skillIconMap = ReadJsonFile<SkillIconMapDataModel>(filePath);
            if (_skillIconMap == null)
            {
                _skillIconMap = new SkillIconMapDataModel();
            }

            if (!DataValidator.ValidateSkillIconMap(_skillIconMap))
            {
                _skillIconMap = new SkillIconMapDataModel();
            }
        }

        private void LoadFolder<T>(string subfolder, DataCache<T> cache, Func<T, bool> validator) where T : class
        {
            string folderPath = Path.Combine(Application.dataPath, DataRoot, subfolder);
            if (!Directory.Exists(folderPath))
            {
                DebugLogger.LogWarning($"[DataManager] Data folder not found: {folderPath}", DebugLogger.LogCategory.Data);
                return;
            }

            string[] files = Directory.GetFiles(folderPath, "*.json");
            foreach (string filePath in files)
            {
                T data = ReadJsonFile<T>(filePath);
                if (data == null || !validator(data))
                {
                    continue;
                }

                string id = GetIdField(data);
                if (string.IsNullOrEmpty(id))
                {
                    DebugLogger.LogWarning($"[DataManager] Could not read 'id' from {Path.GetFileName(filePath)}", DebugLogger.LogCategory.Data);
                    continue;
                }

                cache.Set(id, data);
            }
        }

        private T GetOrLoad<T>(string id, string subfolder, DataCache<T> cache, Func<T, bool> validator) where T : class
        {
            if (cache.Has(id))
            {
                return cache.Get(id);
            }

            string filePath = Path.Combine(Application.dataPath, DataRoot, subfolder, $"{id}.json");
            T data = ReadJsonFile<T>(filePath);
            if (data == null || !validator(data))
            {
                return null;
            }

            cache.Set(id, data);
            return data;
        }

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
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception e)
            {
                DebugLogger.LogError($"[DataManager] Failed to parse {Path.GetFileName(filePath)}: {e.Message}", DebugLogger.LogCategory.Data);
                return null;
            }
        }

        private static string GetIdField<T>(T obj) where T : class
        {
            var field = typeof(T).GetField("id");
            return field?.GetValue(obj) as string;
        }
    }
}

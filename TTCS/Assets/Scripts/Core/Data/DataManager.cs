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
#if UNITY_EDITOR
using UnityEditor;
#endif

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

        // ─── Path constants ───────────────────────────────────────────────
        /// <summary>
        /// Path dưới Resources/ — phải đặt JSON files vào Assets/Resources/Data/...
        /// Đây là cách duy nhất để data được đóng gói vào build.
        /// </summary>
        private const string ResourcesDataRoot = "Data";

        /// <summary>
        /// Fallback path dùng trong Editor (Application.dataPath = Assets/).
        /// Cho phép load thẳng từ Assets/Data/ khi chạy Play Mode mà không cần copy sang Resources.
        /// </summary>
        private const string EditorDataRoot = "Data";
        private const string SpriteRoot = "Sprites";
        private const string CharacterSpriteFolder = "Characters";

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

        public Sprite LoadCharacterPortraitSprite(string portraitPath)
        {
            if (string.IsNullOrWhiteSpace(portraitPath))
            {
                return null;
            }

            var normalized = NormalizePortraitPath(portraitPath);

            foreach (var resourcePath in BuildResourceSpriteCandidates(portraitPath, normalized))
            {
                var resourceSprite = Resources.Load<Sprite>(resourcePath);
                if (resourceSprite != null)
                {
                    return resourceSprite;
                }
            }

#if UNITY_EDITOR
            var editorPath = normalized.Replace("\\", "/");
            if (!editorPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                editorPath = $"Assets/{editorPath}";
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(editorPath);
            if (sprite != null)
            {
                return sprite;
            }

            var exts = new[] { ".png", ".jpg", ".jpeg", ".tga", ".psd" };
            for (var i = 0; i < exts.Length; i++)
            {
                var withExt = Path.ChangeExtension(editorPath, exts[i]);
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(withExt);
                if (sprite != null)
                {
                    return sprite;
                }
            }
#endif

            return null;
        }

        private static string NormalizePortraitPath(string rawPath)
        {
            var path = rawPath.Replace("\\", "/").Trim();
            path = Path.ChangeExtension(path, null)?.Replace("\\", "/") ?? path;

            if (path.StartsWith("Assets/Sprites/", StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            if (path.StartsWith("Sprites/", StringComparison.OrdinalIgnoreCase))
            {
                return $"Assets/{path}";
            }

            if (path.StartsWith("Characters/", StringComparison.OrdinalIgnoreCase))
            {
                return $"Assets/{SpriteRoot}/{path}";
            }

            if (path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            return $"Assets/{SpriteRoot}/{CharacterSpriteFolder}/{path}";
        }

        private static IEnumerable<string> BuildResourceSpriteCandidates(string rawPath, string normalizedPath)
        {
            foreach (var path in NormalizeResourceCandidate(rawPath))
                yield return path;

            foreach (var path in NormalizeResourceCandidate(normalizedPath))
                yield return path;

            var raw = Path.ChangeExtension(rawPath.Replace("\\", "/").Trim(), null)?.Replace("\\", "/") ?? rawPath;
            if (raw.StartsWith("Characters/", StringComparison.OrdinalIgnoreCase)
                || raw.StartsWith("Enemies/", StringComparison.OrdinalIgnoreCase))
            {
                yield return $"{SpriteRoot}/{raw}";
            }

            var fileName = Path.GetFileName(raw);
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                yield return $"{SpriteRoot}/{CharacterSpriteFolder}/{fileName}";
                yield return $"{SpriteRoot}/Enemies/{fileName}";
            }
        }

        private static IEnumerable<string> NormalizeResourceCandidate(string rawPath)
        {
            if (string.IsNullOrWhiteSpace(rawPath))
                yield break;

            var path = rawPath.Replace("\\", "/").Trim();
            if (path.StartsWith("Assets/Resources/", StringComparison.OrdinalIgnoreCase))
                path = path.Substring("Assets/Resources/".Length);
            else if (path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                path = path.Substring("Assets/".Length);
            else if (path.StartsWith("Resources/", StringComparison.OrdinalIgnoreCase))
                path = path.Substring("Resources/".Length);

            path = Path.ChangeExtension(path, null)?.Replace("\\", "/") ?? path;
            if (!string.IsNullOrWhiteSpace(path))
                yield return path;
        }

        private void LoadSkillIconMap()
        {
            // Thử Resources trước (hoạt động trong cả Editor và build)
            var resourcePath = $"{ResourcesDataRoot}/Meta/skill_icon_map";
            var textAsset = Resources.Load<TextAsset>(resourcePath);
            if (textAsset != null)
            {
                _skillIconMap = ParseJson<SkillIconMapDataModel>(textAsset.text, "skill_icon_map");
            }

#if UNITY_EDITOR
            // Fallback: đọc thẳng từ Assets/Data/ khi chạy trong Editor
            if (_skillIconMap == null)
            {
                string filePath = Path.Combine(Application.dataPath, EditorDataRoot, "Meta", "skill_icon_map.json");
                _skillIconMap = ReadJsonFileFromDisk<SkillIconMapDataModel>(filePath);
            }
#endif

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
            // ── Ưu tiên 1: Load từ Resources/Data/<subfolder>/ (hoạt động trong build) ──
            string resourceFolder = $"{ResourcesDataRoot}/{subfolder}";
            var textAssets = Resources.LoadAll<TextAsset>(resourceFolder);
            if (textAssets != null && textAssets.Length > 0)
            {
                foreach (var ta in textAssets)
                {
                    T data = ParseJson<T>(ta.text, ta.name);
                    if (data == null || !validator(data)) continue;

                    string id = GetIdField(data);
                    if (string.IsNullOrEmpty(id))
                    {
                        DebugLogger.LogWarning($"[DataManager] Could not read 'id' from Resources asset '{ta.name}'", DebugLogger.LogCategory.Data);
                        continue;
                    }
                    cache.Set(id, data);
                }
                return; // Resources loaded successfully
            }

#if UNITY_EDITOR
            // ── Fallback Editor: Load từ Assets/Data/<subfolder>/ ──
            string folderPath = Path.Combine(Application.dataPath, EditorDataRoot, subfolder);
            if (!Directory.Exists(folderPath))
            {
                DebugLogger.LogWarning($"[DataManager] Data folder not found: {folderPath}", DebugLogger.LogCategory.Data);
                return;
            }

            string[] files = Directory.GetFiles(folderPath, "*.json");
            foreach (string filePath in files)
            {
                T data = ReadJsonFileFromDisk<T>(filePath);
                if (data == null || !validator(data)) continue;

                string id = GetIdField(data);
                if (string.IsNullOrEmpty(id))
                {
                    DebugLogger.LogWarning($"[DataManager] Could not read 'id' from {Path.GetFileName(filePath)}", DebugLogger.LogCategory.Data);
                    continue;
                }
                cache.Set(id, data);
            }
#else
            DebugLogger.LogWarning($"[DataManager] No Resources data found for folder '{subfolder}'. Make sure JSON files are under Assets/Resources/Data/{subfolder}/", DebugLogger.LogCategory.Data);
#endif
        }

        private T GetOrLoad<T>(string id, string subfolder, DataCache<T> cache, Func<T, bool> validator) where T : class
        {
            if (cache.Has(id))
            {
                return cache.Get(id);
            }

            // Thử Resources trước
            string resourcePath = $"{ResourcesDataRoot}/{subfolder}/{id}";
            var textAsset = Resources.Load<TextAsset>(resourcePath);
            if (textAsset != null)
            {
                T resData = ParseJson<T>(textAsset.text, id);
                if (resData != null && validator(resData))
                {
                    cache.Set(id, resData);
                    return resData;
                }
            }

#if UNITY_EDITOR
            // Fallback Editor: đọc từ Assets/Data/
            string filePath = Path.Combine(Application.dataPath, EditorDataRoot, subfolder, $"{id}.json");
            T data = ReadJsonFileFromDisk<T>(filePath);
            if (data == null || !validator(data))
            {
                return null;
            }

            cache.Set(id, data);
            return data;
#else
            return null;
#endif
        }

        private T ParseJson<T>(string json, string sourceName) where T : class
        {
            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception e)
            {
                DebugLogger.LogError($"[DataManager] Failed to parse '{sourceName}': {e.Message}", DebugLogger.LogCategory.Data);
                return null;
            }
        }

#if UNITY_EDITOR
        private T ReadJsonFileFromDisk<T>(string filePath) where T : class
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
#endif

        private static string GetIdField<T>(T obj) where T : class
        {
            var field = typeof(T).GetField("id");
            return field?.GetValue(obj) as string;
        }
    }
}

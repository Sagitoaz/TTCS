using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TTCS.Core.Data;
using TTCS.Data;
using TTCS.Meta;
using TTCS.Meta.Inventory;
using TTCS.Meta.Progression;
using TTCS.Meta.Team;
using UnityEngine;
using UnityEngine.UI;

namespace TTCS.Flow.LevelSelect
{
    public class LevelSelectUIController : MonoBehaviour
    {
        [Header("Chapter List (Vertical Scroll)")]
        [SerializeField] private RectTransform _chapterContent;
        [SerializeField] private ChapterView _chapterViewPrefab;
        [SerializeField] private float _chapterSpacing = 18f;
        [SerializeField] private float _chapterPaddingTopBottom = 24f;

        [Header("Level List (Vertical Scroll)")]
        [SerializeField] private RectTransform _levelContent;
        [SerializeField] private LevelView _levelViewPrefab;
        [SerializeField] private float _levelSpacing = 16f;
        [SerializeField] private float _levelPaddingTopBottom = 20f;

        [Header("Level Details")]
        [SerializeField] private TMP_Text _detailLevelTitleText;
        [SerializeField] private Transform _enemyRoot;
        [SerializeField] private LevelEnemyView _enemyViewPrefab;
        [SerializeField] private TMP_Text _starConditionText1;
        [SerializeField] private TMP_Text _starConditionText2;
        [SerializeField] private TMP_Text _starConditionText3;
        [SerializeField] private Image _starStateImage1;
        [SerializeField] private Image _starStateImage2;
        [SerializeField] private Image _starStateImage3;

        [Header("Reward List (Horizontal Scroll)")]
        [SerializeField] private RectTransform _rewardContent;
        [SerializeField] private LevelRewardItemView _rewardItemViewPrefab;
        [SerializeField] private float _rewardSpacing = 14f;
        [SerializeField] private float _rewardPaddingLeftRight = 18f;

        [Header("Actions")]
        [SerializeField] private Button _fightButton;
        [SerializeField] private Button _backButton;

        public IProgressionService ProgressionService { get; set; }

        private readonly List<ChapterDataModel> _chapters = new List<ChapterDataModel>();
        private readonly List<LevelDataModel> _levels = new List<LevelDataModel>();
        private readonly List<GameObject> _spawnedChapterViews = new List<GameObject>();
        private readonly List<GameObject> _spawnedLevelViews = new List<GameObject>();
        private readonly List<GameObject> _spawnedEnemyViews = new List<GameObject>();
        private readonly List<GameObject> _spawnedRewardViews = new List<GameObject>();

        private string _selectedChapterId;
        private LevelDataModel _selectedLevel;

        private const string StarUnlockedPath = "UI/Star/star_unlocked";
        private const string StarLockedPath = "UI/Star/star_locked";

        private void Start()
        {
            if (_backButton != null)
            {
                _backButton.onClick.AddListener(BackToMainMenu);
            }

            if (_fightButton != null)
            {
                _fightButton.onClick.AddListener(OnFightClicked);
            }

            StartCoroutine(InitializeWhenReady());
        }

        private IEnumerator InitializeWhenReady()
        {
            var hub = MetaServiceHub.Instance;
            hub?.EnsureInitialized();

            const int maxWaitFrames = 120;
            var waited = 0;
            while (waited < maxWaitFrames)
            {
                var dataReady = DataManager.Instance != null && DataManager.Instance.IsLoaded;
                ProgressionService ??= hub?.ProgressionService;

                if (dataReady && ProgressionService != null)
                {
                    break;
                }

                waited++;
                yield return null;
            }

            if (DataManager.Instance == null || !DataManager.Instance.IsLoaded)
            {
                Debug.LogWarning("[LevelSelectUI] DataManager not ready; level data may be missing.");
            }

            if (ProgressionService == null)
            {
                Debug.LogWarning("[LevelSelectUI] ProgressionService not ready; unlock states may be missing.");
            }

            BuildSources();
            RebuildChapterList();

            if (!string.IsNullOrWhiteSpace(_selectedChapterId))
            {
                OnChapterSelected(_selectedChapterId);
            }
        }

        private void BuildSources()
        {
            _chapters.Clear();
            _levels.Clear();

            var allChapters = DataManager.Instance?.GetAllChapters();
            if (allChapters != null)
            {
                _chapters.AddRange(allChapters.Where(c => c != null).OrderBy(c => c.order));
            }

            var allLevels = DataManager.Instance?.GetAllLevels();
            if (allLevels != null)
            {
                _levels.AddRange(allLevels.Where(l => l != null).OrderBy(l => l.order));
            }

            if (_chapters.Count > 0)
            {
                _selectedChapterId = _chapters[0].id;
            }
        }

        private void RebuildChapterList()
        {
            ClearSpawned(_spawnedChapterViews);
            if (_chapterContent == null || _chapterViewPrefab == null)
            {
                return;
            }

            for (var i = 0; i < _chapters.Count; i++)
            {
                var chapter = _chapters[i];
                var chapterState = ProgressionService?.GetChapterState(chapter.id);
                var isUnlocked = chapterState == null || chapterState.Value.IsUnlocked;

                var view = Instantiate(_chapterViewPrefab, _chapterContent);
                _spawnedChapterViews.Add(view.gameObject);

                var chapterIndex = Math.Max(1, chapter.order);
                var chapterName = chapter.nameKey;
                var chapterId = chapter.id;
                view.Bind(chapterIndex, chapterName, isUnlocked, chapterId, () => OnChapterSelected(chapterId));
            }

            ResizeVerticalContent(_chapterContent, _chapterViewPrefab.GetComponent<RectTransform>(), _chapters.Count, _chapterSpacing, _chapterPaddingTopBottom);
        }

        private void OnChapterSelected(string chapterId)
        {
            if (string.IsNullOrWhiteSpace(chapterId))
            {
                return;
            }

            _selectedChapterId = chapterId;
            RebuildLevelList(chapterId);

            var levelsInChapter = _levels
                .Where(l => string.Equals(l.chapterId, chapterId, StringComparison.Ordinal))
                .OrderBy(l => l.order)
                .ToList();

            if (levelsInChapter.Count > 0)
            {
                OnLevelSelected(levelsInChapter[0]);
            }
            else
            {
                _selectedLevel = null;
                RefreshLevelDetails();
            }
        }

        private void RebuildLevelList(string chapterId)
        {
            ClearSpawned(_spawnedLevelViews);
            if (_levelContent == null || _levelViewPrefab == null)
            {
                return;
            }

            var chapter = _chapters.FirstOrDefault(c => string.Equals(c.id, chapterId, StringComparison.Ordinal));
            var chapterOrder = chapter != null ? Math.Max(1, chapter.order) : 1;

            var levelsInChapter = _levels
                .Where(l => string.Equals(l.chapterId, chapterId, StringComparison.Ordinal))
                .OrderBy(l => l.order)
                .ToList();

            for (var i = 0; i < levelsInChapter.Count; i++)
            {
                var level = levelsInChapter[i];
                var state = ProgressionService?.GetLevelState(level.id);
                var isUnlocked = state == null || state.Value.IsUnlocked || ProgressionService.CanEnterLevel(level.id);
                var isCompleted = state != null && state.Value.IsCleared;
                var stage = DataManager.Instance?.LoadStage(level.stageId);

                var recommendLevel = stage?.difficulty?.recommendedLevel ?? stage?.requirements?.minLevel ?? 0;
                var enemyElements = ExtractEnemyElements(stage);

                var view = Instantiate(_levelViewPrefab, _levelContent);
                _spawnedLevelViews.Add(view.gameObject);

                var localLevel = level;
                var stageName = stage?.nameKey;
                view.Bind(
                    level.id,
                    chapterOrder,
                    Math.Max(1, localLevel.order),
                    stageName,
                    recommendLevel,
                    isUnlocked,
                        isCompleted,
                    enemyElements,
                    () => OnLevelSelected(localLevel));
            }

            ResizeVerticalContent(_levelContent, _levelViewPrefab.GetComponent<RectTransform>(), levelsInChapter.Count, _levelSpacing, _levelPaddingTopBottom);
        }

        private void OnLevelSelected(LevelDataModel level)
        {
            _selectedLevel = level;
            RefreshLevelDetails();
        }

        private void RefreshLevelDetails()
        {
            ClearSpawned(_spawnedEnemyViews);
            ClearSpawned(_spawnedRewardViews);

            if (_selectedLevel == null)
            {
                if (_detailLevelTitleText != null)
                {
                    _detailLevelTitleText.text = "-";
                }

                SetStarConditions(Array.Empty<string>());
                UpdateStarStates(0);
                ResizeHorizontalContent(_rewardContent, _rewardItemViewPrefab != null ? _rewardItemViewPrefab.GetComponent<RectTransform>() : null, 0, _rewardSpacing, _rewardPaddingLeftRight);
                return;
            }

            var stage = DataManager.Instance?.LoadStage(_selectedLevel.stageId);
            var chapter = _chapters.FirstOrDefault(c => string.Equals(c.id, _selectedLevel.chapterId, StringComparison.Ordinal));
            var chapterOrder = chapter != null ? Math.Max(1, chapter.order) : 1;
            var levelOrder = Math.Max(1, _selectedLevel.order);

            if (_detailLevelTitleText != null)
            {
                var levelName = stage?.nameKey ?? _selectedLevel.id;
                _detailLevelTitleText.text = $"Chapter {chapterOrder} - {levelOrder}: {levelName}";
            }

            RebuildEnemyDetails(stage);
            SetStarConditions(ExtractStarConditions(stage));

            var starCount = ProgressionService?.GetLevelState(_selectedLevel.id).Stars ?? 0;
            UpdateStarStates(starCount);

            RebuildRewardItems(stage);
            UpdateFightButtonState();
        }

        private void RebuildEnemyDetails(StageDataModel stage)
        {
            if (_enemyRoot == null || _enemyViewPrefab == null)
            {
                return;
            }

            var enemies = ExtractEnemyEntries(stage, 3);
            for (var i = 0; i < enemies.Count; i++)
            {
                var entry = enemies[i];
                var enemyData = DataManager.Instance?.LoadEnemy(entry.enemyId);

                var view = Instantiate(_enemyViewPrefab, _enemyRoot);
                _spawnedEnemyViews.Add(view.gameObject);

                var portrait = LoadEnemyPortrait(enemyData);
                view.Bind(
                    enemyData?.type,
                    Mathf.Max(1, entry.level),
                    enemyData?.element,
                    enemyData?.role,
                    portrait);
            }
        }

        private void RebuildRewardItems(StageDataModel stage)
        {
            if (_rewardContent == null || _rewardItemViewPrefab == null)
            {
                return;
            }

            var rewards = BuildRewardDisplayList(stage);
            for (var i = 0; i < rewards.Count; i++)
            {
                var reward = rewards[i];
                var view = Instantiate(_rewardItemViewPrefab, _rewardContent);
                _spawnedRewardViews.Add(view.gameObject);
                view.Bind(reward.Rarity, reward.Icon, reward.Label);
            }

            ResizeHorizontalContent(_rewardContent, _rewardItemViewPrefab.GetComponent<RectTransform>(), rewards.Count, _rewardSpacing, _rewardPaddingLeftRight);
        }

        private void UpdateFightButtonState()
        {
            if (_fightButton == null)
            {
                return;
            }

            if (_selectedLevel == null)
            {
                _fightButton.interactable = false;
                return;
            }

            _fightButton.interactable = ProgressionService == null || ProgressionService.CanEnterLevel(_selectedLevel.id);
        }

        private void OnFightClicked()
        {
            if (_selectedLevel == null)
            {
                return;
            }

            if (ProgressionService != null && !ProgressionService.CanEnterLevel(_selectedLevel.id))
            {
                return;
            }

            var lineup = MetaServiceHub.Instance?.TeamService?.GetCurrentLineup() ?? new List<string>();
           if(MetaServiceHub.Instance == null)
            {
                Debug.LogWarning("[LevelSelectUI] MetaServiceHub not available; cannot retrieve team lineup.");
            }
             else if (MetaServiceHub.Instance.TeamService == null)
            {
                Debug.LogWarning("[LevelSelectUI] TeamService not available; cannot retrieve team lineup.");
            }
             else if (lineup.Count == 0)
            {
                Debug.LogWarning("[LevelSelectUI] Team lineup is empty; combat may be difficult.");
            }
            else
            {
                Debug.Log($"[LevelSelectUI] Entering combat for level {_selectedLevel.id} with lineup: {string.Join(", ", lineup)}");
            }
            FlowController.Instance.EnterCombat(_selectedLevel.id, lineup);
        }

        private void BackToMainMenu()
        {
            FlowController.Instance.OpenMainMenu();
        }

        private static void ClearSpawned(List<GameObject> targets)
        {
            for (var i = 0; i < targets.Count; i++)
            {
                if (targets[i] != null)
                {
                    Destroy(targets[i]);
                }
            }

            targets.Clear();
        }

        private static void ResizeVerticalContent(RectTransform content, RectTransform prefabRect, int count, float spacing, float paddingTopBottom)
        {
            if (content == null)
            {
                return;
            }

            var itemHeight = prefabRect != null && prefabRect.rect.height > 0f ? prefabRect.rect.height : 200f;
            var effectiveCount = Mathf.Max(count, 0);
            var totalHeight = paddingTopBottom + effectiveCount * itemHeight + Mathf.Max(0, effectiveCount - 1) * spacing;
            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(totalHeight, 0f));
        }

        private static void ResizeHorizontalContent(RectTransform content, RectTransform prefabRect, int count, float spacing, float paddingLeftRight)
        {
            if (content == null)
            {
                return;
            }

            var itemWidth = prefabRect != null && prefabRect.rect.width > 0f ? prefabRect.rect.width : 140f;
            var effectiveCount = Mathf.Max(count, 0);
            var totalWidth = paddingLeftRight + effectiveCount * itemWidth + Mathf.Max(0, effectiveCount - 1) * spacing;
            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(totalWidth, 0f));
        }

        private static IReadOnlyList<string> ExtractEnemyElements(StageDataModel stage)
        {
            var result = new List<string>();
            if (stage?.encounters == null)
            {
                return result;
            }

            var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < stage.encounters.Count; i++)
            {
                var enemies = stage.encounters[i]?.enemies;
                if (enemies == null)
                {
                    continue;
                }

                for (var j = 0; j < enemies.Count; j++)
                {
                    var enemy = DataManager.Instance?.LoadEnemy(enemies[j].enemyId);
                    var element = string.IsNullOrWhiteSpace(enemy?.element) ? "physical" : enemy.element;
                    if (unique.Add(element))
                    {
                        result.Add(element);
                    }
                }
            }

            return result;
        }

        private static List<StageEnemy> ExtractEnemyEntries(StageDataModel stage, int maxCount)
        {
            var entries = new List<StageEnemy>();
            if (stage?.encounters == null)
            {
                return entries;
            }

            for (var i = 0; i < stage.encounters.Count; i++)
            {
                var enemies = stage.encounters[i]?.enemies;
                if (enemies == null)
                {
                    continue;
                }

                for (var j = 0; j < enemies.Count; j++)
                {
                    entries.Add(enemies[j]);
                    if (entries.Count >= maxCount)
                    {
                        return entries;
                    }
                }
            }

            return entries;
        }

        private static string[] ExtractStarConditions(StageDataModel stage)
        {
            var conditions = new string[3] { "-", "-", "-" };
            if (stage?.rewards?.stars == null)
            {
                return conditions;
            }

            for (var i = 0; i < stage.rewards.stars.Count && i < 3; i++)
            {
                var condition = stage.rewards.stars[i]?.condition;
                conditions[i] = string.IsNullOrWhiteSpace(condition) ? "-" : condition;
            }

            return conditions;
        }

        private void SetStarConditions(string[] conditions)
        {
            if (_starConditionText1 != null)
            {
                _starConditionText1.text = conditions.Length > 0 ? conditions[0] : "-";
            }

            if (_starConditionText2 != null)
            {
                _starConditionText2.text = conditions.Length > 1 ? conditions[1] : "-";
            }

            if (_starConditionText3 != null)
            {
                _starConditionText3.text = conditions.Length > 2 ? conditions[2] : "-";
            }
        }

        private void UpdateStarStates(int stars)
        {
            var unlockedSprite = Resources.Load<Sprite>(StarUnlockedPath);
            var lockedSprite = Resources.Load<Sprite>(StarLockedPath);

            SetStarImage(_starStateImage1, stars >= 1 ? unlockedSprite : lockedSprite);
            SetStarImage(_starStateImage2, stars >= 2 ? unlockedSprite : lockedSprite);
            SetStarImage(_starStateImage3, stars >= 3 ? unlockedSprite : lockedSprite);
        }

        private static void SetStarImage(Image target, Sprite sprite)
        {
            if (target == null)
            {
                return;
            }

            target.sprite = sprite;
            target.color = sprite == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
        }

        private static Sprite LoadEnemyPortrait(EnemyDataModel enemyData)
        {
            var portraitPath = enemyData?.visual?.portraitPath;
            if (string.IsNullOrWhiteSpace(portraitPath))
            {
                return null;
            }

            var viaDataManager = DataManager.Instance?.LoadCharacterPortraitSprite(portraitPath);
            if (viaDataManager != null)
            {
                return viaDataManager;
            }

            return LoadResourceSprite(portraitPath);
        }

        private static Sprite LoadResourceSprite(string rawPath)
        {
            if (string.IsNullOrWhiteSpace(rawPath))
            {
                return null;
            }

            var path = rawPath.Replace("\\", "/").Trim();
            if (path.StartsWith("Assets/Resources/", StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring("Assets/Resources/".Length);
            }
            else if (path.StartsWith("Resources/", StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring("Resources/".Length);
            }

            path = System.IO.Path.ChangeExtension(path, null)?.Replace("\\", "/") ?? path;
            return Resources.Load<Sprite>(path);
        }

        private static List<RewardDisplay> BuildRewardDisplayList(StageDataModel stage)
        {
            var displays = new List<RewardDisplay>();
            if (stage?.rewards?.firstClear == null)
            {
                return displays;
            }

            if (stage.rewards.firstClear.exp > 0)
            {
                displays.Add(new RewardDisplay
                {
                    Label = $"Exp x{stage.rewards.firstClear.exp}",
                    Icon = Resources.Load<Sprite>("UI/Reward/icon_exp"),
                    Rarity = "common"
                });
            }

            if (stage.rewards.firstClear.gold > 0)
            {
                displays.Add(new RewardDisplay
                {
                    Label = $"Gold x{stage.rewards.firstClear.gold}",
                    Icon = Resources.Load<Sprite>("UI/Reward/icon_gold"),
                    Rarity = "common"
                });
            }

            var items = stage.rewards.firstClear.items;
            if (items != null)
            {
                for (var i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    if (item == null || string.IsNullOrWhiteSpace(item.id))
                    {
                        continue;
                    }

                    var itemData = DataManager.Instance?.LoadItem(item.id);
                    displays.Add(new RewardDisplay
                    {
                        Label = $"{item.id} x{Mathf.Max(1, item.amount)}",
                        Icon = LoadResourceSprite(itemData?.iconPath),
                        Rarity = string.IsNullOrWhiteSpace(itemData?.rarity) ? "common" : itemData.rarity
                    });
                }
            }

            return displays;
        }

        private sealed class RewardDisplay
        {
            public string Label;
            public Sprite Icon;
            public string Rarity;
        }
    }
}

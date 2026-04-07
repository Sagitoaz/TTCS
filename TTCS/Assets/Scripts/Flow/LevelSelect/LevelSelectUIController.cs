using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TTCS.Core.Data;
using TTCS.Meta;
using TTCS.Meta.Progression;
using TTCS.Meta.Team;
using UnityEngine;
using UnityEngine.UI;

namespace TTCS.Flow.LevelSelect
{
    /// <summary>
    /// Day 6 - Level select controller with chapter tabs and level grid.
    /// Supports DataManager content and falls back to mock 3x3 data.
    /// </summary>
    public class LevelSelectUIController : MonoBehaviour
    {
        [Header("Chapter Tabs")]
        [SerializeField] private Transform _chapterTabRoot;
        [SerializeField] private GameObject _chapterTabPrefab;

        [Header("Level Grid")]
        [SerializeField] private Transform _levelGridRoot;
        [SerializeField] private GameObject _levelCardPrefab;

        [Header("Actions")]
        [SerializeField] private Button _backButton;

        public IProgressionService ProgressionService { get; set; }

        private readonly List<GameObject> _spawnedChapterTabs = new List<GameObject>();
        private readonly List<GameObject> _spawnedLevelCards = new List<GameObject>();
        private readonly List<ChapterDataModel> _chapters = new List<ChapterDataModel>();
        private string _selectedChapterId = "chapter_01";

        private void Start()
        {
            var hub = MetaServiceHub.Instance;
            hub?.EnsureInitialized();
            ProgressionService ??= hub?.ProgressionService;

            if (_backButton != null)
            {
                _backButton.onClick.AddListener(Back);
            }

            DisplayChapters();
        }

        public void DisplayChapters()
        {
            BuildChapterSource();
            SpawnChapterTabs();

            if (_chapters.Count == 0)
            {
                return;
            }

            var preferred = _chapters.FirstOrDefault(c => string.Equals(c.id, _selectedChapterId, StringComparison.Ordinal));
            OnChapterSelected(preferred?.id ?? _chapters[0].id);
        }

        public void OnChapterSelected(string chapterId)
        {
            if (string.IsNullOrWhiteSpace(chapterId))
            {
                return;
            }

            _selectedChapterId = chapterId;
            SpawnLevelCardsForChapter(chapterId);
        }

        public void OnLevelClicked(string levelId)
        {
            if (string.IsNullOrWhiteSpace(levelId))
            {
                return;
            }

            var canEnter = ProgressionService == null || ProgressionService.CanEnterLevel(levelId);
            if (!canEnter)
            {
                Debug.LogWarning($"[LevelSelectUI] Level locked: {levelId}");
                return;
            }

            var lineup = MetaServiceHub.Instance?.TeamService?.GetCurrentLineup() ?? new List<string>();
            FlowController.Instance.EnterCombat(levelId, lineup);
        }

        public void Back()
        {
            FlowController.Instance.OpenMainMenu();
        }

        private void BuildChapterSource()
        {
            _chapters.Clear();

            var loaded = DataManager.Instance?.GetAllChapters();
            if (loaded != null)
            {
                _chapters.AddRange(loaded.Where(c => c != null).OrderBy(c => c.order));
            }

            if (_chapters.Count > 0)
            {
                return;
            }

            for (var i = 1; i <= 3; i++)
            {
                _chapters.Add(new ChapterDataModel
                {
                    id = $"chapter_{i:D2}",
                    nameKey = $"Chapter {i}",
                    order = i
                });
            }
        }

        private void SpawnChapterTabs()
        {
            ClearSpawned(_spawnedChapterTabs);
            if (_chapterTabRoot == null || _chapterTabPrefab == null)
            {
                return;
            }

            for (var i = 0; i < _chapters.Count; i++)
            {
                var chapter = _chapters[i];
                var go = Instantiate(_chapterTabPrefab, _chapterTabRoot);
                _spawnedChapterTabs.Add(go);

                SetText(go, chapter.nameKey ?? chapter.id);
                var button = go.GetComponent<Button>();
                if (button != null)
                {
                    var chapterId = chapter.id;
                    button.onClick.AddListener(() => OnChapterSelected(chapterId));
                }
            }
        }

        private void SpawnLevelCardsForChapter(string chapterId)
        {
            ClearSpawned(_spawnedLevelCards);
            if (_levelGridRoot == null || _levelCardPrefab == null)
            {
                return;
            }

            var levels = BuildLevelsByChapter(chapterId);
            for (var i = 0; i < levels.Count; i++)
            {
                var levelId = levels[i];
                var go = Instantiate(_levelCardPrefab, _levelGridRoot);
                _spawnedLevelCards.Add(go);

                var state = ProgressionService?.GetLevelState(levelId);
                var label = state.HasValue
                    ? $"{levelId}\n{(state.Value.IsUnlocked ? "Unlocked" : "Locked")} | ★{state.Value.Stars}"
                    : $"{levelId}\nUnlocked";

                SetText(go, label);

                var button = go.GetComponent<Button>();
                if (button != null)
                {
                    button.interactable = !state.HasValue || state.Value.IsUnlocked || ProgressionService.CanEnterLevel(levelId);
                    button.onClick.AddListener(() => OnLevelClicked(levelId));
                }
            }
        }

        private static List<string> BuildLevelsByChapter(string chapterId)
        {
            var levels = new List<string>();
            var loaded = DataManager.Instance?.GetAllLevels();
            if (loaded != null)
            {
                levels.AddRange(
                    loaded
                        .Where(l => l != null && string.Equals(l.chapterId, chapterId, StringComparison.Ordinal))
                        .OrderBy(l => l.order)
                        .Select(l => l.id));
            }

            if (levels.Count > 0)
            {
                return levels;
            }

            for (var i = 1; i <= 3; i++)
            {
                levels.Add($"{chapterId}_level_{i:D2}");
            }

            return levels;
        }

        private static void SetText(GameObject root, string value)
        {
            if (root == null)
            {
                return;
            }

            var tmp = root.GetComponentInChildren<TMP_Text>();
            if (tmp != null)
            {
                tmp.text = value;
                return;
            }

            var legacy = root.GetComponentInChildren<Text>();
            if (legacy != null)
            {
                legacy.text = value;
            }
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
    }
}

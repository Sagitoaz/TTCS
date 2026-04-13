using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TTCS.Meta;
using TTCS.Meta.Progression;

namespace TTCS.Flow.LevelSelect
{
    /// <summary>
    /// Controls level select scene UI and logic.
    /// Dev B - Ngày 2-3 Sprint 03.
    /// Shows chapter list, level list, unlock state, and enter combat.
    /// </summary>
    public class LevelSelectController : MonoBehaviour
    {
        [SerializeField] private Transform _levelButtonContainer;
        [SerializeField] private GameObject _levelButtonPrefab;
        [SerializeField] private Button _backButton;

        private string _selectedChapterId = "chapter_01";
        private IProgressionService _progressionService;
        private UIStateManager _uiStateManager;

        private void Start()
        {
            _selectedChapterId = string.IsNullOrWhiteSpace(FlowRuntimeContext.SelectedChapterId)
                ? "chapter_01"
                : FlowRuntimeContext.SelectedChapterId;

            Debug.Log($"[LevelSelect] Level select scene loaded for chapter: {_selectedChapterId}");

            TryBindServices();

            _uiStateManager = GetComponent<UIStateManager>() ?? gameObject.AddComponent<UIStateManager>();

            WireButtons();
            StartCoroutine(DeferredPopulate());
        }

        private void TryBindServices()
        {
            var hub = MetaServiceHub.Instance;
            hub?.EnsureInitialized();
            _progressionService = hub?.ProgressionService;
        }

        private IEnumerator DeferredPopulate()
        {
            const int maxWaitFrames = 60;
            int waited = 0;

            while (_progressionService == null && waited < maxWaitFrames)
            {
                TryBindServices();
                if (_progressionService != null)
                {
                    break;
                }

                waited++;
                yield return null;
            }

            if (_progressionService == null)
            {
                Debug.LogWarning("[LevelSelect] ProgressionService not available after startup wait");
                yield break;
            }

            PopulateChapterLevels();
        }

        private void WireButtons()
        {
            if (_backButton != null)
                _backButton.onClick.AddListener(OnBackClicked);
        }

        private void PopulateChapterLevels()
        {
            ClearLevelButtons();

            // TODO: Load chapter data from JSON or DataManager
            // For now, create mock levels
            if (_progressionService == null)
            {
                return;
            }

            var chapterState = _progressionService.GetChapterState(_selectedChapterId);

            Debug.Log($"[LevelSelect] Populating levels for chapter {_selectedChapterId}: unlocked={chapterState.IsUnlocked}");

            if (!chapterState.IsUnlocked)
            {
                Debug.LogWarning($"[LevelSelect] Chapter locked: {_selectedChapterId}");
                return;
            }

            // Mock: Create 5 levels
            for (int i = 1; i <= 5; i++)
            {
                string levelId = $"{_selectedChapterId}_level_{i:D2}";
                var levelState = _progressionService.GetLevelState(levelId);

                CreateLevelButton(levelId, levelState, i);
            }
        }

        private void CreateLevelButton(string levelId, LevelState levelState, int displayNumber)
        {
            if (_levelButtonPrefab == null || _levelButtonContainer == null)
                return;

            var buttonGo = Instantiate(_levelButtonPrefab, _levelButtonContainer);
            var button = buttonGo.GetComponent<Button>();
            var text = buttonGo.GetComponentInChildren<Text>();

            if (button == null)
            {
                Debug.LogWarning("[LevelSelect] Level button prefab is missing Button component");
                Destroy(buttonGo);
                return;
            }

            if (text != null)
                text.text = $"Level {displayNumber}";

            // Update UI state (locked/unlocked/cleared)
            _uiStateManager.UpdateLevelButtonState(buttonGo, levelState);

            // Add click listener
            button.onClick.AddListener(() => OnLevelClicked(levelId, levelState));

            // Disable if locked
            if (_progressionService != null && !_progressionService.CanEnterLevel(levelId))
                button.interactable = false;
        }

        private void OnLevelClicked(string levelId, LevelState levelState)
        {
            if (_progressionService != null && !_progressionService.CanEnterLevel(levelId))
            {
                Debug.LogWarning($"[LevelSelect] Cannot enter locked level: {levelId}");
                return;
            }

            Debug.Log($"[LevelSelect] Level clicked: {levelId}");

            // Get current team lineup
            var teamService = MetaServiceHub.Instance?.TeamService;
            IReadOnlyList<string> lineup = teamService?.GetCurrentLineup() ?? new List<string>();

            // Enter combat
            FlowController.Instance.EnterCombat(levelId, lineup);
        }

        private void OnBackClicked()
        {
            Debug.Log("[LevelSelect] Back button clicked - returning to main menu");
            FlowController.Instance.OpenMainMenu();
        }

        private void ClearLevelButtons()
        {
            if (_levelButtonContainer == null)
            {
                return;
            }

            for (int i = _levelButtonContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_levelButtonContainer.GetChild(i).gameObject);
            }
        }
    }

    /// <summary>
    /// Manages UI state for level buttons (locked/unlocked/cleared).
    /// </summary>
    public class UIStateManager : MonoBehaviour
    {
        public void UpdateLevelButtonState(GameObject buttonGo, LevelState levelState)
        {
            var image = buttonGo.GetComponent<Image>();
            var label = buttonGo.GetComponentInChildren<Text>();

            if (image == null || label == null)
                return;

            if (!levelState.IsUnlocked)
            {
                image.color = new Color(0.5f, 0.5f, 0.5f); // Gray for locked
                label.text += " [LOCKED]";
            }
            else if (levelState.IsCleared)
            {
                image.color = new Color(0.7f, 1f, 0.7f); // Green for cleared
                label.text += $" [{levelState.Stars}*]";
            }
            else
            {
                image.color = Color.white; // Normal for available
            }
        }
    }
}

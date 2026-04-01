using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TTCS.Core;
using TTCS.Meta;

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
            Debug.Log($"[LevelSelect] Level select scene loaded for chapter: {_selectedChapterId}");

            // Get progression service
            _progressionService = FlowController.Instance.GetComponent<IProgressionService>();
            if (_progressionService == null)
            {
                Debug.LogWarning("[LevelSelect] ProgressionService not found, using mock");
                _progressionService = new MockProgressionService();
            }

            _uiStateManager = GetComponent<UIStateManager>() ?? gameObject.AddComponent<UIStateManager>();

            WireButtons();
            PopulateChapterLevels();
        }

        private void WireButtons()
        {
            if (_backButton != null)
                _backButton.onClick.AddListener(OnBackClicked);
        }

        private void PopulateChapterLevels()
        {
            // TODO: Load chapter data from JSON or DataManager
            // For now, create mock levels
            var chapterState = _progressionService.GetChapterState(_selectedChapterId);

            Debug.Log($"[LevelSelect] Populating levels for chapter {_selectedChapterId}: unlocked={chapterState.Unlocked}");

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

            if (text != null)
                text.text = $"Level {displayNumber}";

            // Update UI state (locked/unlocked/cleared)
            _uiStateManager.UpdateLevelButtonState(buttonGo, levelState);

            // Add click listener
            button.onClick.AddListener(() => OnLevelClicked(levelId, levelState));

            // Disable if locked
            if (!levelState.Unlocked)
                button.interactable = false;
        }

        private void OnLevelClicked(string levelId, LevelState levelState)
        {
            if (!levelState.Unlocked)
            {
                Debug.LogWarning($"[LevelSelect] Cannot enter locked level: {levelId}");
                return;
            }

            Debug.Log($"[LevelSelect] Level clicked: {levelId}");

            // Get current team lineup
            var teamService = FlowController.Instance.GetComponent<ITeamService>();
            IReadOnlyList<string> lineup = teamService?.GetCurrentLineup() ?? new List<string>();

            // Enter combat
            FlowController.Instance.EnterCombat(levelId, lineup);
        }

        private void OnBackClicked()
        {
            Debug.Log("[LevelSelect] Back button clicked - returning to main menu");
            FlowController.Instance.OpenMainMenu();
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
            if (image == null)
                return;

            if (!levelState.Unlocked)
            {
                image.color = new Color(0.5f, 0.5f, 0.5f); // Gray for locked
                buttonGo.GetComponentInChildren<Text>().text += " [LOCKED]";
            }
            else if (levelState.Cleared)
            {
                image.color = new Color(0.7f, 1f, 0.7f); // Green for cleared
                buttonGo.GetComponentInChildren<Text>().text += $" [{levelState.BestStars}★]";
            }
            else
            {
                image.color = Color.white; // Normal for available
            }
        }
    }
}

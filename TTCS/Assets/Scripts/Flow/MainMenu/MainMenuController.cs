using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TTCS.Core.Save;

namespace TTCS.Flow.MainMenu
{
    /// <summary>
    /// Controls main menu UI and navigation to Team/Gacha/Inventory/LevelSelect.
    /// Dev B - Ngày 2-3 Sprint 03.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Root Panels")]
        [SerializeField] private GameObject _startMenuRoot;
        [SerializeField] private GameObject _mainMenuRoot;

        [Header("Transition")]
        [SerializeField] private float _transitionDuration = 0.4f;

        [Header("Start Menu")]
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _tutorialButton;
        [SerializeField] private Button _startMenuSettingsButton;
        [SerializeField] private Button _exitGameButton;

        [SerializeField] private Button _playButton;
        [SerializeField] private Button _teamButton;
        [SerializeField] private Button _gachaButton;
        [SerializeField] private Button _inventoryButton;
        [SerializeField] private Button _characterCollectionButton;
        [SerializeField] private Button _settingsButton;

        [Header("Gold Display")]
        [SerializeField] private TextMeshProUGUI _goldText;

        [Header("Panels")]
        [SerializeField] private TutorialPanelSequenceController _tutorialPanelController;
        [SerializeField] private SettingsPanelController _settingsPanelController;

        private SaveManager _saveManager;
        private CanvasGroup _startMenuCanvasGroup;
        private CanvasGroup _mainMenuCanvasGroup;
        private Coroutine _transitionCoroutine;

        private void Start()
        {
            Debug.Log("[MainMenu] Main menu loaded");
            _saveManager = SaveManager.Instance;

            _startMenuCanvasGroup = GetOrAddCanvasGroup(_startMenuRoot);
            _mainMenuCanvasGroup = GetOrAddCanvasGroup(_mainMenuRoot);

            WireButtons();
            RefreshEntryState();

            if (ShouldShowStartMenuOnOpen())
            {
                ShowStartMenu();
                return;
            }

            ShowMainMenu();
        }

        private void OnEnable()
        {
            RefreshEntryState();
            RefreshGoldUI();
        }

        private void WireButtons()
        {
            if (_continueButton != null)
                _continueButton.onClick.AddListener(OnContinueClicked);

            if (_newGameButton != null)
                _newGameButton.onClick.AddListener(OnNewGameClicked);

            if (_tutorialButton != null)
                _tutorialButton.onClick.AddListener(OnTutorialClicked);

            if (_startMenuSettingsButton != null)
                _startMenuSettingsButton.onClick.AddListener(OnSettingsClicked);

            if (_exitGameButton != null)
                _exitGameButton.onClick.AddListener(OnExitGameClicked);

            if (_playButton != null)
                _playButton.onClick.AddListener(OnPlayClicked);

            if (_teamButton != null)
                _teamButton.onClick.AddListener(OnTeamClicked);

            if (_gachaButton != null)
                _gachaButton.onClick.AddListener(OnGachaClicked);

            if (_inventoryButton != null)
                _inventoryButton.onClick.AddListener(OnInventoryClicked);

            if (_characterCollectionButton != null)
                _characterCollectionButton.onClick.AddListener(OnCharacterCollectionClicked);

            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        private void OnContinueClicked()
        {
            _saveManager ??= SaveManager.Instance;
            if (_saveManager == null)
                return;

            if (!_saveManager.HasSaveData(SaveManager.PrimarySlotIndex))
            {
                // Chưa có save data → tạo mới như New Game
                Debug.Log("[MainMenu] Continue: no save data found, starting new game.");
                _saveManager.NewGame();
                _saveManager.Save(SaveManager.PrimarySlotIndex);
            }
            else
            {
                _saveManager.Load(SaveManager.PrimarySlotIndex);
            }

            TransitionToMainMenu();
        }

        private void OnNewGameClicked()
        {
            _saveManager ??= SaveManager.Instance;
            if (_saveManager == null)
            {
                return;
            }

            _saveManager.NewGame();
            _saveManager.Save(SaveManager.PrimarySlotIndex);
            TransitionToMainMenu();
        }

        private void OnTutorialClicked()
        {
            _tutorialPanelController?.Open();
        }

        private void OnPlayClicked()
        {
            EnsureSessionReady();
            Debug.Log("[MainMenu] Play button clicked - opening level select");
            FlowController.Instance.OpenLevelSelect("chapter_01");
        }

        private void OnTeamClicked()
        {
            EnsureSessionReady();
            Debug.Log("[MainMenu] Team button clicked");
            FlowController.Instance.OpenTeamSelection();
        }

        private void OnGachaClicked()
        {
            EnsureSessionReady();
            Debug.Log("[MainMenu] Gacha button clicked");
            FlowController.Instance.OpenGacha();
        }

        private void OnInventoryClicked()
        {
            EnsureSessionReady();
            Debug.Log("[MainMenu] Inventory button clicked");
            FlowController.Instance.OpenInventory();
        }

        private void OnCharacterCollectionClicked()
        {
            EnsureSessionReady();
            Debug.Log("[MainMenu] Character Collection button clicked");
            FlowController.Instance.OpenCharacterCollection();
        }

        private void OnSettingsClicked()
        {
            Debug.Log("[MainMenu] Settings button clicked");
            _settingsPanelController?.Open();
        }

        private void OnExitGameClicked()
        {
            Debug.Log("[MainMenu] Exit Game clicked");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void RefreshGoldUI()
        {
            if (_saveManager == null)
            {
                _saveManager = SaveManager.Instance;
            }

            if (_goldText != null)
            {
                _goldText.text = $"Gold: {(_saveManager?.CurrentSave?.gold ?? 0):N0}";
            }
        }

        private void RefreshEntryState()
        {
            if (_saveManager == null)
            {
                _saveManager = SaveManager.Instance;
            }

            if (_continueButton != null)
            {
                _continueButton.interactable = _saveManager != null && _saveManager.HasSaveData(SaveManager.PrimarySlotIndex);
            }
        }

        private bool ShouldShowStartMenuOnOpen()
        {
            if (_startMenuRoot == null)
            {
                return false;
            }

            return _saveManager == null ||
                   _saveManager.CurrentSave == null ||
                   _saveManager.ActiveSlotIndex != SaveManager.PrimarySlotIndex;
        }

        private void ShowStartMenu()
        {
            SetRootActive(_startMenuRoot, true);
            SetRootActive(_mainMenuRoot, false);
            _settingsPanelController?.Close();

            if (_startMenuCanvasGroup != null)
            {
                _startMenuCanvasGroup.alpha = 1f;
                _startMenuCanvasGroup.interactable = true;
                _startMenuCanvasGroup.blocksRaycasts = true;
            }
            if (_mainMenuCanvasGroup != null)
            {
                _mainMenuCanvasGroup.alpha = 0f;
                _mainMenuCanvasGroup.interactable = false;
                _mainMenuCanvasGroup.blocksRaycasts = false;
            }
        }

        private void ShowMainMenu()
        {
            EnsureSessionReady();
            SetRootActive(_startMenuRoot, false);
            SetRootActive(_mainMenuRoot, true);
            RefreshGoldUI();
            RefreshEntryState();

            if (_startMenuCanvasGroup != null)
            {
                _startMenuCanvasGroup.alpha = 0f;
                _startMenuCanvasGroup.interactable = false;
                _startMenuCanvasGroup.blocksRaycasts = false;
            }
            if (_mainMenuCanvasGroup != null)
            {
                _mainMenuCanvasGroup.alpha = 1f;
                _mainMenuCanvasGroup.interactable = true;
                _mainMenuCanvasGroup.blocksRaycasts = true;
            }
        }

        /// <summary>
        /// Fades out the Start Menu then fades in the Main Menu.
        /// </summary>
        private void TransitionToMainMenu()
        {
            EnsureSessionReady();
            RefreshGoldUI();
            RefreshEntryState();

            if (_transitionCoroutine != null)
                StopCoroutine(_transitionCoroutine);

            _transitionCoroutine = StartCoroutine(CrossFadeToMainMenu());
        }

        private IEnumerator CrossFadeToMainMenu()
        {
            // Make sure both panels are visible so they can be faded
            SetRootActive(_startMenuRoot, true);
            SetRootActive(_mainMenuRoot, true);

            // Set starting alpha states
            if (_startMenuCanvasGroup != null)
            {
                _startMenuCanvasGroup.alpha = 1f;
                _startMenuCanvasGroup.interactable = false;
                _startMenuCanvasGroup.blocksRaycasts = false;
            }
            if (_mainMenuCanvasGroup != null)
            {
                _mainMenuCanvasGroup.alpha = 0f;
                _mainMenuCanvasGroup.interactable = false;
                _mainMenuCanvasGroup.blocksRaycasts = false;
            }

            // Cross-fade
            float elapsed = 0f;
            float duration = Mathf.Max(0.01f, _transitionDuration);
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                if (_startMenuCanvasGroup != null)
                    _startMenuCanvasGroup.alpha = 1f - t;
                if (_mainMenuCanvasGroup != null)
                    _mainMenuCanvasGroup.alpha = t;

                yield return null;
            }

            // Finalise
            SetRootActive(_startMenuRoot, false);

            if (_startMenuCanvasGroup != null)
            {
                _startMenuCanvasGroup.alpha = 0f;
                _startMenuCanvasGroup.interactable = false;
                _startMenuCanvasGroup.blocksRaycasts = false;
            }
            if (_mainMenuCanvasGroup != null)
            {
                _mainMenuCanvasGroup.alpha = 1f;
                _mainMenuCanvasGroup.interactable = true;
                _mainMenuCanvasGroup.blocksRaycasts = true;
            }

            _settingsPanelController?.Close();
            _transitionCoroutine = null;
        }

        private void EnsureSessionReady()
        {
            _saveManager ??= SaveManager.Instance;
            _saveManager?.EnsureCurrentSave(SaveManager.PrimarySlotIndex);
        }

        private static void SetRootActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }

        private static CanvasGroup GetOrAddCanvasGroup(GameObject target)
        {
            if (target == null) return null;
            var cg = target.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = target.AddComponent<CanvasGroup>();
            return cg;
        }
    }
}

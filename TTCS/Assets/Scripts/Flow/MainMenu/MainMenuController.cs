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


        [Header("Start Menu Buttons")]
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _tutorialButton;
        [SerializeField] private Button _startMenuSettingsButton;
        [SerializeField] private Button _exitGameButton;

        [Header("Main Menu Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _teamButton;
        [SerializeField] private Button _gachaButton;
        [SerializeField] private Button _inventoryButton;
        [SerializeField] private Button _characterCollectionButton;
        [SerializeField] private Button _settingsButton;       // Settings trong Start Menu (cũ)
        [SerializeField] private Button _mainMenuSettingsButton; // Settings trong Main Menu (mới)


        [Header("Gold Display")]
        [SerializeField] private TextMeshProUGUI _goldText;

        [Header("Panels")]
        [SerializeField] private TutorialPanelSequenceController _tutorialPanelController;
        [SerializeField] private SettingsPanelController _settingsPanelController;

        private SaveManager _saveManager;

        // Track panel caller để Settings biết quay về đâu khi đóng
        private enum CallerPanel { None, StartMenu, MainMenu }
        private CallerPanel _settingsCaller;

        private void Start()
        {
            Debug.Log("[MainMenu] Main menu loaded");
            _saveManager = SaveManager.Instance;

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
                _startMenuSettingsButton.onClick.AddListener(OnSettingsFromStartMenu);

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

            // Nút Settings cũ trong Start Menu (nếu còn dùng)
            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(OnSettingsFromStartMenu);

            // Nút Settings mới trong Main Menu
            if (_mainMenuSettingsButton != null)
                _mainMenuSettingsButton.onClick.AddListener(OnSettingsFromMainMenu);
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

        private void OnSettingsFromStartMenu()
        {
            Debug.Log("[MainMenu] Settings opened from Start Menu");
            _settingsCaller = CallerPanel.StartMenu;
            // Ẩn Start Menu khi Settings mở để tránh UI chồng chéo
            SetRootActive(_startMenuRoot, false);
            _settingsPanelController?.Open(OnSettingsClosed);
        }

        private void OnSettingsFromMainMenu()
        {
            Debug.Log("[MainMenu] Settings opened from Main Menu");
            _settingsCaller = CallerPanel.MainMenu;
            // Ẩn Main Menu khi Settings mở
            SetRootActive(_mainMenuRoot, false);
            _settingsPanelController?.Open(OnSettingsClosed);
        }

        private void OnSettingsClosed()
        {
            Debug.Log($"[MainMenu] Settings closed, returning to: {_settingsCaller}");
            switch (_settingsCaller)
            {
                case CallerPanel.StartMenu:
                    SetRootActive(_startMenuRoot, true);
                    break;
                case CallerPanel.MainMenu:
                    SetRootActive(_mainMenuRoot, true);
                    break;
            }
            _settingsCaller = CallerPanel.None;
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
        }

        private void ShowMainMenu()
        {
            EnsureSessionReady();
            SetRootActive(_startMenuRoot, false);
            SetRootActive(_mainMenuRoot, true);
            RefreshGoldUI();
            RefreshEntryState();
        }

        /// <summary>
        /// Switches to the Main Menu panel using the SceneTransitionController iris-wipe.
        /// </summary>
        private void TransitionToMainMenu()
        {
            EnsureSessionReady();
            RefreshGoldUI();
            RefreshEntryState();

            var stc = FlowController.Instance?.GetComponent<SceneTransitionController>();
            if (stc != null)
            {
                stc.PlayIrisTransition(ShowMainMenu);
            }
            else
            {
                // Fallback: no transition controller found, switch instantly.
                ShowMainMenu();
            }
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
    }
}

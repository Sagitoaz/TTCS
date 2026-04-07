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
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _teamButton;
        [SerializeField] private Button _gachaButton;
        [SerializeField] private Button _inventoryButton;
        [SerializeField] private Button _characterCollectionButton;
        [SerializeField] private Button _settingsButton;

        [Header("Gold Display")]
        [SerializeField] private TextMeshProUGUI _goldText;

        private SaveManager _saveManager;

        private void Start()
        {
            Debug.Log("[MainMenu] Main menu loaded");
            _saveManager = SaveManager.Instance;
            _saveManager?.EnsureCurrentSave(0);
            WireButtons();
            RefreshGoldUI();
        }

        private void OnEnable()
        {
            RefreshGoldUI();
        }

        private void WireButtons()
        {
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

        private void OnPlayClicked()
        {
            Debug.Log("[MainMenu] Play button clicked - opening level select");
            FlowController.Instance.OpenLevelSelect("chapter_01");
        }

        private void OnTeamClicked()
        {
            Debug.Log("[MainMenu] Team button clicked");
            FlowController.Instance.OpenTeamSelection();
        }

        private void OnGachaClicked()
        {
            Debug.Log("[MainMenu] Gacha button clicked");
            FlowController.Instance.OpenGacha();
        }

        private void OnInventoryClicked()
        {
            Debug.Log("[MainMenu] Inventory button clicked");
            FlowController.Instance.OpenInventory();
        }

        private void OnCharacterCollectionClicked()
        {
            Debug.Log("[MainMenu] Character Collection button clicked");
            FlowController.Instance.OpenCharacterCollection();
        }

        private void OnSettingsClicked()
        {
            Debug.Log("[MainMenu] Settings button clicked");
            // TODO: Open SettingsPanel or SettingsScene
        }

        private void RefreshGoldUI()
        {
            if (_saveManager == null)
            {
                _saveManager = SaveManager.Instance;
                _saveManager?.EnsureCurrentSave(0);
            }

            if (_goldText != null)
            {
                _goldText.text = $"Gold: {(_saveManager?.CurrentSave?.gold ?? 0):N0}";
            }
        }
    }
}

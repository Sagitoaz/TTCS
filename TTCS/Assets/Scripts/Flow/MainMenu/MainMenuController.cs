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
        [SerializeField] private Button _settingsButton;

        [Header("Gold Display")]
        [SerializeField] private TextMeshProUGUI _goldText;
        [SerializeField] private Image _goldIcon;

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

            if (_goldIcon != null)
            {
                var icon = LoadGoldIconSprite();
                _goldIcon.sprite = icon;
                _goldIcon.color = icon == null ? new Color(1f, 0.85f, 0f, 1f) : Color.white;
            }
        }

        private static Sprite LoadGoldIconSprite()
        {
            var candidates = new[]
            {
                "UI/icon_gold",
                "Icons/icon_gold",
                "Icons/UI/icon_gold",
                "Sprites/UI/icon_gold",
                "Sprites/Items/item_gold",
                "Items/item_gold",
                "item_gold"
            };

            for (var i = 0; i < candidates.Length; i++)
            {
                var sprite = Resources.Load<Sprite>(candidates[i]);
                if (sprite != null)
                {
                    return sprite;
                }
            }

            // Fallback vàng 1x1 để luôn có icon hiển thị khi chưa có asset chuẩn.
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, new Color(1f, 0.84f, 0f, 1f));
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }
    }
}

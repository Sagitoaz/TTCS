using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TTCS.Flow.Inventory
{
    /// <summary>
    /// Visual cell for inventory square prefab.
    /// Displays icon, quantity and rarity-colored border.
    /// </summary>
    public sealed class InventoryItemCellView : MonoBehaviour
    {
        [SerializeField] private Button _button;

        [SerializeField] private Image _SelectedEffect;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _quantityText;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _borderImage;

        public string ItemId { get; private set; }

        public void Bind(Sprite icon, int quantity, string rarity, string itemId, Action onClick)
        {
            ItemId = itemId;

            if (_iconImage != null)
            {
                _iconImage.sprite = icon;
                _iconImage.color = icon == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
                _iconImage.preserveAspect = true;
            }

            if (_quantityText != null)
            {
                _quantityText.text = $"x{Mathf.Max(0, quantity)}";
            }

            if (_backgroundImage != null)
            {
                _backgroundImage.sprite = LoadRarityBackgroundSprite(rarity);
                _backgroundImage.color = Color.white;
            }

            if (_borderImage != null)
            {
                _borderImage.sprite = LoadRarityBorderSprite(rarity);
                _borderImage.color = Color.white;
            }

            SetSelected(false);

            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                if (onClick != null)
                {
                    _button.onClick.AddListener(() => { onClick(); });
                }
            }
        }

        public void SetSelected(bool isSelected)
        {
            if (_SelectedEffect != null)
            {
                _SelectedEffect.gameObject.SetActive(isSelected);
            }
        }

        private static Sprite LoadRarityBorderSprite(string rarity)
        {
            var key = NormalizeRarityKey(rarity);
            var sprite = Resources.Load<Sprite>($"UI/Border/{key}_border");
            return sprite ?? Resources.Load<Sprite>("UI/Border/common_border");
        }

        private static Sprite LoadRarityBackgroundSprite(string rarity)
        {
            var key = NormalizeRarityKey(rarity);
            var sprite = Resources.Load<Sprite>($"UI/Border/{key}_bg");
            return sprite ?? Resources.Load<Sprite>("UI/Border/common_bg");
        }

        private static string NormalizeRarityKey(string rarity)
        {
            if (string.IsNullOrWhiteSpace(rarity))
            {
                return "common";
            }

            var key = rarity.Trim().ToLowerInvariant();
            return key switch
            {
                "ssr" => "ssr",
                "sr" => "sr",
                "r" => "r",
                "ur" => "ur",
                _ => "common"
            };
        }
    }
}

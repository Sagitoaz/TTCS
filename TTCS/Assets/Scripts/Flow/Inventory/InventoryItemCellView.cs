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
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _quantityText;
        [SerializeField] private Image _borderImage;

        public void Bind(Sprite icon, int quantity, Color borderColor, Action onClick)
        {
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

            if (_borderImage != null)
            {
                _borderImage.color = borderColor;
            }

            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                if (onClick != null)
                {
                    _button.onClick.AddListener(() => onClick());
                }
            }
        }
    }
}

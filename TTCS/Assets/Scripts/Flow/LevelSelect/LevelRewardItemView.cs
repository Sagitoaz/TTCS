using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TTCS.Flow.LevelSelect
{
    public class LevelRewardItemView : MonoBehaviour
    {
        [SerializeField] private Image _borderImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _itemIconImage;
        [SerializeField] private TMP_Text _quantityText;

        public void Bind(string rarity, Sprite itemIcon, string quantityLabel)
        {
            var normalizedRarity = NormalizeRarity(rarity);

            if (_borderImage != null)
            {
                var border = Resources.Load<Sprite>($"UI/Border/{normalizedRarity}_border");
                _borderImage.sprite = border;
                _borderImage.color = border == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
            }

            if (_backgroundImage != null)
            {
                var bg = Resources.Load<Sprite>($"UI/Border/{normalizedRarity}_bg");
                _backgroundImage.sprite = bg;
                _backgroundImage.color = bg == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
            }

            if (_itemIconImage != null)
            {
                _itemIconImage.sprite = itemIcon;
                _itemIconImage.color = itemIcon == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
            }

            if (_quantityText != null)
            {
                _quantityText.text = quantityLabel;
            }
        }

        private static string NormalizeRarity(string rarity)
        {
            if (string.IsNullOrWhiteSpace(rarity))
            {
                return "common";
            }

            var normalized = rarity.Trim().ToLowerInvariant();
            return normalized switch
            {
                "ssr" => "ssr",
                "sr" => "sr",
                "r" => "r",
                "common" => "common",
                _ => "common"
            };
        }
    }
}

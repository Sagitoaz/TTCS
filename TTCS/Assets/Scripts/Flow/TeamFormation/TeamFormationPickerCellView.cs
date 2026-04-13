using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TTCS.Flow.TeamFormation
{
    public sealed class TeamFormationPickerCellView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _portrait;
        [SerializeField] private Image _borderImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _rarityText;
        [SerializeField] private GameObject _highlight;

        public Button Button => _button;

        public void Bind(string nameText, int level, string rarity, Sprite portrait)
        {
            if (_nameText != null)
            {
                _nameText.text = nameText;
            }

            if (_levelText != null)
            {
                _levelText.text = $"Lv.{level}";
            }

            if (_rarityText != null)
            {
                _rarityText.text = rarity;
                ApplyRarityColor(rarity);
            }

            if (_portrait != null)
            {
                _portrait.sprite = portrait;
                _portrait.color = portrait == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
                _portrait.preserveAspect = true;
            }

            if (_borderImage != null)
            {
                var borderSprite = LoadBorderImage(rarity);
                _borderImage.sprite = borderSprite;
                _borderImage.color = borderSprite == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
            }
        }

        private void ApplyRarityColor(string rarity)
        {
            if (_rarityText == null) return;

            if(string.Equals(rarity, "UR", System.StringComparison.OrdinalIgnoreCase)){
                var topColor = HexToColor("#E61919");
                var bottomColor = HexToColor("#3D0000");
                _rarityText.color = topColor;
                _rarityText.enableVertexGradient = true;
                _rarityText.colorGradient = new VertexGradient(topColor, topColor, bottomColor, bottomColor);
                return;
            }

            else if (string.Equals(rarity, "SSR", System.StringComparison.OrdinalIgnoreCase))
            {
                // SSR: Gradient color - top corners #FFFFB3FF, bottom corners #FFB300FF
                Color topColor = HexToColor("#FFFFB3FF");
                Color bottomColor = HexToColor("#FFB300FF");
                _rarityText.color = HexToColor("#FFD700");

                var gradient = new VertexGradient(topColor, topColor, bottomColor, bottomColor);
                _rarityText.colorGradient = gradient;
            }
            else if (string.Equals(rarity, "SR", System.StringComparison.OrdinalIgnoreCase))
            {
                // SR: solid color #FF007F
                _rarityText.color = HexToColor("#FF007F");
            }
            else if (string.Equals(rarity, "R", System.StringComparison.OrdinalIgnoreCase))
            {
                // R: solid color #00F0FF
                _rarityText.color = HexToColor("#00F0FF");
            }
            else
            {
                // Default: white
                _rarityText.color = Color.white;
            }
        }

        private static Color HexToColor(string hex)
        {
            hex = hex.Replace("#", "");
            if (hex.Length == 6)
            {
                hex += "FF"; // Add alpha if not present
            }

            if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int result))
            {
                byte r = (byte)((result >> 24) & 0xFF);
                byte g = (byte)((result >> 16) & 0xFF);
                byte b = (byte)((result >> 8) & 0xFF);
                byte a = (byte)(result & 0xFF);
                return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
            }

            return Color.white;
        }

        private static Sprite LoadBorderImage(string rarity)
        {
            if (string.IsNullOrWhiteSpace(rarity))
            {
                return null;
            }

            var normalized = rarity.ToLowerInvariant();
            var borderPath = $"UI/Border/{normalized}_border";
            return Resources.Load<Sprite>(borderPath);
        }


        public void SetSelected(bool selected)
        {
            if (_highlight != null)
            {
                _highlight.SetActive(selected);
            }
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TTCS.Flow.LevelSelect
{
    public class LevelEnemyView : MonoBehaviour
    {
        [SerializeField] private Image _borderImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private TMP_Text _enemyLevelText;
        [SerializeField] private Image _elementIconImage;
        [SerializeField] private Image _roleIconImage;
        [SerializeField] private Image _portraitImage;

        public void Bind(string enemyType, int enemyLevel, string element, string role, Sprite portrait)
        {
            if (_borderImage != null)
            {
                var border = Resources.Load<Sprite>($"UI/Enemy/enemy_border_{NormalizeEnemyType(enemyType)}");
                _borderImage.sprite = border;
                _borderImage.color = border == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
            }

            if (_backgroundImage != null)
            {
                var background = Resources.Load<Sprite>($"UI/Enemy/enemy_bg_{NormalizeEnemyType(enemyType)}");
                _backgroundImage.sprite = background;
                _backgroundImage.color = background == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
            }

            if (_enemyLevelText != null)
            {
                _enemyLevelText.text = $"Lv.{Mathf.Max(1, enemyLevel)}";
            }

            if (_elementIconImage != null)
            {
                var elementSprite = Resources.Load<Sprite>($"UI/Element/icon_element_{NormalizeElement(element)}");
                _elementIconImage.sprite = elementSprite;
                _elementIconImage.color = elementSprite == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
            }

            if (_roleIconImage != null)
            {
                var roleSprite = Resources.Load<Sprite>($"UI/Role/icon_role_{NormalizeRole(role)}");
                _roleIconImage.sprite = roleSprite;
                _roleIconImage.color = roleSprite == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
            }

            if (_portraitImage != null)
            {
                _portraitImage.sprite = portrait;
                _portraitImage.color = portrait == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
            }
        }

        private static string NormalizeEnemyType(string enemyType)
        {
            if (string.IsNullOrWhiteSpace(enemyType))
            {
                return "common";
            }

            var normalized = enemyType.Trim().ToLowerInvariant();
            return normalized switch
            {
                "boss" => "boss",
                "elite" => "elite",
                _ => "common"
            };
        }

        private static string NormalizeElement(string element)
        {
            if (string.IsNullOrWhiteSpace(element))
            {
                return "physical";
            }

            var normalized = element.Trim().ToLowerInvariant();
            if (normalized == "neutral")
            {
                return "physical";
            }

            return normalized;
        }

        private static string NormalizeRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return "warrior";
            }

            var normalized = role.Trim().ToLowerInvariant();
            return normalized switch
            {
                "support" => "support",
                "tank" => "tank",
                "mage" => "mage",
                "bow" => "mage",
                _ => "warrior"
            };
        }
    }
}

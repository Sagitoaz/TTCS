using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TTCS.Flow.TeamFormation
{
    public sealed class TeamFormationSkillQuickItemView : MonoBehaviour
    {
        [SerializeField] private Image _skillIcon;
        [SerializeField] private TextMeshProUGUI _skillNameText;

        public void Bind(Sprite icon, string skillName)
        {
            if (_skillIcon != null)
            {
                _skillIcon.sprite = icon;
                _skillIcon.color = icon == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
            }

            if (_skillNameText != null)
            {
                _skillNameText.text = string.IsNullOrWhiteSpace(skillName) ? "Unknown Skill" : skillName;
            }
        }
    }
}

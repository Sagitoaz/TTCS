using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace TTCS.Flow.TeamFormation
{
    public sealed class TeamFormationSkillQuickItemView : MonoBehaviour
    {
        [SerializeField] private Image _skillIcon;
        [SerializeField] private TextMeshProUGUI _skillNameText;
        private Button _button;
        private string _skillId;
        private string _skillDescription;
        public Action<string, string, Vector3> OnSkillDetailClicked { get; set; }

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_button == null)
                _button = gameObject.AddComponent<Button>();

            _button.onClick.AddListener(OnSkillClicked);
        }

        public void Bind(Sprite icon, string skillName, string skillId = "", string skillDescription = "")
        {
            _skillId = skillId;
            _skillDescription = skillDescription;

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

        private void OnSkillClicked()
        {
            if (!string.IsNullOrWhiteSpace(_skillId))
            {
                OnSkillDetailClicked?.Invoke(_skillId, _skillDescription, transform.position);
            }
        }
    }
}

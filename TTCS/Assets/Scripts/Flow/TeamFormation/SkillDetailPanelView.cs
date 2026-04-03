using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TTCS.Flow.TeamFormation
{
    /// <summary>
    /// Chi tiết panel hiển thị skill name + description khi click skill item.
    /// </summary>
    public class SkillDetailPanelView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI skillNameText;
        [SerializeField] private TextMeshProUGUI skillDescriptionText;
        [SerializeField] private TextMeshProUGUI skillDamageText;
        [SerializeField] private TextMeshProUGUI skillCooldownText;
        [SerializeField] private TextMeshProUGUI skillManaText;
        [SerializeField] private CanvasGroup canvasGroup; // Để fade in/out
        [SerializeField] private RectTransform panelRect;
        [SerializeField] private float verticalOffset = 120f;

        private void Awake()
        {
            if (panelRect == null)
            {
                panelRect = GetComponent<RectTransform>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        private void Start()
        {
            // Init: hide
            Hide();
        }

        public void ShowSkillDetail(
            string skillName,
            string skillDescription,
            string damageText,
            string cooldownText,
            string manaText,
            Vector3 targetPosition)
        {
            if (skillNameText != null)
                skillNameText.text = skillName;

            if (skillDescriptionText != null)
                skillDescriptionText.text = skillDescription;

            if (skillDamageText != null)
                skillDamageText.text = damageText;

            if (skillCooldownText != null)
                skillCooldownText.text = cooldownText;

            if (skillManaText != null)
                skillManaText.text = manaText;

            // Position phía trên icon skill vừa click
            if (panelRect != null && targetPosition != Vector3.zero)
            {
                panelRect.position = targetPosition + Vector3.up * verticalOffset;
            }

            Show();
        }

        public void Hide()
        {
            if (canvasGroup != null)
                canvasGroup.alpha = 0;

            gameObject.SetActive(false);
        }

        private void Show()
        {
            gameObject.SetActive(true);
            if (canvasGroup != null)
                canvasGroup.alpha = 1;
        }

        public void Clear()
        {
            if (skillNameText != null)
                skillNameText.text = "";

            if (skillDescriptionText != null)
                skillDescriptionText.text = "";

            if (skillDamageText != null)
                skillDamageText.text = "";

            if (skillCooldownText != null)
                skillCooldownText.text = "";

            if (skillManaText != null)
                skillManaText.text = "";
        }
    }
}

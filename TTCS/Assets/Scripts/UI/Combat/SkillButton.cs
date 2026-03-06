using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TTCS.UI.Combat
{
    /// <summary>
    /// 🔵 Dev A - Skill Button
    /// Một nút skill: icon, cooldown overlay, mana cost.
    /// Được quản lý bởi SkillButtonPanel.
    /// </summary>
    public class SkillButton : MonoBehaviour
    {
        // ─── Inspector ────────────────────────────────────────────────────
        [SerializeField] private Button          _button;
        [SerializeField] private Image           _icon;
        [SerializeField] private Image           _cooldownOverlay;   // fillAmount 0→1 = ready→on cooldown
        [SerializeField] private TextMeshProUGUI _manaCostText;
        [SerializeField] private TextMeshProUGUI _cooldownText;      // hiển thị số lượt cooldown còn lại
        [SerializeField] private CanvasGroup     _canvasGroup;

        // ─── State ────────────────────────────────────────────────────────
        private string _skillId;
        private SkillButtonPanel _panel;

        // ─── Colors ───────────────────────────────────────────────────────
        private static readonly Color _disabledAlpha = new Color(1f, 1f, 1f, 0.4f);
        private static readonly Color _enabledAlpha  = Color.white;

        // ──────────────────────────────────────────────────────────────────
        #region Initialization

        /// <summary>
        /// Setup button với skill data. Gọi một lần khi panel initialize.
        /// </summary>
        public void Setup(string skillId, string skillName, int manaCost, SkillButtonPanel panel)
        {
            _skillId = skillId;
            _panel   = panel;

            _manaCostText.text = manaCost > 0 ? manaCost.ToString() : "—";
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnClick);

            gameObject.SetActive(true);
        }

        /// <summary>Ẩn slot khi character có ít hơn 4 skills.</summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Refresh

        /// <summary>
        /// Cập nhật trạng thái button dựa trên cooldown và mana.
        /// Gọi mỗi lần bắt đầu player turn.
        /// </summary>
        /// <param name="canUse">True nếu đủ mana và không on cooldown</param>
        /// <param name="cooldownRemaining">Số lượt cooldown còn lại (0 = sẵn sàng)</param>
        /// <param name="maxCooldown">Cooldown tối đa của skill (để tính fill ratio; mặc định 1)</param>
        public void Refresh(bool canUse, int cooldownRemaining, int maxCooldown = 1)
        {
            _button.interactable = canUse;

            // Icon dim khi không dùng được
            _icon.color = canUse ? _enabledAlpha : _disabledAlpha;

            // BUG-4 FIX: set fillAmount theo % cooldown còn lại
            if (cooldownRemaining > 0)
            {
                _cooldownOverlay.gameObject.SetActive(true);
                _cooldownOverlay.fillAmount = maxCooldown > 0
                    ? (float)cooldownRemaining / maxCooldown
                    : 1f;
                _cooldownText.text = cooldownRemaining.ToString();
            }
            else
            {
                _cooldownOverlay.gameObject.SetActive(false);
                _cooldownOverlay.fillAmount = 0f;
                _cooldownText.text = string.Empty;
            }
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Click Handler

        private void OnClick()
        {
            if (string.IsNullOrEmpty(_skillId) || _panel == null) return;

            // Scale bounce feedback
            transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 5, 0.5f);
            _panel.OnSkillSelected(_skillId);
        }

        #endregion
    }
}

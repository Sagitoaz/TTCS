using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TTCS.UI.Combat
{
    /// <summary>
    /// 🔵 Dev A - Turn Order Slot
    /// Một ô trong hàng turn order — hiển thị icon + tên entity.
    /// Pooled bởi TurnOrderDisplay.
    /// </summary>
    public class TurnOrderSlot : MonoBehaviour
    {
        // ─── Inspector ────────────────────────────────────────────────────
        [SerializeField] private Image          _portrait;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Image          _highlightOutline;

        // ─── Colors ───────────────────────────────────────────────────────
        private static readonly Color _highlightColor = new Color(1f, 0.85f, 0.1f, 1f); // Gold
        private static readonly Color _normalColor    = Color.clear;

        // ──────────────────────────────────────────────────────────────────
        #region Public API

        /// <summary>
        /// Set dữ liệu cho slot.
        /// </summary>
        /// <param name="entityId">ID entity (dùng để load icon sau nếu cần)</param>
        /// <param name="displayName">Tên hiển thị</param>
        /// <param name="isPlayer">True = ally tint xanh, False = enemy tint đỏ nhẹ</param>
        /// <param name="isCurrentActor">True = hiển thị gold outline</param>
        public void SetData(string entityId, string displayName, bool isPlayer, bool isCurrentActor)
        {
            _nameText.text = displayName;
            _portrait.color = isPlayer
                ? new Color(0.6f, 0.9f, 1f, 1f)   // xanh dương nhạt
                : new Color(1f, 0.6f, 0.6f, 1f);   // đỏ nhạt

            SetHighlight(isCurrentActor);
            gameObject.SetActive(true);
        }

        /// <summary>Bật/tắt gold outline highlight.</summary>
        public void SetHighlight(bool active)
        {
            if (_highlightOutline != null)
                _highlightOutline.color = active ? _highlightColor : _normalColor;
        }

        /// <summary>Xóa slot về trạng thái trống.</summary>
        public void Clear()
        {
            _nameText.text    = string.Empty;
            _portrait.color   = Color.white;
            SetHighlight(false);
        }

        #endregion
    }
}

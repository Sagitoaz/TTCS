using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace TTCS.UI.Combat
{
    /// <summary>
    /// 🔵 Dev A - Turn Order Slot
    /// Một ô trong hàng turn order — hiển thị portrait entity.
    /// Pooled bởi TurnOrderDisplay.
    /// </summary>
    public class TurnOrderSlot : MonoBehaviour
    {
        // ─── Inspector ────────────────────────────────────────────────────
        [SerializeField] private Image _portrait;
        [SerializeField] private Image _frameImage;

        [Header("Current Actor Scale")]
        [SerializeField] private float _currentScale = 1.18f;
        [SerializeField] private float _scaleDuration = 0.16f;

        private RectTransform _scaledRect;
        private Vector2 _baseAnchoredPos;
        private Vector3 _baseScale = Vector3.one;
        private Tween _scaleTween;
        private Tween _moveTween;

        private void Awake()
        {
            _scaledRect = _frameImage != null ? _frameImage.rectTransform : null;

            if (_scaledRect != null)
            {
                _baseAnchoredPos = _scaledRect.anchoredPosition;
                _baseScale = _scaledRect.localScale;
            }
        }

        // ──────────────────────────────────────────────────────────────────
        #region Public API

        /// <summary>
        /// Set dữ liệu cho slot.
        /// </summary>
        public void SetData(Sprite portrait, Sprite frameSprite, bool isCurrentActor)
        {
            if (_portrait != null)
            {
                _portrait.sprite = portrait;
                _portrait.color = portrait != null ? Color.white : new Color(1f, 1f, 1f, 0f);
                _portrait.preserveAspect = true;
            }

            if (_frameImage != null)
            {
                _frameImage.sprite = frameSprite;
                _frameImage.color = frameSprite != null ? Color.white : new Color(1f, 1f, 1f, 0f);
                _frameImage.preserveAspect = true;
            }

            SetCurrentActor(isCurrentActor);
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Scale portrait của actor hiện tại và đẩy lên nhẹ để tránh đè icon bên dưới.
        /// </summary>
        public void SetCurrentActor(bool active)
        {
            if (_scaledRect == null) return;

            _scaleTween?.Kill();
            _moveTween?.Kill();

            float targetScaleFactor = active ? _currentScale : 1f;
            Vector3 targetScale = _baseScale * targetScaleFactor;

            // Đẩy frame lên nhẹ để tránh che phần dưới của frame kế tiếp.
            float extraHeight = _scaledRect.rect.height * (targetScaleFactor - 1f);
            Vector2 targetPos = _baseAnchoredPos + new Vector2(0f, extraHeight * 0.5f);

            _scaleTween = _scaledRect.DOScale(targetScale, _scaleDuration).SetEase(Ease.OutCubic);
            _moveTween = _scaledRect.DOAnchorPos(targetPos, _scaleDuration).SetEase(Ease.OutCubic);
        }

        /// <summary>Xóa slot về trạng thái trống.</summary>
        public void Clear()
        {
            if (_portrait != null)
            {
                _portrait.sprite = null;
                _portrait.color = Color.white;
            }

            if (_frameImage != null)
            {
                _frameImage.sprite = null;
                _frameImage.color = Color.white;
            }

            SetCurrentActor(false);
        }

        private void OnDisable()
        {
            _scaleTween?.Kill();
            _moveTween?.Kill();
            _scaleTween = null;
            _moveTween = null;
        }

        #endregion
    }
}

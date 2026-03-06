using UnityEngine;
using DG.Tweening;
using System;

namespace TTCS.Visual
{
    /// <summary>
    /// 🟢 Dev B Sprint 2 - Telegraph Visual (FEH-style Enemy Warning)
    ///
    /// Hiển thị cảnh báo khi enemy chuẩn bị tấn công, cho player thời gian phản ứng.
    ///
    /// FEH-inspired design:
    ///   - "!" icon xuất hiện trên đầu enemy
    ///   - Ring co lại từ ngoài vào (shrink) — khi về 0 = timing window mở
    ///   - Màu chuyển vàng → đỏ khi gần hit (tạo cảm giác khẩn cấp)
    ///
    /// UNITY SETUP:
    ///   1. Tạo 2 child GameObjects con của EnemyRoot:
    ///      "WarningIcon"   → SpriteRenderer, sprite "!" hoặc skull icon
    ///      "RingIndicator" → SpriteRenderer, sprite circle/ring (pivot = center)
    ///   2. Gán vào Inspector fields bên dưới
    ///   Chi tiết: aidlc-docs/construction/unity-setup/unit-devB-5-character-visual-setup.md
    /// </summary>
    public class TelegraphVisual : MonoBehaviour
    {
        // ─── Inspector ────────────────────────────────────────────────────
        [Header("Visual Parts")]
        [Tooltip("SpriteRenderer cho dấu ! / icon warning")]
        [SerializeField] private SpriteRenderer _warningIcon;

        [Tooltip("SpriteRenderer vòng tròn co lại (ring/circle sprite, pivot center)")]
        [SerializeField] private SpriteRenderer _ringObject;

        [Header("Colors")]
        [Tooltip("Màu khi mới bắt đầu telegraph (vàng FEH)")]
        [SerializeField] private Color _warningColor = new Color(1f, 0.87f, 0.1f, 1f);

        [Tooltip("Màu khi gần kết thúc telegraph (đỏ nguy hiểm)")]
        [SerializeField] private Color _dangerColor  = new Color(1f, 0.25f, 0.25f, 1f);

        [Tooltip("Tại % tiến độ nào bắt đầu chuyển sang màu đỏ (0.0-1.0)")]
        [Range(0.4f, 0.8f)]
        [SerializeField] private float _colorShiftThreshold = 0.6f;

        // ─── Events ───────────────────────────────────────────────────────

        /// <summary>
        /// Invoke khi ring co về 0 — timing window bắt đầu mở (Dev A dùng).
        /// Nếu player không guard trong window sau event này → full damage.
        /// </summary>
        public event Action OnTelegraphComplete;

        // ─── State ────────────────────────────────────────────────────────
        public bool IsPlaying { get; private set; }
        private Sequence _sequence;

        // ─── Lifecycle ────────────────────────────────────────────────────
        private void Awake()
        {
            SetVisible(false);
        }

        // ─── Public API ───────────────────────────────────────────────────

        /// <summary>
        /// Bắt đầu telegraph animation.
        /// duration = thời gian "chuẩn bị" của enemy trước khi tấn công (giây).
        /// Khi animation xong → invoke OnTelegraphComplete.
        /// </summary>
        public void Play(float duration)
        {
            if (duration <= 0f) duration = 1f;
            if (IsPlaying) Stop();

            SetVisible(true);
            IsPlaying = true;

            // Reset state
            if (_ringObject  != null) _ringObject.transform.localScale = Vector3.one * 1.6f;
            if (_warningIcon != null)
            {
                Color c         = _warningColor;
                c.a             = 0f;
                _warningIcon.color = c;
                _ringObject.color  = _warningColor;
            }

            float ringDuration   = duration * 0.88f; // ring xong trước một chút
            float colorShiftTime = duration * _colorShiftThreshold;

            _sequence = DOTween.Sequence();

            // Warning icon fade in nhanh
            if (_warningIcon != null)
                _sequence.Join(_warningIcon.DOFade(1f, 0.1f).SetEase(Ease.OutQuad));

            // Ring co lại từ 1.6x → 0
            if (_ringObject != null)
            {
                _sequence.Join(
                    _ringObject.transform.DOScale(Vector3.zero, ringDuration)
                        .SetEase(Ease.Linear));

                // Chuyển màu vàng → đỏ khi gần xong
                _sequence.InsertCallback(colorShiftTime, () =>
                {
                    float shiftDuration = duration - colorShiftTime;
                    _ringObject.DOColor(_dangerColor, shiftDuration * 0.6f).SetEase(Ease.InQuad);
                    if (_warningIcon != null)
                        _warningIcon.DOColor(_dangerColor, shiftDuration * 0.4f).SetEase(Ease.InQuad);
                });
            }

            // Completion callback
            _sequence
                .AppendInterval(duration - ringDuration + 0.05f)
                .OnComplete(() =>
                {
                    IsPlaying = false;
                    SetVisible(false);
                    OnTelegraphComplete?.Invoke();
                });
        }

        /// <summary>Dừng và ẩn telegraph ngay lập tức (ví dụ: combat bị interrupt)</summary>
        public void Stop()
        {
            _sequence?.Kill();
            IsPlaying = false;
            SetVisible(false);
        }

        // ─── Private ──────────────────────────────────────────────────────
        private void SetVisible(bool visible)
        {
            if (_warningIcon != null) _warningIcon.gameObject.SetActive(visible);
            if (_ringObject  != null) _ringObject.gameObject.SetActive(visible);
        }
    }
}

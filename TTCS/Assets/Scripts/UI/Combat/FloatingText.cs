using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace TTCS.UI.Combat
{
    /// <summary>
    /// 🔵 Dev A - Floating Text
    /// Poolable text element tween lên và fade out khi hiển thị damage/heal số.
    ///
    /// Usage:
    ///   floatingText.Show("123", Color.red, worldPos);
    ///   // Tự động trả về pool qua OnComplete callback sau 1 giây.
    /// </summary>
    public class FloatingText : MonoBehaviour
    {
        // ─── Inspector ────────────────────────────────────────────────────
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private float _floatHeight   = 80f;
        [SerializeField] private float _duration      = 1f;
        [SerializeField] private float _criticalScale = 1.5f;

        // ─── Pool Callback ────────────────────────────────────────────────
        /// <summary>Được set bởi ActionResultDisplay. Gọi khi animation kết thúc.</summary>
        public Action<FloatingText> OnComplete { get; set; }

        // ─── Active Tween ─────────────────────────────────────────────────
        private Sequence _sequence;

        // ──────────────────────────────────────────────────────────────────
        #region Public API

        /// <summary>
        /// Hiển thị text tại vị trí world, tween lên rồi fade out.
        /// </summary>
        /// <param name="text">Nội dung hiển thị</param>
        /// <param name="color">Màu sắc (đỏ=damage, xanh=heal, vàng=crit)</param>
        /// <param name="worldPos">Vị trí world space (chuyển sang canvas space bên trong)</param>
        /// <param name="isCritical">True thì scale lên _criticalScale</param>
        public void Show(string text, Color color, Vector3 worldPos, bool isCritical = false)
        {
            // Hủy tween cũ nếu đang chạy (edge case khi pool tái sử dụng nhanh)
            _sequence?.Kill();

            _label.text  = text;
            _label.color = color;

            transform.position = worldPos;
            transform.localScale = isCritical
                ? Vector3.one * _criticalScale
                : Vector3.one;

            gameObject.SetActive(true);

            // Reset alpha
            var c = _label.color;
            c.a = 1f;
            _label.color = c;

            _sequence = DOTween.Sequence();
            _sequence.Append(
                transform.DOMoveY(worldPos.y + _floatHeight * 0.01f, _duration)
                         .SetEase(Ease.OutCubic)
            );
            _sequence.Join(
                _label.DOFade(0f, _duration * 0.6f)
                      .SetDelay(_duration * 0.4f)
                      .SetEase(Ease.InQuad)
            );
            _sequence.OnComplete(() =>
            {
                gameObject.SetActive(false);
                OnComplete?.Invoke(this);
            });
            _sequence.SetAutoKill(true);
            _sequence.Play();
        }

        /// <summary>Hủy animation hiện tại (dùng khi pool bị clear).</summary>
        public void Cancel()
        {
            _sequence?.Kill();
            gameObject.SetActive(false);
        }

        #endregion

        private void OnDestroy()
        {
            _sequence?.Kill();
        }
    }
}

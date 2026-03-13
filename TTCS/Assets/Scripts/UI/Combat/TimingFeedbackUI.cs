using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TTCS.UI.Combat
{
    /// <summary>
    /// 🔵 Dev A - Timing Feedback UI
    /// Hiển thị kết quả timing (Perfect / Good / Miss) bằng flash overlay và text.
    /// DOTween-driven animations.
    ///
    /// Được gọi bởi TimingSystem qua CombatUIController.ShowTimingResult().
    /// </summary>
    public class TimingFeedbackUI : MonoBehaviour
    {
        // ─── Inspector ────────────────────────────────────────────────────
        [Header("References")]
        [SerializeField] private CanvasGroup      _flashOverlay;
        [SerializeField] private TextMeshProUGUI  _gradeText;

        [Header("Colors")]
        [SerializeField] private Color _perfectColor = new Color(1f, 0.85f, 0.1f, 0.35f);  // Gold
        [SerializeField] private Color _goodColor    = new Color(0.3f, 0.7f, 1f,  0.25f);  // Blue
        [SerializeField] private Color _missColor    = new Color(0.8f, 0.2f, 0.2f, 0.2f);  // Red

        [Header("Timing")]
        [SerializeField] private float _perfectDuration = 0.6f;
        [SerializeField] private float _goodDuration    = 0.4f;
        [SerializeField] private float _missDuration    = 0.3f;

        // ─── Runtime ──────────────────────────────────────────────────────
        private Sequence _sequence;
        private Image    _flashImage;

        // ──────────────────────────────────────────────────────────────────
        private void Awake()
        {
            _flashImage = _flashOverlay.GetComponent<Image>();

            // Đảm bảo ẩn khi bắt đầu
            _flashOverlay.alpha = 0f;
            _gradeText.alpha    = 0f;
        }

        // ──────────────────────────────────────────────────────────────────
        #region Public API

        /// <summary>Hiển thị kết quả timing với animation tương ứng.</summary>
        public void ShowResult(TimingGrade grade)
        {
            _sequence?.Kill();

            switch (grade)
            {
                case TimingGrade.Perfect: PlayPerfect(); break;
                case TimingGrade.Good:    PlayGood();    break;
                case TimingGrade.Miss:    PlayMiss();    break;
            }
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Animations

        private void PlayPerfect()
        {
            SetupText("PERFECT!", _perfectColor);
            SetFlashColor(_perfectColor);

            _sequence = DOTween.Sequence();
            // Flash overlay
            _sequence.Append(_flashOverlay.DOFade(1f, 0.05f));
            _sequence.Append(_flashOverlay.DOFade(0f, _perfectDuration * 0.8f).SetEase(Ease.OutQuad));
            // Grade text: scale bounce + fade
            _sequence.Join(
                _gradeText.DOFade(1f, 0.05f)
            );
            _sequence.Join(
                _gradeText.transform.DOScale(1.5f, 0.15f)
                          .SetEase(Ease.OutBack)
                          .OnComplete(() =>
                              _gradeText.transform.DOScale(1f, 0.1f))
            );
            _sequence.AppendInterval(_perfectDuration * 0.3f);
            _sequence.Append(_gradeText.DOFade(0f, 0.2f));
            _sequence.SetAutoKill(true);
        }

        private void PlayGood()
        {
            SetupText("GOOD!", _goodColor);
            SetFlashColor(_goodColor);

            _sequence = DOTween.Sequence();
            _sequence.Append(_flashOverlay.DOFade(0.8f, 0.04f));
            _sequence.Append(_flashOverlay.DOFade(0f, _goodDuration * 0.8f).SetEase(Ease.OutQuad));
            // Grade text: scale bounce + fade
            _sequence.Join(
                _gradeText.DOFade(1f, 0.05f)
            );
            _sequence.Join(
                _gradeText.transform.DOScale(1.5f, 0.15f)
                          .SetEase(Ease.OutBack)
                          .OnComplete(() =>
                              _gradeText.transform.DOScale(1f, 0.1f))
            );
            
            _sequence.AppendInterval(_goodDuration * 0.4f);
            _sequence.Append(_gradeText.DOFade(0f, 0.15f));
            _sequence.SetAutoKill(true);
        }

        private void PlayMiss()
        {
            SetupText("MISS", _missColor);
            SetFlashColor(_missColor);

            _sequence = DOTween.Sequence();
            _sequence.Append(_flashOverlay.DOFade(0.6f, 0.03f));
            _sequence.Append(_flashOverlay.DOFade(0f, _missDuration * 0.8f).SetEase(Ease.OutQuad));
            // Grade text: scale bounce + fade
            _sequence.Join(
                _gradeText.DOFade(1f, 0.05f)
            );
            _sequence.Join(
                _gradeText.transform.DOScale(1.5f, 0.15f)
                          .SetEase(Ease.OutBack)
                          .OnComplete(() =>
                              _gradeText.transform.DOScale(1f, 0.1f))
            );
            // Screen shake qua Camera (chỉ khi Camera.main tồn tại)
            if (Camera.main != null)
            {
                Debug.Log("SHAKE CAMERA");
                Vector3 shakeStrength = new Vector3(0.5f, 0.5f, 0f);
                _sequence.Join(Camera.main.transform.DOShakePosition(_missDuration, shakeStrength, 15, 90f));
            }
                
            _sequence.AppendInterval(_missDuration);
            _sequence.Append(_gradeText.DOFade(0f, 0.12f));
            _sequence.SetAutoKill(true);
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Helpers

        private void SetupText(string text, Color color)
        {
            _gradeText.text  = text;
            _gradeText.color = color;
            _gradeText.alpha = 0f;
            _gradeText.transform.localScale = Vector3.one;
        }

        private void SetFlashColor(Color color)
        {
            if (_flashImage != null)
                _flashImage.color = color;
        }

        #endregion

        private void OnDestroy()
        {
            _sequence?.Kill();
        }
    }

    /// <summary>Kết quả của một lần timing input.</summary>
    public enum TimingGrade
    {
        Perfect,
        Good,
        Miss
    }
}

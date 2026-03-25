using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TTCS.Combat.Timing;

namespace TTCS.UI.Combat
{
    /// <summary>
    /// Hiển thị UI "parry window đang mở": prompt + thanh thời gian đếm ngược.
    /// Lắng nghe trực tiếp từ TimingSystem.OnWindowOpened / OnWindowClosed.
    /// </summary>
    public class TimingWindowUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup _root;
        [SerializeField] private TextMeshProUGUI _promptText;
        [SerializeField] private Image _progressFill;

        [Header("Content")]
        [SerializeField] private string _prompt = "PARRY! Press Space";

        [Header("Behavior")]
        [SerializeField] private bool _useUnscaledTime = false;

        private Coroutine _progressRoutine;
        private bool _subscribed;

        private void Awake()
        {
            HideImmediate();
        }

        private void OnEnable()
        {
            TrySubscribe();
        }

        private void Update()
        {
            // TimingSystem có thể spawn sau UI, nên thử subscribe lại.
            if (!_subscribed)
                TrySubscribe();

            // Failsafe: nếu window đã đóng mà UI vẫn còn, ép tắt ngay.
            if (_subscribed && TimingSystem.Instance != null && !TimingSystem.Instance.IsWindowActive)
            {
                if (_progressRoutine != null || (_root != null && _root.alpha > 0f))
                {
                    StopProgress();
                    Hide();
                }
            }
        }

        private void OnDisable()
        {
            Unsubscribe();
            StopProgress();
        }

        private void OnDestroy()
        {
            Unsubscribe();
            StopProgress();
        }

        private void TrySubscribe()
        {
            if (_subscribed || TimingSystem.Instance == null)
                return;

            TimingSystem.Instance.OnWindowOpened += HandleWindowOpened;
            TimingSystem.Instance.OnWindowClosed += HandleWindowClosed;
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed || TimingSystem.Instance == null)
                return;

            TimingSystem.Instance.OnWindowOpened -= HandleWindowOpened;
            TimingSystem.Instance.OnWindowClosed -= HandleWindowClosed;
            _subscribed = false;
        }

        private void HandleWindowOpened(float duration)
        {
            Show();
            SetPrompt(_prompt);
            StartProgress(duration);
        }

        private void HandleWindowClosed()
        {
            StopProgress();
            Hide();
        }

        private void Show()
        {
            if (_root != null)
            {
                _root.alpha = 1f;
                _root.interactable = false;
                _root.blocksRaycasts = false;
            }
            SetProgress(1f);
        }

        private void Hide()
        {
            if (_root != null)
            {
                _root.alpha = 0f;
                _root.interactable = false;
                _root.blocksRaycasts = false;
            }
            SetProgress(0f);
        }

        private void HideImmediate()
        {
            Hide();
            SetPrompt(string.Empty);
        }

        private void SetPrompt(string text)
        {
            if (_promptText != null)
                _promptText.text = text;
        }

        private void SetProgress(float value01)
        {
            if (_progressFill != null)
                _progressFill.fillAmount = Mathf.Clamp01(value01);
        }

        private void StartProgress(float duration)
        {
            StopProgress();
            _progressRoutine = StartCoroutine(ProgressCountdown(Mathf.Max(0.01f, duration)));
        }

        private void StopProgress()
        {
            if (_progressRoutine != null)
            {
                StopCoroutine(_progressRoutine);
                _progressRoutine = null;
            }
        }

        private IEnumerator ProgressCountdown(float duration)
        {
            float elapsed = 0f;
            SetProgress(1f);

            while (elapsed < duration)
            {
                elapsed += _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                SetProgress(1f - t);
                yield return null;
            }

            SetProgress(0f);
            _progressRoutine = null;
        }
    }
}

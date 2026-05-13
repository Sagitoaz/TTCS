using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TTCS.Flow
{
    /// <summary>
    /// Global scene transition overlay.
    /// UX: Iris In -> Transition panel (random image + random advice + load progress) -> Iris Out.
    /// </summary>
    public sealed class SceneTransitionController : MonoBehaviour
    {
        public const string ImagesResourcesPath = "UI/SceneTransition/Images";

        [Header("Timings")]
        [SerializeField] private float _irisDuration = 0.35f;
        [SerializeField] private float _minPanelSeconds = 3f;
        [SerializeField] private float _postLoadHoldSeconds = 0f;

        [Header("Advice")]
        [TextArea(2, 6)]
        [SerializeField] private string[] _advicePool =
        {
            "Tip: Nâng cấp nhân vật giúp vượt ải dễ hơn.",
            "Tip: Thử đổi đội hình để tận dụng role/element.",
            "Tip: Trang bị có thể tăng chỉ số đáng kể.",
            "Tip: Ưu tiên nâng level cho nhân vật chủ lực.",
            "Tip: Kiểm tra skill cooldown trước khi vào combat.",
            "Tip: Rarity cao thường có chỉ số tốt hơn.",
            "Tip: Đừng quên vào Inventory để quản lý item.",
            "Tip: Farm lại level để kiếm tài nguyên khi cần.",
        };

        private bool _isTransitioning;
        private Canvas _canvas;
        private RectTransform _root;

        // Iris overlay
        private Image _irisImage;
        private Material _irisMaterial;

        // Transition panel
        private GameObject _panelRoot;
        private Image _randomImage;
        private TMP_Text _adviceText;
        private Slider _progressSlider;

        private void Awake()
        {
            EnsureUiBuilt();
            SetPanelVisible(false);
            SetIrisRadius(1.2f);
            SetIrisVisible(false);

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // No-op. We keep this hook in case later we want scene-specific tips.
        }

        public void LoadScene(string sceneName)
        {
            if (_isTransitioning)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(sceneName))
            {
                return;
            }

            StartCoroutine(LoadSceneRoutine(sceneName));
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            _isTransitioning = true;
            EnsureUiBuilt();

            // Iris In
            SetIrisVisible(true);
            yield return AnimateIris(from: 1.2f, to: 0f, _irisDuration);

            // Show transition panel
            SetPanelVisible(true);
            ApplyRandomContent();
            SetProgress(0f);
            var panelStartTime = Time.unscaledTime;
            // Iris is now fully closed (black). Disable it so the panel becomes visible.
            SetIrisVisible(false);

            // Load async
            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (operation == null)
            {
                SetPanelVisible(false);
                SetIrisRadius(0f);
                SetIrisVisible(true);
                yield return AnimateIris(from: 0f, to: 1.2f, _irisDuration);
                SetIrisVisible(false);
                _isTransitioning = false;
                yield break;
            }

            operation.allowSceneActivation = false;

            var displayedProgress = 0f;

            while (operation.progress < 0.9f)
            {
                var targetProgress = Mathf.Clamp01(operation.progress / 0.9f);
                // Smooth so the bar is visually readable.
                displayedProgress = Mathf.MoveTowards(displayedProgress, targetProgress, Time.unscaledDeltaTime * 1.5f);
                SetProgress(displayedProgress);
                yield return null;
            }

            // Ensure the loading panel is visible for a minimum time (prevents instant transitions).
            // While waiting, animate the progress bar to 100% so it doesn't look stuck.
            var elapsed = Time.unscaledTime - panelStartTime;
            var remaining = _minPanelSeconds - elapsed;
            if (remaining > 0f)
            {
                var start = Mathf.Clamp01(displayedProgress);
                var t = 0f;
                while (t < remaining)
                {
                    t += Time.unscaledDeltaTime;
                    var k = Mathf.Clamp01(t / remaining);
                    SetProgress(Mathf.Lerp(start, 1f, k));
                    yield return null;
                }
            }

            SetProgress(1f);

            if (_postLoadHoldSeconds > 0f)
            {
                yield return new WaitForSecondsRealtime(_postLoadHoldSeconds);
            }

            operation.allowSceneActivation = true;

            while (!operation.isDone)
            {
                yield return null;
            }

            // Scene is activated now.
            // Cover before removing the panel to avoid flashing.
            SetIrisRadius(0f);
            SetIrisVisible(true);
            SetPanelVisible(false);

            // Iris Out
            yield return AnimateIris(from: 0f, to: 1.2f, _irisDuration);
            SetIrisVisible(false);

            _isTransitioning = false;
        }

        private void EnsureUiBuilt()
        {
            if (_canvas != null && _root != null)
            {
                return;
            }

            var canvasGo = new GameObject("SceneTransitionCanvas");
            canvasGo.transform.SetParent(transform, false);

            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = short.MaxValue;

            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            _root = canvasGo.GetComponent<RectTransform>();
            _root.anchorMin = Vector2.zero;
            _root.anchorMax = Vector2.one;
            _root.offsetMin = Vector2.zero;
            _root.offsetMax = Vector2.zero;

            BuildPanel(_root);
            BuildIris(_root);
        }

        private void BuildIris(RectTransform parent)
        {
            var irisGo = new GameObject("Iris");
            irisGo.transform.SetParent(parent, false);

            var rt = irisGo.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            _irisImage = irisGo.AddComponent<Image>();
            _irisImage.raycastTarget = true;
            _irisImage.color = Color.black;

            var shader = Shader.Find("UI/IrisWipe");
            if (shader != null)
            {
                _irisMaterial = new Material(shader);
                _irisImage.material = _irisMaterial;
            }
        }

        private void BuildPanel(RectTransform parent)
        {
            _panelRoot = new GameObject("TransitionPanel");
            _panelRoot.transform.SetParent(parent, false);

            var rt = _panelRoot.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Fullscreen background to hide the previous scene while loading.
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(_panelRoot.transform, false);
            var bgRt = bgGo.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            var bgImage = bgGo.AddComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 1f);
            bgImage.raycastTarget = true;

            // Image area (top)
            var imageGo = new GameObject("RandomImage");
            imageGo.transform.SetParent(_panelRoot.transform, false);
            var imageRt = imageGo.AddComponent<RectTransform>();
            imageRt.anchorMin = new Vector2(0.5f, 0.6f);
            imageRt.anchorMax = new Vector2(0.5f, 0.9f);
            imageRt.sizeDelta = new Vector2(700f, 350f);
            imageRt.anchoredPosition = new Vector2(0f, -100f);

            _randomImage = imageGo.AddComponent<Image>();
            _randomImage.preserveAspect = true;
            _randomImage.color = Color.white;

            // Advice text
            var adviceGo = new GameObject("AdviceText");
            adviceGo.transform.SetParent(_panelRoot.transform, false);
            var adviceRt = adviceGo.AddComponent<RectTransform>();
            adviceRt.anchorMin = new Vector2(0.1f, 0.35f);
            adviceRt.anchorMax = new Vector2(0.9f, 0.55f);
            adviceRt.offsetMin = new Vector2(0f, -100f);
            adviceRt.offsetMax = new Vector2(0f, -100f);

            _adviceText = adviceGo.AddComponent<TextMeshProUGUI>();
            _adviceText.alignment = TextAlignmentOptions.Center;
            _adviceText.fontSize = 28;
            _adviceText.color = Color.white;
            _adviceText.enableWordWrapping = true;

            // Progress slider
            var sliderGo = new GameObject("Progress");
            sliderGo.transform.SetParent(_panelRoot.transform, false);
            var sliderRt = sliderGo.AddComponent<RectTransform>();
            sliderRt.anchorMin = new Vector2(0.2f, 0.2f);
            sliderRt.anchorMax = new Vector2(0.8f, 0.25f);
            sliderRt.offsetMin = new Vector2(0f, -100f);
            sliderRt.offsetMax = new Vector2(0f, -100f);

            _progressSlider = sliderGo.AddComponent<Slider>();
            _progressSlider.minValue = 0f;
            _progressSlider.maxValue = 1f;
            _progressSlider.value = 0f;

            // Build a minimal slider visuals (background + fill)
            var sliderBgGo = new GameObject("Background");
            sliderBgGo.transform.SetParent(sliderGo.transform, false);
            var sliderBgRt = sliderBgGo.AddComponent<RectTransform>();
            sliderBgRt.anchorMin = Vector2.zero;
            sliderBgRt.anchorMax = Vector2.one;
            sliderBgRt.offsetMin = Vector2.zero;
            sliderBgRt.offsetMax = Vector2.zero;
            var sliderBgImage = sliderBgGo.AddComponent<Image>();
            sliderBgImage.color = new Color(1f, 1f, 1f, 0.35f);

            var fillAreaGo = new GameObject("Fill Area");
            fillAreaGo.transform.SetParent(sliderGo.transform, false);
            var fillAreaRt = fillAreaGo.AddComponent<RectTransform>();
            fillAreaRt.anchorMin = Vector2.zero;
            fillAreaRt.anchorMax = Vector2.one;
            fillAreaRt.offsetMin = new Vector2(5f, 5f);
            fillAreaRt.offsetMax = new Vector2(-5f, -5f);

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(fillAreaGo.transform, false);
            var fillRt = fillGo.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            var fillImage = fillGo.AddComponent<Image>();
            fillImage.color = new Color(1f, 1f, 1f, 1f);

            _progressSlider.targetGraphic = sliderBgImage;
            _progressSlider.fillRect = fillRt;
            _progressSlider.direction = Slider.Direction.LeftToRight;

            // Hide handle
            _progressSlider.handleRect = null;
        }

        private void ApplyRandomContent()
        {
            // Random image from Resources
            var sprites = Resources.LoadAll<Sprite>(ImagesResourcesPath);
            if (_randomImage != null)
            {
                Sprite picked = null;
                if (sprites != null && sprites.Length > 0)
                {
                    picked = sprites[UnityEngine.Random.Range(0, sprites.Length)];
                }

                _randomImage.sprite = picked;
                _randomImage.color = picked == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
            }

            // Random advice
            if (_adviceText != null)
            {
                var advice = PickAdvice();
                _adviceText.text = advice;
            }
        }

        private string PickAdvice()
        {
            if (_advicePool == null || _advicePool.Length == 0)
            {
                return string.Empty;
            }

            var idx = UnityEngine.Random.Range(0, _advicePool.Length);
            var value = _advicePool[idx] ?? string.Empty;
            return value;
        }

        private IEnumerator AnimateIris(float from, float to, float duration)
        {
            duration = Mathf.Max(0.01f, duration);
            var t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                var k = Mathf.Clamp01(t / duration);
                var radius = Mathf.Lerp(from, to, k);
                SetIrisRadius(radius);
                yield return null;
            }

            SetIrisRadius(to);
        }

        private void SetIrisRadius(float radius)
        {
            if (_irisMaterial != null)
            {
                _irisMaterial.SetFloat("_Radius", Mathf.Clamp(radius, 0f, 1.2f));
                _irisMaterial.SetFloat("_Softness", 0.03f);
            }
        }

        private void SetIrisVisible(bool visible)
        {
            if (_irisImage != null)
            {
                _irisImage.enabled = visible;
                _irisImage.raycastTarget = visible;
            }
        }

        private void SetPanelVisible(bool visible)
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(visible);
            }
        }

        private void SetProgress(float value)
        {
            if (_progressSlider != null)
            {
                _progressSlider.value = Mathf.Clamp01(value);
            }
        }
    }
}

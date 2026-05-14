using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TTCS.Flow.MainMenu
{
    public class TutorialPanelSequenceController : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private bool _hideOnStart = true;

        [Header("Content")]
        [SerializeField] private Image _tutorialImage;
        [SerializeField] private List<Sprite> _panelSprites = new List<Sprite>();
        [SerializeField] private TMP_Text _pageIndicatorText;
        [SerializeField] private TMP_Text _continueButtonText;

        [Header("Navigation")]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _exitButton;

        private int _currentIndex;
        private bool _initialized;
        private bool _openedExplicitly;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void Start()
        {
            if (_hideOnStart && !_openedExplicitly)
            {
                SetPanelVisible(false);
            }
        }

        public void Open()
        {
            EnsureInitialized();
            _openedExplicitly = true;
            _currentIndex = 0;
            SetPanelVisible(true);
            RefreshView();
        }

        public void Close()
        {
            EnsureInitialized();
            _openedExplicitly = false;
            SetPanelVisible(false);
        }

        public void ShowPrevious()
        {
            if (_currentIndex <= 0)
            {
                return;
            }

            _currentIndex--;
            RefreshView();
        }

        public void ShowNext()
        {
            if (_currentIndex >= _panelSprites.Count - 1)
            {
                // Ở trang cuối → đóng panel
                Close();
                return;
            }

            _currentIndex++;
            RefreshView();
        }

        private void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            WireEvents();
        }

        private void WireEvents()
        {
            if (_backButton != null)
            {
                _backButton.onClick.RemoveListener(ShowPrevious);
                _backButton.onClick.AddListener(ShowPrevious);
            }

            if (_continueButton != null)
            {
                _continueButton.onClick.RemoveListener(ShowNext);
                _continueButton.onClick.AddListener(ShowNext);
            }

            if (_exitButton != null)
            {
                _exitButton.onClick.RemoveListener(Close);
                _exitButton.onClick.AddListener(Close);
            }
        }

        private void RefreshView()
        {
            if (_panelSprites == null || _panelSprites.Count == 0)
            {
                if (_backButton != null) _backButton.interactable = false;
                if (_continueButton != null) _continueButton.interactable = false;
                if (_pageIndicatorText != null) _pageIndicatorText.text = "1 / 1";
                return;
            }

            if (_tutorialImage != null)
            {
                var hasSprite = _currentIndex >= 0 && _currentIndex < _panelSprites.Count;
                _tutorialImage.enabled = hasSprite;
                _tutorialImage.sprite = hasSprite ? _panelSprites[_currentIndex] : null;
            }

            var isFirst = _currentIndex <= 0;
            var isLast  = _currentIndex >= _panelSprites.Count - 1;

            if (_backButton != null)
            {
                _backButton.interactable = !isFirst;
            }

            // Continue luôn interactable: ở trang cuối nó đóng panel thay vì bị disable
            if (_continueButton != null)
            {
                _continueButton.interactable = true;
            }

            if (_continueButtonText != null)
            {
                _continueButtonText.text = isLast ? "Finish" : "Continue";
            }

            if (_pageIndicatorText != null)
            {
                var pageCount = _panelSprites.Count;
                _pageIndicatorText.text = $"{_currentIndex + 1} / {pageCount}";
            }
        }

        private void SetPanelVisible(bool visible)
        {
            var target = _panelRoot != null ? _panelRoot : gameObject;
            target.SetActive(visible);
        }
    }
}

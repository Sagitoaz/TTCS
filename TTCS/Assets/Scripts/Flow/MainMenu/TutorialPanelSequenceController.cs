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
            if (_tutorialImage != null)
            {
                var hasSprite = _currentIndex >= 0 && _currentIndex < _panelSprites.Count;
                _tutorialImage.enabled = hasSprite;
                _tutorialImage.sprite = hasSprite ? _panelSprites[_currentIndex] : null;
            }

            if (_backButton != null)
            {
                _backButton.interactable = _currentIndex > 0;
            }

            if (_continueButton != null)
            {
                _continueButton.interactable = _currentIndex < _panelSprites.Count - 1;
            }

            if (_continueButtonText != null)
            {
                _continueButtonText.text = "Continue";
            }

            if (_pageIndicatorText != null)
            {
                var pageCount = Mathf.Max(1, _panelSprites.Count);
                _pageIndicatorText.text = $"{Mathf.Clamp(_currentIndex + 1, 1, pageCount)} / {pageCount}";
            }
        }

        private void SetPanelVisible(bool visible)
        {
            var target = _panelRoot != null ? _panelRoot : gameObject;
            target.SetActive(visible);
        }
    }
}

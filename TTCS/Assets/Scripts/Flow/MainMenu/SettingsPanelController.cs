using System;
using TMPro;
using TTCS.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace TTCS.Flow.MainMenu
{
    public class SettingsPanelController : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private bool _hideOnStart = true;

        [Header("Controls")]
        [SerializeField] private Slider _masterVolumeSlider;
        [SerializeField] private Slider _sfxVolumeSlider;
        [SerializeField] private Slider _uiVolumeSlider;
        [SerializeField] private Slider _bgmVolumeSlider;
        [SerializeField] private Button _closeButton;

        [Header("Optional Value Labels")]
        [SerializeField] private TMP_Text _masterValueText;
        [SerializeField] private TMP_Text _sfxValueText;
        [SerializeField] private TMP_Text _uiValueText;
        [SerializeField] private TMP_Text _bgmValueText;

        private bool _initialized;
        private bool _openedExplicitly;
        private Action _onCloseCallback;

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

            RefreshFromAudio();
        }

        public void Open()
        {
            Open(null);
        }

        /// <summary>
        /// Mở Settings panel. Khi đóng lại sẽ gọi <paramref name="onClose"/> callback.
        /// Dùng để biết phải quay về panel nào (Start Menu hay Main Menu).
        /// </summary>
        public void Open(Action onClose)
        {
            EnsureInitialized();
            _openedExplicitly = true;
            _onCloseCallback = onClose;
            RefreshFromAudio();
            SetPanelVisible(true);
        }

        public void Close()
        {
            EnsureInitialized();
            _openedExplicitly = false;
            SetPanelVisible(false);

            var callback = _onCloseCallback;
            _onCloseCallback = null;
            callback?.Invoke();
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
            if (_masterVolumeSlider != null)
            {
                _masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
                _masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (_sfxVolumeSlider != null)
            {
                _sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
                _sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            }

            if (_uiVolumeSlider != null)
            {
                _uiVolumeSlider.onValueChanged.RemoveListener(OnUiVolumeChanged);
                _uiVolumeSlider.onValueChanged.AddListener(OnUiVolumeChanged);
            }

            if (_bgmVolumeSlider != null)
            {
                _bgmVolumeSlider.onValueChanged.RemoveListener(OnBgmVolumeChanged);
                _bgmVolumeSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(Close);
                _closeButton.onClick.AddListener(Close);
            }
        }

        private void RefreshFromAudio()
        {
            var audio = AudioController.Instance;
            if (audio == null)
            {
                return;
            }

            SetSliderValue(_masterVolumeSlider, audio.GetMasterVolume());
            SetSliderValue(_sfxVolumeSlider, audio.GetSFXVolume());
            SetSliderValue(_uiVolumeSlider, audio.GetUIVolume());
            SetSliderValue(_bgmVolumeSlider, audio.GetBGMVolume());

            UpdateValueText(_masterValueText, audio.GetMasterVolume());
            UpdateValueText(_sfxValueText, audio.GetSFXVolume());
            UpdateValueText(_uiValueText, audio.GetUIVolume());
            UpdateValueText(_bgmValueText, audio.GetBGMVolume());
        }

        private void OnMasterVolumeChanged(float value)
        {
            AudioController.Instance?.SetMasterVolume(value);
            UpdateValueText(_masterValueText, value);
        }

        private void OnSfxVolumeChanged(float value)
        {
            AudioController.Instance?.SetSFXVolume(value);
            UpdateValueText(_sfxValueText, value);
        }

        private void OnUiVolumeChanged(float value)
        {
            AudioController.Instance?.SetUIVolume(value);
            UpdateValueText(_uiValueText, value);
        }

        private void OnBgmVolumeChanged(float value)
        {
            AudioController.Instance?.SetBGMVolume(value);
            UpdateValueText(_bgmValueText, value);
        }

        private void SetPanelVisible(bool visible)
        {
            var target = _panelRoot != null ? _panelRoot : gameObject;
            target.SetActive(visible);
        }

        private static void SetSliderValue(Slider slider, float value)
        {
            if (slider == null)
            {
                return;
            }

            slider.SetValueWithoutNotify(Mathf.Clamp01(value));
        }

        private static void UpdateValueText(TMP_Text label, float value)
        {
            if (label == null)
            {
                return;
            }

            label.text = $"{Mathf.RoundToInt(Mathf.Clamp01(value) * 100f)}%";
        }
    }
}

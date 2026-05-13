using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TTCS.Audio
{
    public enum AudioCueType
    {
        Click,
        Confirm,
        Cancel,
        Back,
        Hover,
        PopupOpen,
        PopupClose
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public class UIButtonSoundRelay : MonoBehaviour, IPointerEnterHandler
    {
        [SerializeField] private AudioCueType _clickCue = AudioCueType.Click;
        [SerializeField] private bool _playHover = true;
        [SerializeField] private AudioCueType _hoverCue = AudioCueType.Hover;

        private Button _button;
        private bool _isHooked;

        private void Awake()
        {
            CacheButton();
            Hook();
        }

        private void OnEnable()
        {
            CacheButton();
            Hook();
        }

        private void OnDisable()
        {
            Unhook();
        }

        public void EnsureConfigured()
        {
            CacheButton();
            Hook();
        }

        public void SetCue(AudioCueType clickCue, bool playHover = true, AudioCueType hoverCue = AudioCueType.Hover)
        {
            _clickCue = clickCue;
            _playHover = playHover;
            _hoverCue = hoverCue;
            EnsureConfigured();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_playHover || _button == null || !_button.interactable)
                return;

            AudioController.Instance?.PlayUICue(_hoverCue);
        }

        private void CacheButton()
        {
            if (_button == null)
                _button = GetComponent<Button>();
        }

        private void Hook()
        {
            if (_isHooked || _button == null)
                return;

            _button.onClick.AddListener(OnClicked);
            _isHooked = true;
        }

        private void Unhook()
        {
            if (!_isHooked || _button == null)
                return;

            _button.onClick.RemoveListener(OnClicked);
            _isHooked = false;
        }

        private void OnClicked()
        {
            if (_button == null || !_button.interactable)
                return;

            AudioController.Instance?.PlayUICue(_clickCue);
        }
    }
}

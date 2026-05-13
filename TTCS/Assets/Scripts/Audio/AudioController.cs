using System;
using System.Collections.Generic;
using TTCS.Core.Events;
using TTCS.Debugging;
using TTCS.UI.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TTCS.Audio
{
    /// <summary>
    /// Global audio service for the whole project.
    /// Handles:
    /// - Combat SFX via EventBus
    /// - UI click / hover sounds
    /// - Scene BGM routing
    /// - Persisted volume settings
    /// </summary>
    public class AudioController : MonoBehaviour
    {
        [Serializable]
        private struct SceneBgmEntry
        {
            public string sceneName;
            public AudioClip clip;
            public bool loop;
        }

        private const string MasterVolumeKey = "Audio.MasterVolume";
        private const string SfxVolumeKey = "Audio.SfxVolume";
        private const string UiVolumeKey = "Audio.UiVolume";
        private const string BgmVolumeKey = "Audio.BgmVolume";

        private static AudioController _instance;
        public static AudioController Instance => _instance;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _uiSource;
        [SerializeField] private AudioSource _bgmSource;

        [Header("Combat SFX")]
        [SerializeField] private AudioClip _hitSFX;
        [SerializeField] private AudioClip _critHitSFX;
        [SerializeField] private AudioClip _healSFX;
        [SerializeField] private AudioClip _deathSFX;
        [SerializeField] private AudioClip _skillCastSFX;

        [Header("Timing SFX")]
        [SerializeField] private AudioClip _perfectSFX;
        [SerializeField] private AudioClip _goodSFX;
        [SerializeField] private AudioClip _missSFX;

        [Header("UI SFX")]
        [SerializeField] private AudioClip _uiClickSFX;
        [SerializeField] private AudioClip _uiConfirmSFX;
        [SerializeField] private AudioClip _uiCancelSFX;
        [SerializeField] private AudioClip _uiBackSFX;
        [SerializeField] private AudioClip _uiHoverSFX;
        [SerializeField] private AudioClip _uiPopupOpenSFX;
        [SerializeField] private AudioClip _uiPopupCloseSFX;

        [Header("Combat BGM")]
        [SerializeField] private AudioClip _combatBGM;
        [SerializeField] private AudioClip _victoryBGM;
        [SerializeField] private AudioClip _defeatBGM;

        [Header("Scene BGM")]
        [SerializeField] private List<SceneBgmEntry> _sceneBgmEntries = new List<SceneBgmEntry>();

        [Header("Volume (0-1)")]
        [Range(0f, 1f)] [SerializeField] private float _masterVolume = 1f;
        [Range(0f, 1f)] [SerializeField] private float _sfxVolume = 1f;
        [Range(0f, 1f)] [SerializeField] private float _uiVolume = 1f;
        [Range(0f, 1f)] [SerializeField] private float _bgmVolume = 0.7f;

        private readonly Dictionary<string, SceneBgmEntry> _sceneBgmLookup =
            new Dictionary<string, SceneBgmEntry>(StringComparer.Ordinal);

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            EnsureAudioSources();
            LoadSavedVolumes();
            RebuildSceneBgmLookup();
        }

        private void OnEnable()
        {
            SubscribeEvents();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Start()
        {
            ApplyVolumes();
            HandleSceneLoaded(SceneManager.GetActiveScene());
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        public void PlaySFX(AudioClip clip)
        {
            PlayOneShot(_sfxSource, clip, EffectiveSfxVolume);
        }

        public void PlayUISFX(AudioClip clip)
        {
            var source = _uiSource != null ? _uiSource : _sfxSource;
            PlayOneShot(source, clip, EffectiveUiVolume);
        }

        public void PlayUICue(AudioCueType cueType)
        {
            switch (cueType)
            {
                case AudioCueType.Confirm:
                    PlayUISFX(_uiConfirmSFX != null ? _uiConfirmSFX : _uiClickSFX);
                    break;
                case AudioCueType.Cancel:
                    PlayUISFX(_uiCancelSFX != null ? _uiCancelSFX : _uiClickSFX);
                    break;
                case AudioCueType.Back:
                    PlayUISFX(_uiBackSFX != null ? _uiBackSFX : _uiCancelSFX != null ? _uiCancelSFX : _uiClickSFX);
                    break;
                case AudioCueType.Hover:
                    PlayUISFX(_uiHoverSFX);
                    break;
                case AudioCueType.PopupOpen:
                    PlayUISFX(_uiPopupOpenSFX != null ? _uiPopupOpenSFX : _uiClickSFX);
                    break;
                case AudioCueType.PopupClose:
                    PlayUISFX(_uiPopupCloseSFX != null ? _uiPopupCloseSFX : _uiCancelSFX != null ? _uiCancelSFX : _uiClickSFX);
                    break;
                default:
                    PlayUISFX(_uiClickSFX);
                    break;
            }
        }

        public void PlayBGM(AudioClip clip)
        {
            PlayBGM(clip, true);
        }

        public void PlayBGM(AudioClip clip, bool loop)
        {
            if (_bgmSource == null)
                return;

            if (clip == null)
            {
                _bgmSource.Stop();
                _bgmSource.clip = null;
                return;
            }

            if (_bgmSource.clip == clip && _bgmSource.isPlaying && _bgmSource.loop == loop)
                return;

            _bgmSource.clip = clip;
            _bgmSource.loop = loop;
            _bgmSource.volume = EffectiveBgmVolume;
            _bgmSource.Play();
        }

        public void StopBGM()
        {
            _bgmSource?.Stop();
        }

        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
            ApplyVolumes();
            SaveVolumes();
        }

        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            ApplyVolumes();
            SaveVolumes();
        }

        public void SetUIVolume(float volume)
        {
            _uiVolume = Mathf.Clamp01(volume);
            ApplyVolumes();
            SaveVolumes();
        }

        public void SetBGMVolume(float volume)
        {
            _bgmVolume = Mathf.Clamp01(volume);
            ApplyVolumes();
            SaveVolumes();
        }

        public float GetMasterVolume() => _masterVolume;
        public float GetSFXVolume() => _sfxVolume;
        public float GetUIVolume() => _uiVolume;
        public float GetBGMVolume() => _bgmVolume;

        public void PlayTimingResult(TimingGrade grade)
        {
            switch (grade)
            {
                case TimingGrade.Perfect:
                    PlaySFX(_perfectSFX);
                    break;
                case TimingGrade.Good:
                    PlaySFX(_goodSFX);
                    break;
                case TimingGrade.Miss:
                    PlaySFX(_missSFX);
                    break;
            }
        }

        public void RefreshSceneAudio()
        {
            HandleSceneLoaded(SceneManager.GetActiveScene());
        }

        private void SubscribeEvents()
        {
            var bus = EventBus.Instance;
            if (bus == null)
                return;

            bus.Subscribe<DamageTakenEvent>(OnDamageTaken);
            bus.Subscribe<HealingReceivedEvent>(OnHealingReceived);
            bus.Subscribe<EntityDeathEvent>(OnEntityDeath);
            bus.Subscribe<SkillCastEvent>(OnSkillCast);
            bus.Subscribe<CombatStartedEvent>(OnCombatStarted);
            bus.Subscribe<CombatEndedEvent>(OnCombatEnded);
        }

        private void UnsubscribeEvents()
        {
            var bus = EventBus.Instance;
            if (bus == null)
                return;

            bus.Unsubscribe<DamageTakenEvent>(OnDamageTaken);
            bus.Unsubscribe<HealingReceivedEvent>(OnHealingReceived);
            bus.Unsubscribe<EntityDeathEvent>(OnEntityDeath);
            bus.Unsubscribe<SkillCastEvent>(OnSkillCast);
            bus.Unsubscribe<CombatStartedEvent>(OnCombatStarted);
            bus.Unsubscribe<CombatEndedEvent>(OnCombatEnded);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            HandleSceneLoaded(scene);
        }

        private void HandleSceneLoaded(Scene scene)
        {
            if (!scene.IsValid())
                return;

            AutoWireSceneButtons(scene);
            PlaySceneBgm(scene.name);
        }

        private void OnDamageTaken(DamageTakenEvent evt)
        {
            if (evt == null)
                return;

            PlaySFX(evt.IsCrit ? _critHitSFX : _hitSFX);
        }

        private void OnHealingReceived(HealingReceivedEvent evt)
        {
            if (evt == null)
                return;

            PlaySFX(_healSFX);
        }

        private void OnEntityDeath(EntityDeathEvent evt)
        {
            if (evt == null)
                return;

            PlaySFX(_deathSFX);
        }

        private void OnSkillCast(SkillCastEvent evt)
        {
            if (evt == null)
                return;

            PlaySFX(_skillCastSFX);
        }

        private void OnCombatStarted(CombatStartedEvent evt)
        {
            PlayBGM(_combatBGM, true);
        }

        private void OnCombatEnded(CombatEndedEvent evt)
        {
            if (evt == null)
                return;

            PlayBGM(evt.Victory ? _victoryBGM : _defeatBGM, false);
        }

        private void EnsureAudioSources()
        {
            _sfxSource = EnsureAudioSource(_sfxSource, "SFXSource");
            _uiSource = EnsureAudioSource(_uiSource, "UISource");
            _bgmSource = EnsureAudioSource(_bgmSource, "BGMSource");

            if (_bgmSource != null)
            {
                _bgmSource.playOnAwake = false;
                _bgmSource.loop = true;
            }
        }

        private AudioSource EnsureAudioSource(AudioSource source, string fallbackName)
        {
            if (source != null)
            {
                source.playOnAwake = false;
                return source;
            }

            var child = transform.Find(fallbackName);
            if (child != null)
            {
                source = child.GetComponent<AudioSource>();
                if (source != null)
                {
                    source.playOnAwake = false;
                    return source;
                }
            }

            var host = new GameObject(fallbackName);
            host.transform.SetParent(transform, false);
            source = host.AddComponent<AudioSource>();
            source.playOnAwake = false;
            return source;
        }

        private void ApplyVolumes()
        {
            if (_sfxSource != null)
                _sfxSource.volume = EffectiveSfxVolume;

            if (_uiSource != null)
                _uiSource.volume = EffectiveUiVolume;

            if (_bgmSource != null)
                _bgmSource.volume = EffectiveBgmVolume;
        }

        private void SaveVolumes()
        {
            PlayerPrefs.SetFloat(MasterVolumeKey, _masterVolume);
            PlayerPrefs.SetFloat(SfxVolumeKey, _sfxVolume);
            PlayerPrefs.SetFloat(UiVolumeKey, _uiVolume);
            PlayerPrefs.SetFloat(BgmVolumeKey, _bgmVolume);
            PlayerPrefs.Save();
        }

        private void LoadSavedVolumes()
        {
            _masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, _masterVolume);
            _sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, _sfxVolume);
            _uiVolume = PlayerPrefs.GetFloat(UiVolumeKey, _uiVolume);
            _bgmVolume = PlayerPrefs.GetFloat(BgmVolumeKey, _bgmVolume);
        }

        private void RebuildSceneBgmLookup()
        {
            _sceneBgmLookup.Clear();

            for (var i = 0; i < _sceneBgmEntries.Count; i++)
            {
                var entry = _sceneBgmEntries[i];
                if (string.IsNullOrWhiteSpace(entry.sceneName) || entry.clip == null)
                    continue;

                _sceneBgmLookup[entry.sceneName.Trim()] = entry;
            }
        }

        private void PlaySceneBgm(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                return;

            if (_sceneBgmLookup.TryGetValue(sceneName, out var entry))
            {
                PlayBGM(entry.clip, entry.loop);
                return;
            }

            DebugLogger.LogWarning($"AudioController: No scene BGM mapped for scene '{sceneName}'.", DebugLogger.LogCategory.General);
        }

        private void AutoWireSceneButtons(Scene scene)
        {
            var buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < buttons.Length; i++)
            {
                var button = buttons[i];
                if (button == null || button.gameObject.scene != scene)
                    continue;

                var relay = button.GetComponent<UIButtonSoundRelay>();
                if (relay == null)
                {
                    relay = button.gameObject.AddComponent<UIButtonSoundRelay>();
                    relay.SetCue(GuessCueTypeForButton(button));
                }

                relay.EnsureConfigured();
            }
        }

        private static AudioCueType GuessCueTypeForButton(Button button)
        {
            if (button == null)
                return AudioCueType.Click;

            var key = button.gameObject.name.Trim().ToLowerInvariant();
            if (key.Contains("back"))
                return AudioCueType.Back;

            if (key.Contains("cancel") || key.Contains("close"))
                return AudioCueType.Cancel;

            if (key.Contains("confirm") || key.Contains("fight") || key.Contains("play") ||
                key.Contains("retry") || key.Contains("continue") || key.Contains("use") ||
                key.Contains("roll") || key.Contains("start"))
            {
                return AudioCueType.Confirm;
            }

            return AudioCueType.Click;
        }

        private static void PlayOneShot(AudioSource source, AudioClip clip, float volume)
        {
            if (source == null || clip == null || volume <= 0f)
                return;

            source.PlayOneShot(clip, volume);
        }

        private float EffectiveSfxVolume => _masterVolume * _sfxVolume;
        private float EffectiveUiVolume => _masterVolume * _uiVolume;
        private float EffectiveBgmVolume => _masterVolume * _bgmVolume;
    }
}

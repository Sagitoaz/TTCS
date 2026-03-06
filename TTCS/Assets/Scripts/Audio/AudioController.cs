using UnityEngine;
using TTCS.Core.Events;
using TTCS.Debugging;
using TTCS.UI.Combat;
using static TTCS.Debugging.DebugLogger;

namespace TTCS.Audio
{
    /// <summary>
    /// 🔵 Dev A - Audio Controller
    /// Singleton quản lý SFX và BGM toàn game.
    /// Lắng nghe EventBus để play SFX phù hợp với combat events.
    ///
    /// Setup trong Unity:
    ///   1. Đặt AudioController trên DontDestroyOnLoad GameObject.
    ///   2. Gán AudioSource components (sfxSource, bgmSource).
    ///   3. Gán AudioClip assets qua Inspector.
    /// </summary>
    public class AudioController : MonoBehaviour
    {
        // ─── Singleton ────────────────────────────────────────────────────
        private static AudioController _instance;
        public  static AudioController Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                UnsubscribeEvents();
            }
        }

        // ─── Audio Sources ────────────────────────────────────────────────
        [Header("Audio Sources")]
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _bgmSource;

        // ─── Combat SFX Clips ─────────────────────────────────────────────
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

        [Header("BGM")]
        [SerializeField] private AudioClip _combatBGM;
        [SerializeField] private AudioClip _victoryBGM;
        [SerializeField] private AudioClip _defeatBGM;

        // ─── Volume ───────────────────────────────────────────────────────
        [Header("Volume (0–1)")]
        [Range(0f, 1f)] [SerializeField] private float _sfxVolume = 1f;
        [Range(0f, 1f)] [SerializeField] private float _bgmVolume = 0.7f;

        // ──────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void OnEnable()  => SubscribeEvents();
        private void OnDisable() => UnsubscribeEvents();

        private void Start()
        {
            ApplyVolumes();
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Public API — Playback

        /// <summary>Play một AudioClip one-shot (SFX).</summary>
        public void PlaySFX(AudioClip clip)
        {
            if (clip == null || _sfxSource == null) return;
            _sfxSource.PlayOneShot(clip, _sfxVolume);
        }

        /// <summary>Bắt đầu phát BGM (loop). Dừng track hiện tại nếu đang phát.</summary>
        public void PlayBGM(AudioClip clip)
        {
            if (_bgmSource == null) return;
            if (clip == null)
            {
                _bgmSource.Stop();
                return;
            }
            if (_bgmSource.clip == clip && _bgmSource.isPlaying) return;

            _bgmSource.clip  = clip;
            _bgmSource.loop  = true;
            _bgmSource.volume = _bgmVolume;
            _bgmSource.Play();
        }

        /// <summary>Dừng BGM hiện tại.</summary>
        public void StopBGM() => _bgmSource?.Stop();

        /// <summary>Đặt âm lượng SFX (0–1).</summary>
        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
        }

        /// <summary>Đặt âm lượng BGM (0–1) và áp dụng ngay.</summary>
        public void SetBGMVolume(float volume)
        {
            _bgmVolume = Mathf.Clamp01(volume);
            if (_bgmSource != null) _bgmSource.volume = _bgmVolume;
        }

        /// <summary>Phát SFX tương ứng với kết quả timing.</summary>
        public void PlayTimingResult(TimingGrade grade)
        {
            switch (grade)
            {
                case TimingGrade.Perfect: PlaySFX(_perfectSFX); break;
                case TimingGrade.Good:    PlaySFX(_goodSFX);    break;
                case TimingGrade.Miss:    PlaySFX(_missSFX);    break;
            }
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Event Subscription

        private void SubscribeEvents()
        {
            var bus = EventBus.Instance;
            if (bus == null) return;

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
            if (bus == null) return;

            bus.Unsubscribe<DamageTakenEvent>(OnDamageTaken);
            bus.Unsubscribe<HealingReceivedEvent>(OnHealingReceived);
            bus.Unsubscribe<EntityDeathEvent>(OnEntityDeath);
            bus.Unsubscribe<SkillCastEvent>(OnSkillCast);
            bus.Unsubscribe<CombatStartedEvent>(OnCombatStarted);
            bus.Unsubscribe<CombatEndedEvent>(OnCombatEnded);
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Event Handlers

        private void OnDamageTaken(DamageTakenEvent e)
        {
            PlaySFX(e.IsCrit ? _critHitSFX : _hitSFX);
        }

        private void OnHealingReceived(HealingReceivedEvent e)
        {
            PlaySFX(_healSFX);
        }

        private void OnEntityDeath(EntityDeathEvent e)
        {
            PlaySFX(_deathSFX);
        }

        private void OnSkillCast(SkillCastEvent e)
        {
            PlaySFX(_skillCastSFX);
        }

        private void OnCombatStarted(CombatStartedEvent e)
        {
            PlayBGM(_combatBGM);
        }

        private void OnCombatEnded(CombatEndedEvent e)
        {
            StopBGM();
            PlayBGM(e.Victory ? _victoryBGM : _defeatBGM);
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Helpers

        private void ApplyVolumes()
        {
            if (_bgmSource != null) _bgmSource.volume = _bgmVolume;
        }

        #endregion
    }
}

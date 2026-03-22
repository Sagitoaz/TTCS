# 06 — Audio System — Synchronization & Sound Management

## 1. Overview

Audio System manages background music, sound effects, and dialogue. Key goals:
- Sync audio with action timing
- No overlapping sounds
- Performance efficient (min allocations)
- Volume independent control

---

## 2. AudioManager

**File**: `Scripts/Visual/Audio/AudioManager.cs`

**Responsibility**: Central audio control and master settings.

### 2.1 Structure

```csharp
public class AudioManager : MonoBehaviour {
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource[] sfxSources = new AudioSource[8];  // Pool of SFX sources
    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float musicVolume = 0.8f;
    [SerializeField] private float sfxVolume = 1f;
    [SerializeField] private float voiceVolume = 1f;
    
    private Dictionary<string, AudioClip> audioDatabase = new();
    private Queue<AudioSource> availableSFXSources = new();
}
```

### 2.2 Key Methods

```csharp
public void Initialize() {
    // Load audio database from JSON or Resources
    LoadAudioDatabase();
    
    // Setup SFX source pool
    for (int i = 0; i < sfxSources.Length; i++) {
        availableSFXSources.Enqueue(sfxSources[i]);
    }
}

public void PlaySFX(string sfxId, float volume = 1f) {
    if (!audioDatabase.TryGetValue(sfxId, out AudioClip clip)) {
        Debug.LogWarning($"Audio clip not found: {sfxId}");
        return;
    }
    
    if (availableSFXSources.Count == 0) {
        Debug.LogWarning("No available SFX sources!");
        return;
    }
    
    AudioSource source = availableSFXSources.Dequeue();
    source.clip = clip;
    source.volume = sfxVolume * volume * masterVolume;
    source.Play();
    
    StartCoroutine(ReturnSourceToPool(source, clip.length));
}

private IEnumerator ReturnSourceToPool(AudioSource source, float duration) {
    yield return new WaitForSeconds(duration);
    source.Stop();
    availableSFXSources.Enqueue(source);
}

public void PlayBGM(string bgmId, float fadeInTime = 1f) {
    if (!audioDatabase.TryGetValue(bgmId, out AudioClip clip)) {
        Debug.LogWarning($"BGM clip not found: {bgmId}");
        return;
    }
    
    StartCoroutine(FadeBGM(musicSource.volume, 0, 0.5f));
    StartCoroutine(PlayBGMCoroutine(clip, fadeInTime));
}

private IEnumerator PlayBGMCoroutine(AudioClip clip, float fadeInTime) {
    yield return new WaitForSeconds(0.5f);
    
    musicSource.clip = clip;
    musicSource.loop = true;
    musicSource.Play();
    
    yield return StartCoroutine(FadeBGM(0, musicVolume * masterVolume, fadeInTime));
}

public void StopBGM(float fadeOutTime = 1f) {
    StartCoroutine(FadeBGM(musicSource.volume, 0, fadeOutTime));
}

private IEnumerator FadeBGM(float fromVolume, float toVolume, float duration) {
    float elapsed = 0;
    
    while (elapsed < duration) {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        musicSource.volume = Mathf.Lerp(fromVolume, toVolume, t);
        yield return null;
    }
    
    musicSource.volume = toVolume;
    
    if (toVolume == 0) {
        musicSource.Stop();
    }
}

public void SetMasterVolume(float volume) {
    masterVolume = Mathf.Clamp01(volume);
    UpdateAllVolumes();
}

public void SetMusicVolume(float volume) {
    musicVolume = Mathf.Clamp01(volume);
    musicSource.volume = musicVolume * masterVolume;
}

public void SetSFXVolume(float volume) {
    sfxVolume = Mathf.Clamp01(volume);
}

private void UpdateAllVolumes() {
    musicSource.volume = musicVolume * masterVolume;
    foreach (var source in sfxSources) {
        if (source.isPlaying) {
            source.volume *= masterVolume;
        }
    }
}

private void LoadAudioDatabase() {
    // Load from Resources/Audio or JSON
    var audioClips = Resources.LoadAll<AudioClip>("Audio");
    foreach (var clip in audioClips) {
        audioDatabase[clip.name] = clip;
    }
}
```

---

## 3. SoundEffectController

**File**: `Scripts/Visual/Audio/SoundEffectController.cs`

**Responsibility**: Trigger contextual sound effects based on combat events.

### 3.1 Sound Event Mapping

```csharp
public class SoundEffectController : MonoBehaviour {
    [SerializeField] private AudioManager audioManager;
    
    // Sound ID mappings
    private Dictionary<ActionType, string> actionSounds = new() {
        { ActionType.PhysicalAttack, "sfx_slash" },
        { ActionType.HeavyAttack, "sfx_heavy_hit" },
        { ActionType.Spell, "sfx_magic_cast" },
        { ActionType.Heal, "sfx_heal" }
    };
    
    private Dictionary<string, string> effectSounds = new() {
        { "Poison", "sfx_poison" },
        { "Burn", "sfx_fire" },
        { "Freeze", "sfx_ice" },
        { "Stun", "sfx_stun" }
    };
}
```

### 3.2 Event Handlers

```csharp
public void OnSkillCast(SkillCastEvent evt) {
    // Play action sound
    if (actionSounds.TryGetValue(evt.ActionType, out string sfxId)) {
        audioManager.PlaySFX(sfxId, volume: 0.8f);
    }
}

public void OnDamageTaken(DamageTakenEvent evt) {
    // Play hit sound
    string hitSound = evt.IsCritical ? "sfx_crit_hit" : "sfx_hit";
    audioManager.PlaySFX(hitSound, volume: 0.7f);
    
    // Volume based on damage
    float volumeScale = Mathf.Clamp01(evt.Damage / 100f);
    audioManager.PlaySFX("sfx_impact", volume: volumeScale);
}

public void OnHealing(HealingEvent evt) {
    audioManager.PlaySFX("sfx_heal", volume: 0.8f);
}

public void OnStatusEffectApplied(StatusEffectEvent evt) {
    if (effectSounds.TryGetValue(evt.EffectType, out string sfxId)) {
        audioManager.PlaySFX(sfxId, volume: 0.6f);
    }
}

public void OnEntityDied(EntityDiedEvent evt) {
    audioManager.PlaySFX("sfx_death", volume: 1f);
}
```

---

## 4. MusicController

**File**: `Scripts/Visual/Audio/MusicController.cs`

**Responsibility**: Battle music management and intensity scaling.

```csharp
public class MusicController : MonoBehaviour {
    [SerializeField] private AudioManager audioManager;
    
    // Music tracks
    private string currentBGM = "bgm_battle_normal";
    private int battleIntensity = 0;  // 0 = normal, 1 = danger, 2 = critical
    
    public void StartBattleMusic() {
        audioManager.PlayBGM("bgm_battle_normal", fadeInTime: 1f);
    }
    
    public void UpdateIntensity(float healthPercent) {
        int newIntensity = 0;
        
        if (healthPercent < 0.25f) {
            newIntensity = 2;  // Critical
        } else if (healthPercent < 0.5f) {
            newIntensity = 1;  // Danger
        }
        
        if (newIntensity != battleIntensity) {
            TransitionToIntensity(newIntensity);
        }
    }
    
    private void TransitionToIntensity(int intensity) {
        battleIntensity = intensity;
        
        string newBGM = intensity switch {
            0 => "bgm_battle_normal",
            1 => "bgm_battle_danger",
            2 => "bgm_battle_critical",
            _ => "bgm_battle_normal"
        };
        
        if (newBGM != currentBGM) {
            audioManager.PlayBGM(newBGM, fadeInTime: 2f);
            currentBGM = newBGM;
        }
    }
    
    public void OnBattleEnd(bool victory) {
        if (victory) {
            audioManager.PlayBGM("bgm_victory", fadeInTime: 0.5f);
        } else {
            audioManager.PlayBGM("bgm_defeat", fadeInTime: 0.5f);
        }
    }
}
```

---

## 5. Audio Database Configuration

**File**: `Resources/Audio/AudioConfig.json`

```json
{
  "sfx": {
    "sfx_slash": "Audio/SFX/slash",
    "sfx_heavy_hit": "Audio/SFX/heavy_hit",
    "sfx_magic_cast": "Audio/SFX/magic_cast",
    "sfx_heal": "Audio/SFX/heal",
    "sfx_hit": "Audio/SFX/hit",
    "sfx_crit_hit": "Audio/SFX/crit_hit",
    "sfx_impact": "Audio/SFX/impact",
    "sfx_death": "Audio/SFX/death",
    "sfx_poison": "Audio/SFX/poison",
    "sfx_fire": "Audio/SFX/fire",
    "sfx_ice": "Audio/SFX/ice",
    "sfx_stun": "Audio/SFX/stun",
    "sfx_button_click": "Audio/SFX/button_click"
  },
  "bgm": {
    "bgm_battle_normal": "Audio/BGM/battle_normal",
    "bgm_battle_danger": "Audio/BGM/battle_danger",
    "bgm_battle_critical": "Audio/BGM/battle_critical",
    "bgm_victory": "Audio/BGM/victory",
    "bgm_defeat": "Audio/BGM/defeat"
  }
}
```

---

## 6. Audio Folder Structure

```
Assets/
└── Audio/
    ├── SFX/
    │   ├── slash.wav
    │   ├── heavy_hit.wav
    │   ├── magic_cast.wav
    │   ├── heal.wav
    │   ├── hit.wav
    │   ├── crit_hit.wav
    │   ├── impact.wav
    │   ├── death.wav
    │   ├── poison.wav
    │   ├── fire.wav
    │   ├── ice.wav
    │   ├── stun.wav
    │   └── button_click.wav
    └── BGM/
        ├── battle_normal.ogg
        ├── battle_danger.ogg
        ├── battle_critical.ogg
        ├── victory.ogg
        └── defeat.ogg
```

---

## 7. Audio Integration Timing

```
SkillCast event (time 0s):
├─ 0.0s: Play action SFX (sfx_slash)
├─ 0.3s: Damage applies [from animation damage moment]
├─ 0.3s: Play hit SFX (sfx_hit)
├─ 0.35s: Play impact SFX (sfx_impact)
└─ 0.8s: Animation finishes, ready for next
```

---

## 8. Audio Performance Targets

- **SFX Pool Size**: 8 simultaneous sounds max
- **Memory**: < 50MB for all audio clips
- **CPU**: < 2% for audio processing per frame
- **Zero GC**: No allocations in audio update

---

## 9. Audio Checklist

- [ ] All SFX clips recorded/provided
- [ ] BGM tracks created in formats (OGG for loop, WAV for SFX)
- [ ] AudioManager pool size set appropriately
- [ ] Sound mappings configured for all action types
- [ ] Music intensity transitions smooth
- [ ] Volume controls working (master, music, SFX)
- [ ] No audio clipping or distortion
- [ ] Timing sync with animation/VFX perfect

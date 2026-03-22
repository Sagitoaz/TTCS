# 01 — Tổng Quan Kiến Trúc Sprint 02

## 1. Mục tiêu thiết kế

Sprint 02 xây dựng **Visual Combat Layer** — cung cấp UI, VFX, Animation, Audio để hiển thị combat engine từ Sprint 01. Mục tiêu kỹ thuật bao gồm:

- **Event-Driven Architecture**: Visual layer không gọi trực tiếp Combat logic → hoàn toàn độc lập.
- **Deterministic Timing**: Tất cả visual effect tuân theo exact timing window từ combat engine.
- **Performance Optimized**: Sử dụng object pooling, batching, để đảm bảo không có GC spike trong combat.
- **Scalable Design**: Chuẩn bị cho Sprint 03 (Network, Replay, etc).
- **Clear Visual Feedback**: Player thấy rõ tác động của mỗi action — damage, heal, effect, turn order.

---

## 2. Phân lớp kiến trúc (Expanded from Sprint 01)

```
┌─────────────────────────────────────────────────────────────────┐
│  PRESENTATION LAYER  (Sprint 02 — NEW)                          │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  UI Layer: ActionPanel, StatusDisplay, EffectIndicator    │ │
│  └────────────────────────────────────────────────────────────┘ │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  Animation: CharacterAnimator, ActionAnimationController  │ │
│  └────────────────────────────────────────────────────────────┘ │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  VFX: TelegraphVisual, ParticleEffectController, FloatText│ │
│  └────────────────────────────────────────────────────────────┘ │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  Audio: AudioManager, SoundEffectController               │ │
│  └────────────────────────────────────────────────────────────┘ │
└───────────────────────────────┬────────────────────────────────┘
                                │ EventBus: subscribe
                                │ Bridge: translate events
┌───────────────────────────────▼────────────────────────────────┐
│  GAME LOGIC LAYER (Sprint 01)                                  │
│  CombatFlowController • TurnManager • SkillManager              │
│  SkillAction • ActionValidator • ActionResolver                 │
│  CombatEntity • EntityFactory • AIController                    │
└───────────────────────────────┬────────────────────────────────┘
                                │ EventBus: publish
┌───────────────────────────────▼────────────────────────────────┐
│  DATA LAYER (Sprint 01)                                        │
│  DataManager • DataCache • CharacterDataModel                  │
│  SkillDataModel • EnemyDataModel                               │
└─────────────────────────────────────────────────────────────────┘
```

---

## 3. Module Architecture — Sprint 02

### 3.1 Visual.Views
**Trách nhiệm**: Hiển thị characters, enemies, status trực quan trên scene.

- `CharacterView`: Quản lý visual representation của player character
  - Position, rotation, scale
  - Model rendering (Sprite hoặc 3D model)
  - Health bar, status indicator
  
- `EnemyView`: Quản lý visual representation của enemy
  - Enemy sprite/model
  - Position management
  - Telegraph effect (attack preview)

- `EntityViewFactory`: Tạo instance view cho character/enemy
  - Asset loading
  - Pooling management

---

### 3.2 Visual.Animation
**Trách nhiệm**: Quản lý animations, state transitions, timing.

- `CharacterAnimator`: Trigger animation state dựa trên combat event
  - Idle → Attack → Hit → Dodge → Death
  - Damage knockback animation
  - Healing glow animation
  
- `ActionAnimationController`: Chi tiết hơn cho mỗi loại skill
  - Setup timing window cho mỗi skill frame
  - Damage moment (frame nào deal damage)
  - Recovery moment (frame nào animation kết thúc)

- `AnimationStateManager`: Centralized state tracking
  - Current animation state
  - Blend tree parameters
  - Prevent conflict giữa animations

---

### 3.3 Visual.Effects
**Trách nhiệm**: VFX, particles, visual indicators cho skill/effect.

- `TelegraphVisual`: Hiển thị thông tin skill trước khi execute
  - Target indicator (vùng ảnh hưởng)
  - Damage prediction number
  - Effect icon display
  
- `ParticleEffectController`: Manage particle systems
  - Slash VFX (từ sword attack)
  - Explosion VFX (từ fireball)
  - Healing particle (từ heal skill)

- `FloatingTextController`: Damage/heal number floating lên screen
  - Pooled number objects
  - Color: red = damage, green = heal, yellow = critical
  - Fade out animation

- `EffectIndicatorUI`: Hiển thị buff/debuff icon trên character
  - Poison, burn, freeze, stun icon
  - Duration countdown

---

### 3.4 Visual.UI
**Trách nhiệm**: Combat UI panels, HUD.

- `CombatUIManager`: Main manager cho all UI
  - Show/hide panels
  - Transition control
  
- `ActionPanel`: Hiển thị skill buttons
  - 4 skill buttons (A, B, C, D)
  - Button state: available, cooldown, mana insufficient
  - Selection highlight
  
- `StatusDisplay`: Character status info
  - Current HP / Max HP bar
  - MP bar
  - Current status effect list
  
- `TurnOrderDisplay`: Hiển thị turn order
  - Character turn indicator
  - Enemy turn indicator
  - CTB timeline preview

- `BattleLogDisplay`: Combat action log
  - "Player used Fireball!"
  - "Enemy took 45 damage"
  - Scrollable history

---

### 3.5 Visual.Audio
**Trách nhiệm**: Audio playback, sync với visual.

- `AudioManager`: Central audio control
  - Master volume
  - Music/SFX/Voice separate volume control
  
- `SoundEffectController`: Trigger SFX
  - "Woosh" sound khi attack
  - "Boom" sound khi skill hit
  - "Ding" sound khi heal
  
- `MusicController`: Background music
  - Battle music BGM
  - Sync BGM intensity với battle progress

---

### 3.6 Visual.Bridge
**Trách nhiệm**: Kết nối Sprint 02 (Visual) với Sprint 01 (Combat Engine).

- `CombatEventBridge`: Listen on Combat events, trigger Visual updates
  - Combat: SkillCast → Visual: Play animation + VFX + Audio
  - Combat: DamageTaken → Visual: Play hit animation, damage number, shake screen
  - Combat: EntityDied → Visual: Play death animation, remove from scene
  
- `VisualEventPublisher`: Publish Visual-specific events
  - AnimationFinished (khi animation kết thúc)
  - VFXFinished (khi VFX kết thúc)
  - UIActionSelected (khi player chọn skill)

---

## 4. Cấu trúc thư mục — Sprint 02

```
Assets/
├── Scripts/
│   └── Visual/                          ← SPRINT 02 ADDITIONS
│       ├── Views/
│       │   ├── CharacterView.cs
│       │   ├── EnemyView.cs
│       │   └── EntityViewFactory.cs
│       ├── Animation/
│       │   ├── CharacterAnimator.cs
│       │   ├── ActionAnimationController.cs
│       │   └── AnimationStateManager.cs
│       ├── Effects/
│       │   ├── TelegraphVisual.cs
│       │   ├── ParticleEffectController.cs
│       │   ├── FloatingTextController.cs
│       │   └── EffectIndicatorUI.cs
│       ├── UI/
│       │   ├── CombatUIManager.cs
│       │   ├── ActionPanel.cs
│       │   ├── StatusDisplay.cs
│       │   ├── TurnOrderDisplay.cs
│       │   ├── BattleLogDisplay.cs
│       │   └── UIPoolManager.cs
│       ├── Audio/
│       │   ├── AudioManager.cs
│       │   ├── SoundEffectController.cs
│       │   └── MusicController.cs
│       └── Bridge/
│           ├── CombatEventBridge.cs
│           └── VisualEventPublisher.cs
├── Prefabs/
│   └── Visual/
│       ├── CharacterView_Prefab.prefab
│       ├── EnemyView_Prefab.prefab
│       ├── ActionPanel_Prefab.prefab
│       ├── StatusDisplay_Prefab.prefab
│       └── FloatingText_Prefab.prefab
├── Animations/
│   ├── Character/
│   │   ├── Idle.anim
│   │   ├── Attack.anim
│   │   ├── Hit.anim
│   │   ├── Dodge.anim
│   │   └── Death.anim
│   └── Enemy/
│       ├── Idle.anim
│       ├── Attack.anim
│       └── Hit.anim
├── VFX/
│   ├── Particles/
│   │   ├── SlashVFX.prefab
│   │   ├── FireballVFX.prefab
│   │   └── HealingVFX.prefab
│   └── Materials/
└── Data/
    └── Visual/
        ├── ActionAnimationConfig.json    ← Timing data
        └── EffectVisualConfig.json        ← Effect display config
```

---

## 5. Key Design Patterns

### 5.1 Bridge Pattern
Kết nối Combat Engine (Sprint 01) với Visual Layer (Sprint 02) mà không tạo tight coupling.

```
Sprint 01 (Combat):        Sprint 02 (Visual):
   SkillCast event   ○ ←→ ○ CombatEventBridge
                     │     │ → CharacterAnimator
                     │     │ → ParticleEffectController
                     │     └ → SoundEffectController
```

### 5.2 Object Pooling
Tránh allocate/GC spike bằng reuse objects:

- FloatingText objects pooled
- VFX particle systems pooled
- UI elements reused (e.g., StatusEffect icons)

### 5.3 State Machine
Animation state transitions được quản lý by AnimationStateManager:

```
Idle →[Attack Command]→ AttackAnim
AttackAnim →[Anim Finish]→ IdleAnim
IdleAnim →[Damaged]→ HitAnim
HitAnim →[Anim Finish]→ IdleAnim
```

### 5.4 Observer Pattern
Event subscription architecture:

```
Combat: publish SkillCastEvent
└─ Visual.CharacterAnimator: subscribe → Play attack animation
└─ Visual.ParticleEffectController: subscribe → Play VFX
└─ Visual.SoundEffectController: subscribe → Play SFX
```

---

## 6. Critical Timing Specifications

Sprint 02 phải đảm bảo **exact timing** giữa visual và combat logic.

| Event | Duration | When | Purpose |
|-------|----------|------|---------|
| Skill Telegraph | 0.3s | Trước `SkillCast` | Player thấy preview |
| Attack Animation | Varies | Trong `ActionResolver.Execute()` | Visual hit sync |
| Damage Moment | Precise frame | When damage applied | Sync damage number display |
| Recovery Moment | End anim | After action finish | Return to idle state |
| Knockback | 0.2s | Khi nhận damage | Visual feedback |

---

## 7. Interface Contracts

```csharp
// Bridge Interface — Combat → Visual
interface ICombatEventListener {
    void OnSkillCast(SkillCastEvent evt);
    void OnDamageTaken(DamageTakenEvent evt);
    void OnHealing(HealingEvent evt);
    void OnEntityDied(EntityDiedEvent evt);
    void OnStatusEffectApplied(StatusEffectEvent evt);
}

// Visual Event Interface — Visual → Other systems
interface IVisualEventPublisher {
    void PublishAnimationFinished(string animationId);
    void PublishVFXFinished(string vfxId);
    void PublishUIActionSelected(int skillId);
}
```

---

## 8. Sprint 02 Dependencies on Sprint 01

**DO NOT break these imports:**

```csharp
// From Combat layer (Sprint 01)
using TTCS.Combat.Entities;
using TTCS.Combat.Actions;
using TTCS.Combat.Managers;
using TTCS.Core.Events;

// Visual layer uses read-only interfaces
interface IReadOnlyEntity {
    int HP { get; }
    int MaxHP { get; }
    string Name { get; }
}
```

---

## 9. Performance Targets

- **Frame rate**: 60 FPS min during combat
- **GC**: < 1MB allocation per turn
- **VFX**: Max 8 particle systems active simultaneously
- **UI**: Max 16 UI elements drawn per frame (batched)
- **Audio**: Max 8 simultaneous sound effects

---

## 10. Phase Roadmap

- **Sprint 02.1**: Views + Basic Animation framework
- **Sprint 02.2**: VFX + Telegraph system
- **Sprint 02.3**: UI Layer + BattleLog
- **Sprint 02.4**: Audio integration + Polish
- **Sprint 02.5**: Testing + Performance optimization

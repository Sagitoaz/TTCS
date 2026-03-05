# 📅 Kế hoạch làm việc Sprint 2 — TTCS Development

> **Chu kỳ phát triển:** Sprint 2 (Tuần 3–5)  
> **Team size:** 2 developers  
> **Duration:** 3 tuần (15 ngày làm việc)  
> **Focus:** Visual Combat Layer — Combat UI + Character Visuals + Timing System

---

## 🎯 Mục tiêu Sprint 2

Sprint 1 đã xây dựng **combat engine hoàn chỉnh chạy được không cần UI**.  
Sprint 2 đưa engine đó **lên màn hình** — người chơi thấy, tương tác được với trận đấu.

### Kết quả khi hoàn thành Sprint 2:
- ✅ Trận đấu hiển thị đầy đủ: nhân vật, thanh HP/MP, thứ tự lượt
- ✅ Nhân vật có animation (idle, attack, hurt, death) dùng part-based art
- ✅ Skill buttons hoạt động — click → animation → damage number nổi
- ✅ Timing window nhấp nháy khi enemy tấn công → player có thể guard
- ✅ Telegraph visual khi enemy chuẩn bị đánh
- ✅ VFX hit + audio feedback cơ bản
- ✅ CombatScene.unity hoàn chỉnh, chạy được từ đầu đến cuối

### Không làm trong Sprint 2:
- ❌ Main Menu + scene navigation
- ❌ Gacha / Roster / Progression
- ❌ Save/Load từ title screen
- ❌ Level up, rewards flow

---

## 🏗️ Kiến trúc Visual Layer

```
[Sprint 1 Engine]                    [Sprint 2 Visual Layer]
CombatFlowController  ←EventBus→    CombatBridge
TurnManager                          CombatSceneManager
SkillManager                         CharacterView / EnemyView
CombatEntity                         CharacterAnimator
ActionResolver                       ActionAnimationController
                                     VFXController / AudioController

[UI Layer - uGUI/Canvas]
CombatUIController
├── BattleHUD (HP/MP bars, status icons)
├── SkillButtonPanel (skills, cooldown, mana cost)
├── TurnOrderDisplay (queue visualization)
├── ActionResultDisplay (floating numbers)
└── TimingFeedbackUI (Perfect/Good/Miss flash)
```

---

## 🔀 Animation Approach — Part-based Characters

Art là ảnh **tách part** (head, body, arms/hands, legs, weapon). Cách setup:

```
CharacterRoot (GameObject)
├── Body (SpriteRenderer)
├── Head (SpriteRenderer)
├── WeaponHand (SpriteRenderer)
├── OffHand (SpriteRenderer)
└── Legs (SpriteRenderer)
```

**Animation workflow:**
1. Tạo `Animator Controller` cho mỗi nhân vật
2. Dùng **Record Mode** trong Unity Animator để keyframe các transform (position, rotation, scale) của từng part
3. Tạo các clip cơ bản: `Idle`, `Attack`, `Hurt`, `Death`, `Victory`
4. **Optional DOTween**: Thêm procedural touch (screen shake, part bounce) bằng DOTween sequence trong `CharacterAnimator.cs`

---

## 👥 Phân công team

### 👨‍💻 Developer A — UI + Timing System
**Expertise:** Architecture, Systems, Input handling

**Primary ownership:**
- `Scripts/UI/Combat/` — toàn bộ folder
- `Scripts/Combat/Timing/` — toàn bộ folder
- `Scripts/Combat/Managers/CombatSceneManager.cs`
- `Scripts/Combat/Managers/CombatBridge.cs`
- `Scripts/Audio/AudioController.cs`

### 👩‍💻 Developer B — Character Visuals + Scene Setup
**Expertise:** Gameplay, Animation, Scene composition

**Primary ownership:**
- `Scripts/Visual/` — toàn bộ folder
- `Scripts/Visual/VFX/VFXController.cs`
- `Scripts/Combat/Managers/ActionAnimationController.cs`
- `Scenes/CombatScene.unity`
- `Prefabs/Combat/` — character & enemy prefabs

---

## 🗂️ Cấu trúc thư mục mới trong Sprint 2

```
Assets/Scripts/
│
├── UI/
│   └── Combat/              🔵 Developer A OWNS
│       ├── CombatUIController.cs
│       ├── BattleHUD.cs
│       ├── SkillButtonPanel.cs
│       ├── SkillButton.cs
│       ├── TurnOrderDisplay.cs
│       ├── TurnOrderSlot.cs
│       ├── ActionResultDisplay.cs
│       ├── FloatingText.cs
│       └── TimingFeedbackUI.cs
│
├── Combat/
│   ├── Timing/              🔵 Developer A OWNS
│   │   ├── TimingSystem.cs
│   │   ├── TimingInputHandler.cs
│   │   └── TimingWindow.cs
│   └── Managers/
│       ├── CombatSceneManager.cs    [A] Scene orchestration
│       ├── CombatBridge.cs          [A] EventBus → Visual wiring
│       └── ActionAnimationController.cs  [B] Animate combat events
│
├── Visual/                  🟢 Developer B OWNS
│   ├── CharacterView.cs
│   ├── CharacterAnimator.cs
│   ├── EnemyView.cs
│   ├── CharacterViewFactory.cs
│   ├── TelegraphVisual.cs
│   └── VFX/
│       └── VFXController.cs
│
└── Audio/
    └── AudioController.cs   🔵 Developer A OWNS

Assets/Prefabs/
├── Combat/
│   ├── Characters/          🟢 Dev B creates
│   └── Enemies/             🟢 Dev B creates

Assets/Scenes/
└── CombatScene.unity        🟢 Dev B owns
```

---

## 📋 Unit Breakdown (5 Units)

| Unit | Tên | Owner | Files | Tuần |
|------|-----|-------|-------|------|
| **Unit A** | Combat UI System | Dev A | 9 files | Week 1-2 |
| **Unit B** | Character Visual System | Dev B | 6 files | Week 1-2 |
| **Unit C** | Timing System | Dev A | 3 files | Week 2-3 |
| **Unit D** | Combat Integration Bridge | Both | 3 files | Week 2-3 |
| **Unit E** | Scene Setup + VFX + Audio | Dev B + A | 3 files + scene | Week 3 |

---

## 📅 Lịch làm việc chi tiết — 3 tuần

---

### 🗓️ TUẦN 1 (Ngày 1–5) — Foundation Visual Layer

#### 🔵 Developer A — Tuần 1

**Ngày 1: Setup + BattleHUD skeleton**
- [ ] Tạo `Assets/Scripts/UI/Combat/` folder structure
- [ ] `CombatUIController.cs` — singleton, manages all UI panels
- [ ] `BattleHUD.cs` — HP bars, MP bars cho 3 ally + 3 enemy slots
  - Bind to `CombatEntity` data
  - Listen to `DamageTakenEvent`, `HealEvent` từ EventBus
  - Animate thanh HP (lerp)

**Ngày 2: Skill Button Panel**
- [ ] `SkillButton.cs` — single button: icon, cooldown overlay, mana cost label
- [ ] `SkillButtonPanel.cs` — 4 skill slots, disable khi không đủ mana/cooldown
  - Gọi `CombatFlowController.SubmitPlayerAction()` khi click
  - Ẩn khi không phải lượt player

**Ngày 3: Turn Order Display**
- [ ] `TurnOrderSlot.cs` — hiển thị icon + tên entity trong queue
- [ ] `TurnOrderDisplay.cs` — listen `TurnOrderUpdatedEvent`, rebuild queue visual
  - Pool up to 8 slots
  - Highlight current actor

**Ngày 4: Action Result Display**
- [ ] `FloatingText.cs` — poolable floating text, tween lên và fade out
- [ ] `ActionResultDisplay.cs` — listen `DamageTakenEvent` / `HealEvent`, spawn FloatingText
  - Màu đỏ = damage, xanh = heal, vàng = critical
  - Position trên đầu entity bị ảnh hưởng

**Ngày 5: Test + polish BattleHUD**
- [ ] Kết nối tạm CombatUIController với CombatTestLoader (Sprint 1)
- [ ] Verify HP bars cập nhật đúng khi nhận damage
- [ ] Verify skill buttons enable/disable theo state

---

#### 🟢 Developer B — Tuần 1

**Ngày 1: CharacterView hierarchy**
- [ ] Tạo `Assets/Scripts/Visual/` folder structure
- [ ] `CharacterView.cs` — MonoBehaviour, giữ references đến 5 part SpriteRenderers (Body, Head, WeaponHand, OffHand, Legs)
  - `SetFacing(bool flipX)` — lật hướng nhìn
  - `SetHighlight(Color)` — highlight khi là current actor
  - Inspector serialized fields cho từng part

**Ngày 2: CharacterAnimator**
- [ ] `CharacterAnimator.cs` — wrap Unity `Animator` + optional DOTween sequences
  - `PlayIdle()`, `PlayAttack()`, `PlayHurt()`, `PlayDeath()`, `PlayVictory()`
  - Attack: forward lunge DOTween + Animator clip
  - Hurt: shake DOTween sequence
  - Death: fall fade Animator clip
  - Expose `OnAttackHitFrame` event để sync với VFX/damage resolve

**Ngày 3: EnemyView + TelegraphVisual**
- [ ] `EnemyView.cs` — tương tự CharacterView, thêm `ShowPhaseTransition()` nếu boss
- [ ] `TelegraphVisual.cs` — hiển thị cảnh báo khi enemy chuẩn bị tấn công
  - Ring shrink animation (scale từ 1.5 về 0 trong N giây)
  - Flash sprite overlay
  - Duration = `telegraphDuration` từ EnemyData

**Ngày 4: CharacterViewFactory**
- [ ] `CharacterViewFactory.cs` — static, nhận `CharacterDataModel` / `EnemyDataModel`
  - Instantiate prefab từ `Resources/Prefabs/Characters/` hoặc `Enemies/`
  - Assign sprites từ art assets lên các part SpriteRenderers
  - Return `CharacterView` / `EnemyView` đã configured

**Ngày 5: Tạo prefabs + test**
- [ ] Tạo 2 character prefabs + 1 enemy prefab trong Unity Editor
- [ ] Assign sprites thực từ art assets
- [ ] Test idle animation play đúng

---

### 🗓️ TUẦN 2 (Ngày 6–10) — Integration + Timing Core

#### 🔵 Developer A — Tuần 2

**Ngày 6–7: CombatBridge**
- [ ] `CombatBridge.cs` — lắng nghe EventBus, cầu nối engine → visual
  - `OnDamageTaken(entity, amount, type)` → trigger CharacterAnimator.PlayHurt() + ActionResultDisplay
  - `OnEntityDied(entity)` → trigger CharacterAnimator.PlayDeath() + BattleHUD update
  - `OnSkillCast(caster, skill)` → trigger CharacterAnimator.PlayAttack()
  - `OnTurnStart(entity)` → update TurnOrderDisplay + highlight entity
  - `OnCombatEnd(result)` → show win/lose panel

**Ngày 8–9: TimingSystem core**
- [ ] `TimingWindow.cs` — data class: `openTime`, `duration`, `perfectThreshold (50ms)`, `goodThreshold (150ms)`
- [ ] `TimingSystem.cs` — MonoBehaviour singleton
  - `OpenWindow(TimingWindow window)` — bắt đầu nhận input
  - `CloseWindow()` — kết thúc, trả về `TimingGrade`
  - Coroutine-based window lifecycle
- [ ] `TimingInputHandler.cs` — MonoBehaviour
  - Listen `Input.GetButtonDown("Guard")` / Space
  - Gọi `TimingSystem.RegisterInput(Time.time)` 
  - Buffer: nhận input 60ms trước khi window mở

**Ngày 10: TimingFeedbackUI**
- [ ] `TimingFeedbackUI.cs` — visual response khi player đánh timing
  - Perfect: gold flash full screen nhẹ + "PERFECT!" text 0.5s
  - Good: blue tint nhẹ + "GOOD" text
  - Miss: screen shake nhỏ
  - DOTween cho tất cả animations
- [ ] Wire `TimingSystem.OnTimingResult` → `TimingFeedbackUI`

---

#### 🟢 Developer B — Tuần 2

**Ngày 6–7: ActionAnimationController**
- [ ] `ActionAnimationController.cs` — MonoBehaviour, nhận từ CombatBridge
  - Dictionary mapping `entityId → CharacterView/EnemyView`
  - `PlayAttackSequence(attacker, target)` — coroutine:
    1. attacker lunge forward
    2. Chờ `OnAttackHitFrame`
    3. target PlayHurt + camera shake nhỏ
    4. attacker return
  - `PlaySkillSequence(caster, targets, skillId)` — spawn VFX, animate

**Ngày 8–9: Animator clips**
- [ ] Setup Animator Controller cho Warrior character
  - Clips: Idle (loop), Attack, Hurt, Death, Victory
  - Transitions: Idle ↔ Attack, Idle ↔ Hurt, Any → Death
  - Parameters: `Attack` (trigger), `Hurt` (trigger), `IsDead` (bool)
- [ ] Setup Animator Controller cho Enemy (Goblin + Dark Knight)
- [ ] Mage Animator Controller
- [ ] Test tất cả transitions hoạt động đúng

**Ngày 10: Integration test Tuần 2**
- [ ] Chạy CombatTestLoader → verify CharacterViews spawn đúng
- [ ] Verify attack animation sync với damage resolve
- [ ] Verify telegraph visual hiển thị trước enemy attack

---

### 🗓️ TUẦN 3 (Ngày 11–15) — VFX, Audio, Scene Setup, Polish

#### 🔵 Developer A — Tuần 3

**Ngày 11–12: Timing System Integration**
- [ ] Wire `TimingSystem` vào `CombatFlowController`
  - Khi enemy chuẩn bị attack: gọi `TimingSystem.OpenWindow()`
  - `TimingGrade` ảnh hưởng đến `ActionResolver` — nhân hệ số damage nhận:
    - Perfect: nhận 20% damage
    - Good: nhận 60% damage
    - Miss: nhận 100% damage
- [ ] Modify `CombatFlowController.cs` để pass `TimingGrade` vào `ActionResolver.Resolve()`
- [ ] Modify `ActionResolver.cs` — thêm `TimingGrade` parameter vào signature

**Ngày 13: AudioController**
- [ ] `AudioController.cs` — singleton, play audio clips
  - `PlaySFX(AudioClip clip)` — one-shot
  - `PlayBGM(AudioClip clip)` — loop
  - `SetBGMVolume(float)`, `SetSFXVolume(float)`
  - Listen EventBus: `DamageTakenEvent` → sword clash SFX, `DeathEvent` → death SFX
  - Timing events → perfect/good sound

**Ngày 14: CombatSceneManager**
- [ ] `CombatSceneManager.cs` — replace CombatTestLoader, scene orchestration
  - `InitializeCombat(StageDataModel stage)` — load stage, spawn entities, init views
  - `OnCombatComplete(CombatResult result)` — show result screen (temporary in-scene)
  - Wire: DataManager → EntityFactory → CharacterViewFactory → CombatFlowController → UI

**Ngày 15: Full Integration + Polish**
- [ ] **Dev A**: Fix timing bugs, test timing grade flows
- [ ] **Dev B**: CombatScene.unity full setup (joint day)

---

#### 🟢 Developer B — Tuần 3

**Ngày 11–12: VFXController**
- [ ] `VFXController.cs` — singleton, manage particle effects
  - `PlayHitVFX(Vector3 position, DamageType type)` — màu theo element
  - `PlaySkillVFX(string skillId, Vector3 target)` — skill-specific VFX
  - `PlayDeathVFX(Vector3 position)` — death particles
  - Particle System pooling (reuse, không Instantiate/Destroy mỗi hit)

**Ngày 13: CombatScene.unity setup**
- [ ] Tạo `Assets/Scenes/CombatScene.unity`
- [ ] Setup Canvas hierarchy:
  ```
  Canvas (Screen Space - Camera)
  ├── BattleHUD
  ├── SkillButtonPanel
  ├── TurnOrderDisplay
  ├── ActionResultDisplay (pool của FloatingText)
  └── TimingFeedbackUI
  ```
- [ ] Setup Camera: Orthographic, background
- [ ] Character positions: ally side (left), enemy side (right)
- [ ] Lighting setup

**Ngày 14–15: Prefabs + Final Integration**
- [ ] Tạo đủ prefabs: char_warrior, char_mage, enemy_goblin, enemy_dark_knight
- [ ] Assign đủ: sprites (parts), Animator Controller, AudioSource
- [ ] Test full combat loop từ đầu đến cuối:
  1. Scene load → entities spawn → idle animations
  2. Player lượt → skill buttons hiện → click skill → attack animation
  3. Enemy lượt → telegraph visual → timing window mở → player guard
  4. Damage numbers, HP bars update
  5. Enemy chết → death animation → combat end

---

## 🔗 Integration Points (Cần phối hợp)

### Ngày 7 — Bridge Interface sync
```
Dev A cung cấp: CombatBridge API (method signatures)
Dev B cần: biết các method để gọi từ ActionAnimationController
Sync: review interface trước khi implement ActionAnimationController
```

### Ngày 10 — Mid-sprint integration test
```
Dev A: CombatBridge hoạt động với dummy visuals
Dev B: CharacterView hoạt động với dummy events
Cả hai: chạy test scene, verify events flow đúng
```

### Ngày 12 — Timing + Animation sync
```
Dev A: TimingSystem cần biết khi nào telegraph kết thúc → mở window
Dev B: TelegraphVisual cần expose OnTelegraphComplete callback
Sync: callback interface cho TelegraphVisual
```

### Ngày 14–15 — Full integration
```
Pair programming: Wire tất cả lại với nhau trong CombatScene.unity
Fix issues: timing, animation desync, UI bugs
```

---

## 📊 File Ownership Matrix — Sprint 2

| File | Owner |
|------|-------|
| `UI/Combat/CombatUIController.cs` | 🔵 Dev A |
| `UI/Combat/BattleHUD.cs` | 🔵 Dev A |
| `UI/Combat/SkillButtonPanel.cs` | 🔵 Dev A |
| `UI/Combat/SkillButton.cs` | 🔵 Dev A |
| `UI/Combat/TurnOrderDisplay.cs` | 🔵 Dev A |
| `UI/Combat/TurnOrderSlot.cs` | 🔵 Dev A |
| `UI/Combat/ActionResultDisplay.cs` | 🔵 Dev A |
| `UI/Combat/FloatingText.cs` | 🔵 Dev A |
| `UI/Combat/TimingFeedbackUI.cs` | 🔵 Dev A |
| `Combat/Timing/TimingSystem.cs` | 🔵 Dev A |
| `Combat/Timing/TimingInputHandler.cs` | 🔵 Dev A |
| `Combat/Timing/TimingWindow.cs` | 🔵 Dev A |
| `Combat/Managers/CombatSceneManager.cs` | 🔵 Dev A |
| `Combat/Managers/CombatBridge.cs` | 🔵 Dev A |
| `Audio/AudioController.cs` | 🔵 Dev A |
| `Visual/CharacterView.cs` | 🟢 Dev B |
| `Visual/CharacterAnimator.cs` | 🟢 Dev B |
| `Visual/EnemyView.cs` | 🟢 Dev B |
| `Visual/CharacterViewFactory.cs` | 🟢 Dev B |
| `Visual/TelegraphVisual.cs` | 🟢 Dev B |
| `Visual/VFX/VFXController.cs` | 🟢 Dev B |
| `Combat/Managers/ActionAnimationController.cs` | 🟢 Dev B |
| `Scenes/CombatScene.unity` | 🟢 Dev B |
| `Combat/Actions/ActionResolver.cs` | 💜 Both (timing integration) |
| `Combat/Managers/CombatFlowController.cs` | 💜 Both (timing hook) |

---

## 🔧 Sửa đổi Sprint 1 files (Cần trong Sprint 2)

### `CombatFlowController.cs` — Dev A adds timing hooks
```csharp
// Thêm vào ExecuteEnemyAction:
// 1. Trigger telegraph visual (thông qua EventBus)
// 2. Await TimingSystem.OpenWindow()
// 3. Pass TimingGrade vào ActionResolver.Resolve()
```

### `ActionResolver.cs` — Dev A adds timing parameter
```csharp
// Thay đổi signature:
// public static ResolveResult Resolve(IAction action, CombatEntity caster, CombatEntity target, TimingGrade grade)
// Apply damage multiplier theo grade
```

### `CombatEvents.cs` — Dev A adds new events
```csharp
// Thêm events mới:
// TelegraphStartEvent, TelegraphEndEvent, TimingWindowOpenEvent, TimingResultEvent
// CombatEndEvent (win/lose)
```

---

## ✅ Definition of Done — Sprint 2

Tất cả items sau phải pass:

- [ ] **Visual**: Nhân vật hiển thị đúng part-based art, animation idle/attack/hurt/death
- [ ] **HUD**: HP/MP bars cập nhật real-time khi nhận damage/heal
- [ ] **Skills**: Buttons hiển thị đúng skill, disable khi hết mana/cooldown, click được
- [ ] **Turn Order**: Queue hiển thị và cập nhật đúng
- [ ] **Damage Numbers**: Floating text spawn đúng vị trí với đúng màu
- [ ] **Telegraph**: Enemy hiển thị warning visual trước khi attack
- [ ] **Timing**: Window mở, player có thể guard, grade affect damage received
- [ ] **Timing Feedback**: Perfect/Good/Miss visual khác nhau rõ ràng
- [ ] **VFX**: Hit particles theo loại damage (physical/fire/etc.)
- [ ] **Audio**: SFX cho hit, death, skill cast; BGM loop
- [ ] **Scene**: CombatScene.unity khởi động → combat hoàn chỉnh không crash
- [ ] **End State**: Win/Lose condition hiển thị đúng

---

## 🌿 Git Branch Strategy — Sprint 2

```bash
# Dev A branches
feature/sprint2-combat-ui       # Units A: BattleHUD, SkillPanel, TurnOrder, Floating
feature/sprint2-timing-system   # Unit C: TimingSystem, Handler, FeedbackUI
feature/sprint2-combat-bridge   # Unit D: CombatBridge, CombatSceneManager

# Dev B branches
feature/sprint2-character-view  # Unit B: CharacterView, Animator, Factory
feature/sprint2-action-anim     # Unit D: ActionAnimationController
feature/sprint2-vfx-scene       # Unit E: VFXController, CombatScene
```

---

*Sprint 2 kế hoạch tạo ngày 2026-03-05. Được điều chỉnh bởi AI-DLC workflow.*

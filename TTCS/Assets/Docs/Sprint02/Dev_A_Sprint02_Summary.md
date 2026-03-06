# 📦 Sprint 02 — Dev A Deliverables Summary

> **Dự án**: TTCS — Those at The Crossroads of Story  
> **Developer**: Developer A — UI + Timing System  
> **Hoàn thành**: Sprint 2 (Phase 1)  
> **Scope**: Combat UI, Timing System, Combat Integration Bridge, Audio Controller

---

## 1. Tổng quan những gì đã build

Dev A xây dựng **toàn bộ visual feedback layer và timing system** cho combat:

```
UI Layer           → CombatUIController → BattleHUD, SkillButtonPanel, TurnOrderDisplay
Floating Numbers   → ActionResultDisplay → FloatingText (pooled, DOTween animated)
Timing System      → TimingWindow (data) + TimingSystem (logic) + TimingInputHandler (input)
Timing Feedback    → TimingFeedbackUI (Perfect/Good/Miss visual + defines TimingGrade enum)
Bridge Layer       → CombatBridge (EventBus → Visual) + ICharacterAnimatorBridge (for Dev B)
Scene Init         → CombatSceneManager (replaces CombatTestLoader)
Audio              → AudioController (event-driven SFX + BGM)
```

---

## 2. Cấu trúc thư mục

```
Assets/Scripts/
│
├── UI/
│   └── Combat/                      ← 🔵 Dev A owns
│       ├── CombatUIController.cs    ← Root singleton, điều phối toàn bộ UI
│       ├── BattleHUD.cs             ← HP/MP bars cho 3 ally + 3 enemy
│       ├── SkillButtonPanel.cs      ← 4 skill buttons + auto target resolve
│       ├── SkillButton.cs           ← Single skill button (icon + cooldown + mana)
│       ├── TurnOrderDisplay.cs      ← Pool 8 TurnOrderSlot
│       ├── TurnOrderSlot.cs         ← Single slot trong turn queue
│       ├── ActionResultDisplay.cs   ← Pool 10 FloatingText
│       ├── FloatingText.cs          ← Poolable damage number (DOTween)
│       └── TimingFeedbackUI.cs      ← Perfect/Good/Miss flash + TimingGrade enum
│
├── Combat/
│   ├── Timing/                      ← 🔵 Dev A owns
│   │   ├── TimingWindow.cs          ← Data: openTime, duration, thresholds
│   │   ├── TimingSystem.cs          ← Singleton: window lifecycle + input buffer
│   │   └── TimingInputHandler.cs    ← PlayerInput / Space → TimingSystem
│   └── Managers/
│       ├── CombatBridge.cs          ← EventBus → Visual bridge + ICharacterAnimatorBridge
│       └── CombatSceneManager.cs    ← Scene init coroutine (replaces CombatTestLoader)
│
└── Audio/
    └── AudioController.cs           ← DontDestroyOnLoad + event-driven audio
```

---

## 3. API Reference — UI Layer

### CombatUIController
```csharp
// Khởi tạo toàn bộ UI system
CombatUIController.Instance.Initialize(List<CombatEntity> playerTeam, List<CombatEntity> enemyTeam);

// Dev B gọi sau khi spawn CharacterView/EnemyView
CombatUIController.Instance.RegisterEntityPosition(string entityId, Transform worldTransform);

// Lấy vị trí world space của entity (dùng cho floating text)
Vector3 pos = CombatUIController.Instance.GetEntityWorldPos(string entityId);

// Lấy HP % (dùng cho animated bars nếu cần)
float pct = CombatUIController.Instance.GetEntityHPPercent(string entityId);

// Hiện timing result visual
CombatUIController.Instance.ShowTimingResult(TimingGrade grade);
```

### BattleHUD
```csharp
// Setup HP/MP slots (gọi bởi CombatUIController.Initialize)
battleHUD.InitializeSlots(List<CombatEntity> allies, List<CombatEntity> enemies);
// Tự động subscribe: DamageTakenEvent, HealingReceivedEvent, EntityDiedEvent
```

### SkillButtonPanel
```csharp
// Setup panel cho một player entity
skillButtonPanel.Initialize(string entityId, List<string> skillIds, List<CombatEntity> enemies);
// Tự subscribe TurnStartedEvent/TurnEndedEvent
// Khi player click skill → tự resolve targets → CombatFlowController.SubmitPlayerAction()
```

### TurnOrderDisplay
```csharp
// Đăng ký tên hiển thị cho entity
turnOrderDisplay.RegisterEntity(string entityId, string displayName, bool isPlayer);
// Tự subscribe TimelineUpdatedEvent → rebuild queue visual
```

### ActionResultDisplay
```csharp
// Không cần gọi thủ công — tự subscribe:
// DamageTakenEvent  → FloatingText màu đỏ (vàng nếu critical)
// HealingReceivedEvent → FloatingText màu xanh lá
```

### TimingFeedbackUI
```csharp
timingFeedbackUI.ShowResult(TimingGrade.Perfect); // gold flash + scale bounce
timingFeedbackUI.ShowResult(TimingGrade.Good);    // blue flash
timingFeedbackUI.ShowResult(TimingGrade.Miss);    // screen shake
```

---

## 4. API Reference — Timing System

### TimingWindow (Data Class)
```csharp
// Tạo window mặc định (sử dụng Constants.PERFECT_TIMING_WINDOW)
TimingWindow window = TimingWindow.CreateDefault(openTime: Time.time, duration: 2.0f);

// Các thuộc tính:
float window.OpenTime;          // thời điểm window mở
float window.Duration;          // window kéo dài bao lâu
float window.IdealTime;         // thời điểm tốt nhất để nhấn (giữa window)
float window.PerfectThreshold;  // 0.05s từ IdealTime = Perfect
float window.GoodThreshold;     // 0.15s từ IdealTime = Good (>0.15s = Miss)

// Evaluate input
TimingGrade grade = window.EvaluateInput(float inputTime);
// Perfect (<50ms), Good (<150ms), Miss (>150ms hoặc ngoài window)
```

### TimingSystem (MonoBehaviour Singleton)
```csharp
// Mở timing window (gọi từ CombatBridge khi telegraph kết thúc)
TimingSystem.Instance.OpenWindow(TimingWindow window);

// Đóng window sớm (nếu cần)
TimingSystem.Instance.ForceClose();

// Input registration (gọi bởi TimingInputHandler)
TimingSystem.Instance.RegisterInput(float gameTime);

// Subscribe kết quả
TimingSystem.Instance.OnTimingResult += (TimingGrade grade) => { ... };
```

### TimingInputHandler
```csharp
// Không cần gọi thủ công
// Tự detect: PlayerInput action "Guard" hoặc KeyCode.Space
// Tự gọi TimingSystem.Instance.RegisterInput(Time.time)
```

---

## 5. API Reference — Combat Bridge

### CombatBridge
```csharp
// Dev B implement interface này trong CharacterView/EnemyView
public interface ICharacterAnimatorBridge
{
    void PlayAttack();
    void PlayHurt();
    void PlayDeath();
    void PlayVictory();
    Transform GetWorldTransform();
}

// Dev B đăng ký sau khi spawn view
CombatBridge.Instance.RegisterView(string entityId, ICharacterAnimatorBridge view);

// Gọi khi telegraph animation kết thúc → mở timing window
CombatBridge.Instance.NotifyTelegraphComplete(string enemyId, float windowDuration);
```

### CombatSceneManager
```csharp
// Gọi khi load scene (thay cho CombatTestLoader)
StartCoroutine(CombatSceneManager.Instance.InitializeCombat(
    stageId: "stage_01",
    partyIds: new List<string> { "char_warrior", "char_mage" },
    seed: 42
));
// Pipeline: DataManager → EntityFactory → CombatUIController.Initialize()
//           → yield null (Dev B hook: spawn views here) → CombatFlowController.StartBattle()
```

---

## 6. API Reference — Audio Controller

### AudioController
```csharp
// Phát âm thanh
AudioController.Instance.PlaySFX(AudioClip clip);
AudioController.Instance.PlayBGM(AudioClip clip);     // loop
AudioController.Instance.StopBGM();

// Điều chỉnh âm lượng (0.0 → 1.0)
AudioController.Instance.SetSFXVolume(float volume);
AudioController.Instance.SetBGMVolume(float volume);

// Tự động từ events (không cần gọi thủ công):
// DamageTakenEvent  → sfxHit
// EntityDiedEvent   → sfxDeath
// TurnStartedEvent  → sfxTurnStart
// CombatEndedEvent  → sfxVictory/sfxDefeat

// Assign timing sounds qua Inspector:
// sfxPerfect, sfxGood, sfxMiss
AudioController.Instance.PlayTimingResult(TimingGrade grade);
```

---

## 7. TimingGrade Enum

```csharp
// Defined trong: Assets/Scripts/UI/Combat/TimingFeedbackUI.cs
// Namespace: TTCS.UI.Combat
namespace TTCS.UI.Combat
{
    public enum TimingGrade { Perfect, Good, Miss }
}

// Cách import trong file khác:
using TTCS.UI.Combat;
// Hoặc dùng fully-qualified: TTCS.UI.Combat.TimingGrade.Perfect
```

---

## 8. Events được subscribe

Các events từ Sprint 1 (`Assets/Scripts/Core/Events/CombatEvents.cs`) được subscribe tự động:

| Event | Subscriber(s) |
|-------|--------------|
| `DamageTakenEvent` | BattleHUD, ActionResultDisplay, CombatBridge, AudioController |
| `HealingReceivedEvent` | BattleHUD, ActionResultDisplay |
| `EntityDiedEvent` | BattleHUD, CombatBridge, AudioController |
| `SkillCastEvent` | CombatBridge, AudioController |
| `TurnStartedEvent` | SkillButtonPanel, CombatBridge, AudioController |
| `TurnEndedEvent` | SkillButtonPanel |
| `TimelineUpdatedEvent` | TurnOrderDisplay |
| `CombatEndedEvent` | CombatUIController, AudioController |

---

## 9. Hướng dẫn Integration cho Dev B

### Bước 1: Implement ICharacterAnimatorBridge
```csharp
// Trong CharacterView.cs
public class CharacterView : MonoBehaviour, CombatBridge.ICharacterAnimatorBridge
{
    [SerializeField] private CharacterAnimator _animator;

    public void PlayAttack()  => _animator.PlayAttack();
    public void PlayHurt()    => _animator.PlayHurt();
    public void PlayDeath()   => _animator.PlayDeath();
    public void PlayVictory() => _animator.PlayVictory();
    public Transform GetWorldTransform() => transform;
}
```

### Bước 2: Đăng ký sau spawn
```csharp
// Trong CharacterViewFactory.cs hoặc CombatSceneManager hook
var view = Instantiate(prefab).GetComponent<CharacterView>();
CombatBridge.Instance.RegisterView(entityId, view);
CombatUIController.Instance.RegisterEntityPosition(entityId, view.transform);
```

### Bước 3: Hook telegraph → timing
```csharp
// Trong TelegraphVisual.cs, khi animation kết thúc:
private IEnumerator PlayTelegraph(string enemyId, float duration)
{
    // ... ring shrink animation ...
    yield return new WaitForSeconds(duration);
    CombatBridge.Instance.NotifyTelegraphComplete(enemyId, duration);
}
```

---

## 10. Known Constraints

- `TimingGrade` enum nằm trong namespace `TTCS.UI.Combat` (trong `TimingFeedbackUI.cs`). Các file như `TimingSystem.cs` và `AudioController.cs` dùng `using TTCS.UI.Combat` để reference
- `CombatSceneManager` có `yield return null` sau EntityFactory.CreateParty/CreateWave — đây là "integration hook" để Dev B spawn CharacterViews trong cùng frame trước khi `StartBattle()` được gọi
- Float text pool size = 10 (ActionResultDisplay), turn order pool size = 8 (TurnOrderDisplay) — hardcoded, đủ cho một trận 3v3
- `AudioController` là `DontDestroyOnLoad` — nên chỉ có một instance xuyên suốt game, không spawn lại khi load scene

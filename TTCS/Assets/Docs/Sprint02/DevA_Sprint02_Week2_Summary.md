# ✅ Sprint 02 — Week 2 Summary (Developer A)

> **Dự án:** TTCS — Those at The Crossroads of Story  
> **Developer:** Developer A — UI + Timing System  
> **Tuần:** Tuần 2 (Ngày 6–10)  
> **Hoàn thành:** 2026-03-10  
> **Trạng thái:** ✅ COMPLETE — CombatBridge, TimingSystem, TimingFeedbackUI hoạt động đúng trong scene

---

## 1. Kết quả Tuần 2

Tuần 2 bổ sung **hệ thống động** lên nền UI tĩnh từ Tuần 1 — timing input của player và bridge kết nối engine với visual layer.

| Hệ thống | Files | Trạng thái |
|---------|-------|------------|
| Timing Window data | `TimingWindow.cs` | ✅ Done + In-scene verified |
| Timing System singleton | `TimingSystem.cs` | ✅ Done + In-scene verified |
| Timing Input Handler | `TimingInputHandler.cs` | ✅ Done + In-scene verified |
| Timing Feedback UI | `TimingFeedbackUI.cs` | ✅ Done + In-scene verified |
| Combat Integration Bridge | `CombatBridge.cs` | ✅ Done + In-scene verified |

**Tổng:** 5 files hệ thống động, tất cả đã test thành công trong scene `TestCombat`.

---

## 2. Files đã hoàn thành

```
Assets/Scripts/Combat/Timing/
  TimingWindow.cs         — Data class: openTime, duration, perfect/goodThreshold
  TimingSystem.cs         — Singleton, vòng đời window + input buffer, OnTimingResult event
  TimingInputHandler.cs   — New Input System (Guard action) + Space fallback

Assets/Scripts/UI/Combat/
  TimingFeedbackUI.cs     — Perfect/Good/Miss flash visual, DOTween animations

Assets/Scripts/Combat/Managers/
  CombatBridge.cs         — EventBus subscriber, ICharacterAnimatorBridge interface
```

---

## 3. Kiến trúc hệ thống Timing

```
[Player Input]
TimingInputHandler
  └── RegisterInput(Time.time) → TimingSystem
  
[TimingSystem] (Singleton)
  ├── OpenWindow(TimingWindow)  ← CombatBridge sẽ gọi khi enemy chuẩn bị tấn công
  ├── Coroutine lifecycle (duration based)
  ├── OnTimingResult (Action<TimingGrade>) ← CombatUIController subscribe
  └── GradeEvaluation:
        offset < 50ms  → Perfect
        offset < 150ms → Good
        timeout        → Miss

[TimingFeedbackUI]
  ├── ShowPerfect() — gold flash + "PERFECT!" text
  ├── ShowGood()    — blue tint + "GOOD" text
  └── ShowMiss()    — red tint + "MISS" text + screen shake
  
[CombatBridge]
  ├── Subscribe: DamageTakenEvent, HealingReceivedEvent, EntityDiedEvent
  ├── Subscribe: TurnStartedEvent, SkillUsedEvent, CombatEndedEvent
  └── ICharacterAnimatorBridge interface → Dev B implement
```

---

## 4. Test Results — Tuần 2

### Setup Scene (TestCombat — tiếp từ Tuần 1)

| GameObject | Component | Trạng thái |
|-----------|-----------|-----------|
| `TimingSystem` | `TimingSystem`, `TimingInputHandler` | ✅ Added |
| `CombatBridge` | `CombatBridge` | ✅ Added |
| `CombatCanvas` → `TimingFeedbackUI` | `TimingFeedbackUI` | ✅ Added |
| `TimingFeedbackUI` → `FlashOverlay` | `Image`, `CanvasGroup` | ✅ Configured |
| `TimingFeedbackUI` → `GradeText` | `TextMeshPro` | ✅ Configured |
| Inspector refs | `Flash Overlay`, `Grade Text` | ✅ Assigned |

### Test Cases Passed

| Test | Phím | Kết quả |
|------|------|---------|
| Mở timing window | `T` | Console: `"TimingSystem: Window opened..."` ✅ |
| Input ngay sau khi mở (Perfect) | `T` → `Space` (< 50ms) | Grade=Perfect + gold flash + "PERFECT!" ✅ |
| Input chậm (Good) | `T` → đợi → `Space` | Grade=Good + blue flash + "GOOD" ✅ |
| Không nhấn → Miss tự động | `T` → đợi 2s | Grade=Miss + red flash + "MISS" ✅ |
| Direct Perfect flash | `F` | Gold overlay + "PERFECT!" text ✅ |
| Direct Good flash | `G` | Blue overlay + "GOOD" text ✅ |
| Direct Miss flash | `M` | Red overlay + "MISS" text + shake ✅ |
| Tuần 1 keys không bị break | `H` / `Y` | HP bar vẫn hoạt động đúng ✅ |
| CombatBridge subscribe | Play mode | Console: `"CombatBridge: Subscribed..."` ✅ |

### Kết quả: **9/9 test cases PASSED — 0 bugs**

---

## 5. Interface ICharacterAnimatorBridge (cho Dev B)

`CombatBridge.cs` đã định nghĩa interface mà Dev B cần implement:

```csharp
public interface ICharacterAnimatorBridge
{
    void PlayAttack();
    void PlayHurt();
    void PlayDeath();
    void PlayVictory();
    Transform GetWorldTransform();
}
```

Cách Dev B sử dụng:
```csharp
// 1. Implement interface trong CharacterView
public class CharacterView : MonoBehaviour, ICharacterAnimatorBridge { ... }

// 2. Đăng ký với CombatBridge
CombatBridge.Instance.RegisterView("char_warrior", myCharacterView);

// 3. Đăng ký world position với CombatUIController
CombatUIController.Instance.RegisterEntityPosition("char_warrior", myTransform);
```

---

## 6. Bài học rút ra

1. **DOTween Init:** Cần gọi `DOTween.Init()` sớm (trong Awake) — nếu animation không chạy khi test, kiểm tra DOTween có được khởi tạo chưa.

2. **TimingInputHandler fallback:** Không có `PlayerInput` component là bình thường khi testing — handler tự dùng `Space` key và log cảnh báo. Không cần setup `PlayerInput` cho đến khi full game.

3. **CombatBridge self-subscribe:** `CombatBridge` subscribe EventBus trong `OnEnable()` và unsubscribe trong `OnDisable()` — không cần gán gì trong Inspector.

4. **CanvasGroup alpha cho flash:** Set `alpha = 0` cho `CanvasGroup` trên `FlashOverlay`, không phải trực tiếp trên `Image.color.a` — DOTween fade qua `CanvasGroup` sẽ smooth hơn và bao gồm cả children.

---

## 7. Trạng thái sau Tuần 2

### Dev A — Toàn bộ files đã complete:

| File | Đường dẫn | Trạng thái |
|------|-----------|-----------|
| `CombatUIController.cs` | `Scripts/UI/Combat/` | ✅ Done |
| `BattleHUD.cs` | `Scripts/UI/Combat/` | ✅ Done |
| `SkillButtonPanel.cs` | `Scripts/UI/Combat/` | ✅ Done |
| `SkillButton.cs` | `Scripts/UI/Combat/` | ✅ Done |
| `TurnOrderDisplay.cs` | `Scripts/UI/Combat/` | ✅ Done |
| `TurnOrderSlot.cs` | `Scripts/UI/Combat/` | ✅ Done |
| `ActionResultDisplay.cs` | `Scripts/UI/Combat/` | ✅ Done |
| `FloatingText.cs` | `Scripts/UI/Combat/` | ✅ Done |
| `TimingFeedbackUI.cs` | `Scripts/UI/Combat/` | ✅ Done |
| `TimingSystem.cs` | `Scripts/Combat/Timing/` | ✅ Done |
| `TimingInputHandler.cs` | `Scripts/Combat/Timing/` | ✅ Done |
| `TimingWindow.cs` | `Scripts/Combat/Timing/` | ✅ Done |
| `CombatBridge.cs` | `Scripts/Combat/Managers/` | ✅ Done |
| `CombatSceneManager.cs` | `Scripts/Combat/Managers/` | ✅ Done |
| `AudioController.cs` | `Scripts/Audio/` | ✅ Done |

**Tổng Dev A: 15/15 files ✅ HOÀN THÀNH**

---

## 8. Pending cho Tuần 3

| Hạng mục | Lý do chưa làm | Ưu tiên |
|---------|----------------|---------|
| Wire `TimingSystem` vào `CombatFlowController` | Cần modify Sprint 1 files | 🔴 High |
| Modify `ActionResolver.cs` với `TimingGrade` | Phụ thuộc vào CombatFlowController | 🔴 High |
| Full integration với Dev B views | Dev B chưa hoàn thành | 🟡 Blocked |
| `CombatScene.unity` final setup | Dev B ownership | 🟡 Blocked |

Xem `DevA_Week3_Setup_Guide.md` để tiếp tục.

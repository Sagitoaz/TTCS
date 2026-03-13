# 📊 Sprint 02 — Theo dõi Tiến độ

> **Dự án**: TTCS — Those at The Crossroads of Story  
> **Sprint 2**: Visual Combat Layer  
> **Thời gian**: 3 tuần (15 ngày làm việc)  
> **Cập nhật lần cuối**: Dev A Tuần 3 hoàn thành (2026-03-11), Dev B đang thực hiện

---

## 🏁 Tổng quan tiến độ

| Team Member | Số file | Hoàn thành | Còn lại | Trạng thái |
|-------------|---------|------------|---------|------------|
| **Developer A** | 15 files + tuần 3 | 15 | 0 (chờ joint test) | ✅ HOÀN THÀNH (phần độc lập) |
| **Developer B** | 9 files | 0 | 9 | 🔄 ĐANG LÀM |
| **Shared (Scene)** | 1 scene | 0 | 1 | ⏳ CHỜ DEV B |
| **Tổng cộng** | 25 items | 15 | 10 | 🔄 60% |

---

## 🔵 Developer A — UI + Timing System (✅ HOÀN THÀNH)

### Unit A — Combat UI System (9 files)

| # | File | Đường dẫn | Trạng thái | Ghi chú |
|---|------|-----------|------------|---------|
| 1 | `FloatingText.cs` | `Scripts/UI/Combat/` | ✅ Done | Pool callback via `Action<FloatingText>` |
| 2 | `ActionResultDisplay.cs` | `Scripts/UI/Combat/` | ✅ Done | Tự subscribe damage/heal events |
| 3 | `TurnOrderSlot.cs` | `Scripts/UI/Combat/` | ✅ Done | Gold outline cho current actor |
| 4 | `TurnOrderDisplay.cs` | `Scripts/UI/Combat/` | ✅ Done | Pool 8 slot, rebuild on TimelineUpdatedEvent |
| 5 | `SkillButton.cs` | `Scripts/UI/Combat/` | ✅ Done | DOTween punch khi click |
| 6 | `SkillButtonPanel.cs` | `Scripts/UI/Combat/` | ✅ Done | Auto target resolve + SubmitPlayerAction |
| 7 | `BattleHUD.cs` | `Scripts/UI/Combat/` | ✅ Done | DOTween lerp HP bars |
| 8 | `TimingFeedbackUI.cs` | `Scripts/UI/Combat/` | ✅ Done | Định nghĩa `TimingGrade` enum |
| 9 | `CombatUIController.cs` | `Scripts/UI/Combat/` | ✅ Done | Root singleton, cầu nối UI ↔ Engine |

### Unit C — Timing System (3 files)

| # | File | Đường dẫn | Trạng thái | Ghi chú |
|---|------|-----------|------------|---------|
| 10 | `TimingWindow.cs` | `Scripts/Combat/Timing/` | ✅ Done | Data class, PerfectThreshold=50ms |
| 11 | `TimingSystem.cs` | `Scripts/Combat/Timing/` | ✅ Done | 60ms input buffer |
| 12 | `TimingInputHandler.cs` | `Scripts/Combat/Timing/` | ✅ Done | New Input System + Space fallback |

### Unit D — Combat Integration Bridge (2 files)

| # | File | Đường dẫn | Trạng thái | Ghi chú |
|---|------|-----------|------------|---------|
| 13 | `CombatBridge.cs` | `Scripts/Combat/Managers/` | ✅ Done | Defines `ICharacterAnimatorBridge` |
| 14 | `CombatSceneManager.cs` | `Scripts/Combat/Managers/` | ✅ Done | Replaces CombatTestLoader |

### Unit E — Audio (1 file)

| # | File | Đường dẫn | Trạng thái | Ghi chú |
|---|------|-----------|------------|---------|
| 15 | `AudioController.cs` | `Scripts/Audio/` | ✅ Done | DontDestroyOnLoad, event-driven SFX |

---

## 🟢 Developer B — Character Visual System (⏳ ĐANG LÀM)

### Unit B — Character Visual System (6 files)

| # | File | Đường dẫn | Trạng thái | Ghi chú |
|---|------|-----------|------------|---------|
| B1 | `CharacterView.cs` | `Scripts/Visual/` | ⬜ Chưa làm | Cần cho CombatBridge integration |
| B2 | `CharacterAnimator.cs` | `Scripts/Visual/` | ⬜ Chưa làm | Cần `OnAttackHitFrame` event |
| B3 | `EnemyView.cs` | `Scripts/Visual/` | ⬜ Chưa làm | Cần có `ShowPhaseTransition()` |
| B4 | `TelegraphVisual.cs` | `Scripts/Visual/` | ⬜ Chưa làm | Ring shrink + flash, expose duration |
| B5 | `CharacterViewFactory.cs` | `Scripts/Visual/` | ⬜ Chưa làm | Instantiate prefabs từ Resources |
| B6 | `VFXController.cs` | `Scripts/Visual/VFX/` | ⬜ Chưa làm | Particle system pooling |

### Unit D (phần Dev B) — ActionAnimationController (1 file)

| # | File | Đường dẫn | Trạng thái | Ghi chú |
|---|------|-----------|------------|---------|
| B7 | `ActionAnimationController.cs` | `Scripts/Combat/Managers/` | ⬜ Chưa làm | Implement `ICharacterAnimatorBridge` |

### Unit E (phần Dev B) — Scene Setup (2 items)

| # | Item | Đường dẫn | Trạng thái | Ghi chú |
|---|------|-----------|------------|---------|
| B8 | Prefabs (characters + enemies) | `Assets/Prefabs/Combat/` | ⬜ Chưa làm | art parts assignment |
| B9 | `CombatScene.unity` | `Assets/Scenes/` | ⬜ Chưa làm | Canvas hierarchy + camera setup |

---

## 🔗 Integration Points

| Ngày | Điểm tích hợp | Dev A | Dev B | Trạng thái |
|------|---------------|-------|-------|------------|
| Ngày 7 | CombatBridge API sync | ✅ `ICharacterAnimatorBridge` defined | ⬜ Cần implement | ⚠️ Pending Dev B |
| Ngày 10 | Mid-sprint integration test | ✅ CombatBridge ready | ⬜ CharacterView cần | ⚠️ Pending Dev B |
| Ngày 12 | Timing + Telegraph sync | ✅ `NotifyTelegraphComplete()` ready | ⬜ TelegraphVisual cần | ⚠️ Pending Dev B |
| Ngày 14–15 | Full integration | ✅ Tất cả hệ thống ready | ⬜ Scene cần hoàn thành | ⚠️ Pending Dev B |

---

## 📋 Danh sách việc Dev B cần làm để unblock integration

Sau khi Dev B hoàn thành, cần gọi các API Dev A đã chuẩn bị:

```csharp
// 1. Implement ICharacterAnimatorBridge trong CharacterView/EnemyView
// (interface nằm trong CombatBridge.cs)
public interface ICharacterAnimatorBridge
{
    void PlayAttack();
    void PlayHurt();
    void PlayDeath();
    void PlayVictory();
    Transform GetWorldTransform();
}

// 2. Đăng ký view với CombatBridge
CombatBridge.Instance.RegisterView("entityId", myCharacterView);

// 3. Đăng ký vị trí với CombatUIController
CombatUIController.Instance.RegisterEntityPosition("entityId", myTransform);

// 4. Gọi NotifyTelegraphComplete khi telegraph animation kết thúc
CombatBridge.Instance.NotifyTelegraphComplete("enemyId", telegraphDuration);
```

---

## 🧪 Kiểm thử

| Module | Test file | Trạng thái |
|--------|-----------|------------|
| TimingWindow logic | `TimingWindowTest.cs` | ✅ Done |
| TimingSystem lifecycle | `TimingSystemTest.cs` | ✅ Done |
| CombatBridge + ICharacterAnimatorBridge | `CombatBridgeTest.cs` | ✅ Done |
| CombatUIController entity management | `CombatUIControllerTest.cs` | ✅ Done |
| AudioController volume + events | `AudioControllerTest.cs` | ✅ Done |
| BattleHUD slot management | `BattleHUDTest.cs` | ✅ Done |
| FloatingText pool behavior | `FloatingTextPoolTest.cs` | ✅ Done |
| SkillButtonPanel target resolution | `SkillButtonPanelTest.cs` | ✅ Done |
| **Tuần 1 in-scene** | T/H/Y/V keys in TestCombat | ✅ Done (2026-03-07) |
| **Tuần 2 in-scene** | T/Space/F/G/M keys in TestCombat | ✅ Done (2026-03-10) |
| **Tuần 3 in-scene** | Enemy guard window, player attack timing, damage formula verify | ✅ Done (2026-03-11) |
| **Joint test** | CombatScene.unity full flow với Dev B | ⏳ Chờ merge Dev B |

---

## 📌 Ghi chú

- Dev A có thể test độc lập toàn bộ 16 files nhờ đã dùng Interface (`ICharacterAnimatorBridge`) thay vì concrete class của Dev B
- Khi Dev B xong, chỉ cần: (1) implement interface, (2) đăng ký với `CombatBridge.RegisterView()`, (3) wire trong CombatScene.unity
- `CombatSceneManager.cs` có `yield return null` giữa EntityFactory và `StartBattle()` — đây là "hook" cho Dev B để spawn visual trong một frame

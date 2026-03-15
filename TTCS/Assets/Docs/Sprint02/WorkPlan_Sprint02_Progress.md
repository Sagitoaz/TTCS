# 📊 Sprint 02 — Theo dõi Tiến độ

> **Dự án**: TTCS — Those at The Crossroads of Story  
> **Sprint 2**: Visual Combat Layer  
> **Thời gian**: 3 tuần (15 ngày làm việc)  
> **Cập nhật lần cuối**: 2026-03-15 — Merge status Dev A + Dev B cho demo

---

## 🏁 Tổng quan tiến độ

| Team Member | Số file | Hoàn thành | Còn lại | Trạng thái |
|-------------|---------|------------|---------|------------|
| **Developer A** | 15 files + tuần 3 | 15 | 0 (chờ joint test) | ✅ HOÀN THÀNH (phần độc lập) |
| **Developer B** | 9 items | 7 | 2 | 🔄 GẦN HOÀN THÀNH |
| **Shared (Scene)** | 1 scene | 0 | 1 | ⏳ CHỜ JOINT TEST |
| **Tổng cộng** | 25 items | 22 | 3 | 🔄 88% |

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

## 🟢 Developer B — Character Visual System (🔄 ĐÃ MERGE SCRIPT CORE)

### Unit B — Character Visual System (6 files)

| # | File | Đường dẫn | Trạng thái | Ghi chú |
|---|------|-----------|------------|---------|
| B1 | `CharacterView.cs` | `Scripts/Visual/` | ✅ Done | Đã implement `ICharacterAnimatorBridge` |
| B2 | `CharacterAnimator.cs` | `Scripts/Visual/` | ✅ Done | Có `OnAttackHitFrame` + fallback timer |
| B3 | `EnemyView.cs` | `Scripts/Visual/` | ✅ Done | Có API `ShowTelegraph()`/`HideTelegraph()` |
| B4 | `TelegraphVisual.cs` | `Scripts/Visual/` | ✅ Done | Ring shrink + warning color shift + event complete |
| B5 | `CharacterViewFactory.cs` | `Scripts/Visual/` | ✅ Done | Load prefab từ `Resources/Prefabs/Combat/...` |
| B6 | `VFXController.cs` | `Scripts/Visual/VFX/` | ✅ Done | Event-driven + object pooling |

### Unit D (phần Dev B) — ActionAnimationController (1 file)

| # | File | Đường dẫn | Trạng thái | Ghi chú |
|---|------|-----------|------------|---------|
| B7 | `ActionAnimationController.cs` | `Scripts/Combat/Managers/` | ✅ Done | Registry + sequence attack/hurt/death/victory |

### Unit E (phần Dev B) — Scene Setup (2 items)

| # | Item | Đường dẫn | Trạng thái | Ghi chú |
|---|------|-----------|------------|---------|
| B8 | Prefabs (characters + enemies) | `Assets/Resources/Prefabs/Combat/` | 🔄 In progress | Đã có `char_warrior`, `char_mage`, `enemy_bandit`; cần chốt full demo set |
| B9 | `CombatScene.unity` | `Assets/Scenes/` | ⬜ Chưa làm | Chưa thấy scene file trong repo |

---

## 🔗 Integration Points

| Ngày | Điểm tích hợp | Dev A | Dev B | Trạng thái |
|------|---------------|-------|-------|------------|
| Ngày 7 | CombatBridge API sync | ✅ `ICharacterAnimatorBridge` defined | ✅ Interface đã implement | ✅ Done |
| Ngày 10 | Mid-sprint integration test | ✅ CombatBridge ready | ✅ ActionAnimationController + views đã có | 🔄 Cần test scene chung |
| Ngày 12 | Timing + Telegraph sync | ✅ `NotifyTelegraphComplete()` ready | 🔄 Telegraph event có nhưng chưa wire sang bridge | ⚠️ Cần nối callback |
| Ngày 14–15 | Full integration | ✅ Tất cả hệ thống A ready | 🔄 Scene/prefab final pass | ⚠️ Còn blocker scene |

---

## 📋 Danh sách việc còn lại để chốt demo

Các API Dev A đã sẵn sàng. Các việc còn lại tập trung vào wire scene + callback:

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

// 2. Đăng ký view với CombatBridge (đã có hook trong CombatSceneManager)
CombatBridge.Instance.RegisterView("entityId", myCharacterView);

// 3. Đăng ký vị trí với CombatUIController
CombatUIController.Instance.RegisterEntityPosition("entityId", myTransform);

// 4. Cần wire callback này khi telegraph animation kết thúc
CombatBridge.Instance.NotifyTelegraphComplete("enemyId", telegraphDuration);
```

### Blockers hiện tại

- `CombatScene.unity` chưa có trong `Assets/Scenes/` để chạy full loop demo.
- Callback `TelegraphVisual.OnTelegraphComplete` chưa thấy được nối trực tiếp vào `CombatBridge.NotifyTelegraphComplete()`.
- Prefab demo hiện có đủ để test cơ bản, nhưng chưa xác nhận đủ bộ theo kế hoạch cuối sprint.

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
| **Joint test** | CombatScene.unity full flow Dev A + Dev B | ⏳ Chờ hoàn tất scene + wire telegraph callback |

---

## 📌 Ghi chú

- Dev A + Dev B đã merge được phần script nền: registry view, animation flow, timing core.
- `CombatSceneManager.cs` đã có đăng ký view cho cả `ActionAnimationController` và `CombatBridge`.
- Mốc kế tiếp để hoàn thành demo: tạo scene combat chính thức, wire telegraph callback, rồi chạy joint test end-to-end.

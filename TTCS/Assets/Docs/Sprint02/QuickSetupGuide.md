# 🚀 Quick Setup Guide - TTCS Sprint 02

> Hướng dẫn setup nhanh để chạy luồng combat visual của Sprint02

---

## 1. Pre-check

- [ ] Đã pull code mới nhất có merge Dev A + Dev B
- [ ] Unity project mở không có compile error blocker
- [ ] Các package cần thiết (Input System, TextMeshPro, DOTween nếu dùng) đã cài và init

---

## 2. Thành phần bắt buộc trong scene test

- [ ] EventBus
- [ ] DataManager
- [ ] TurnManager
- [ ] SkillManager
- [ ] CombatFlowController
- [ ] CombatSceneManager
- [ ] CombatBridge
- [ ] TimingSystem + TimingInputHandler
- [ ] CombatCanvas + CombatUIController

---

## 3. UI hierarchy tối thiểu

```
CombatCanvas
├── BattleHUD
├── SkillButtonPanel
├── TurnOrderDisplay
├── ActionResultDisplay
└── TimingFeedbackUI
    ├── FlashOverlay (CanvasGroup)
    └── GradeText (TMP)
```

---

## 4. Visual hierarchy tối thiểu

- Character/Enemy prefab có đủ part renderers theo chuẩn team đang dùng.
- View object đăng ký được với bridge bằng entityId.
- CharacterAnimator có state cơ bản: Idle, Attack, Hurt, Death.

---

## 5. Wiring checklist quan trọng

- [ ] `CombatBridge.RegisterView(entityId, view)` được gọi sau khi spawn visual.
- [ ] `CombatUIController.RegisterEntityPosition(entityId, transform)` được gọi để floating text đúng vị trí.
- [ ] `TimingSystem.OnTimingResult` được nối sang `CombatUIController.ShowTimingResult`.
- [ ] Telegraph kết thúc có gọi `CombatBridge.NotifyTelegraphComplete(...)`.

---

## 6. Keyboard smoke test

- `T` mở timing window.
- `Space` gửi timing input.
- `F/G/M` test trực tiếp feedback Perfect/Good/Miss.
- `H/Y/V` kiểm tra damage/heal/end battle flow.

Kết quả mong đợi:
- UI cập nhật HP/MP và turn order đúng.
- Floating text hiển thị đúng vị trí và màu.
- Character animation phản hồi theo action.
- Timing grade hiển thị đúng và có hiệu ứng visual/audio.

---

## 7. File tham khảo nhanh

- `Sprint02_Summary.md`
- `ProjectDescription_Sprint02.md`
- `TechnicalDescription_Sprint02.md`
- `Dev_A_Sprint02_Summary.md`
- `Dev_B_Sprint02_Summary.md`
- `WorkPlan_Sprint02_Progress.md`

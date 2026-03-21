# 📦 Sprint 02 — Tổng kết & Hướng dẫn Sử dụng

> **Dự án**: TTCS — Those at The Crossroads of Story  
> **Sprint 2 hoàn thành**: 2026-03-21  
> **Scope**: Visual Combat Layer — UI, Character Visual, Animation, Timing, VFX, Audio, Integration

---

## 1. Tổng quan những gì đã build

Sprint 2 đưa combat engine (Sprint 1) lên màn hình với vòng lặp combat có phản hồi đầy đủ:

```
UI Layer          → CombatUIController → BattleHUD, SkillButtonPanel, TurnOrderDisplay
Result Feedback   → ActionResultDisplay + FloatingText + TimingFeedbackUI
Timing Mechanics  → TimingWindow + TimingSystem + TimingInputHandler
Visual Characters → CharacterView, EnemyView, CharacterAnimator, CharacterViewFactory
Animation Bridge  → CombatBridge + ActionAnimationController + ICharacterAnimatorBridge
Telegraph/VFX     → TelegraphVisual + VFXController
Audio             → AudioController (event-driven SFX/BGM)
Scene Orchestration → CombatSceneManager (scene bootstrap + integration hook)
```

---

## 2. Deliverables theo developer

### Developer A
- Combat UI system (BattleHUD, skill buttons, turn order, floating text)
- Timing system và timing feedback
- CombatBridge, CombatSceneManager, AudioController
- Timing integration vào combat flow

### Developer B
- Character/Enemy visual wrappers
- CharacterAnimator và action animation flow
- Telegraph visual và VFX controller
- Factory spawn visual object + wiring view registration
- Hoàn tất scene setup để chạy full combat visual flow

---

## 3. Cấu trúc mã nguồn chính (Sprint 2)

```
Assets/Scripts/
├── UI/Combat/
│   ├── CombatUIController.cs
│   ├── BattleHUD.cs
│   ├── SkillButtonPanel.cs
│   ├── SkillButton.cs
│   ├── TurnOrderDisplay.cs
│   ├── TurnOrderSlot.cs
│   ├── ActionResultDisplay.cs
│   ├── FloatingText.cs
│   ├── TimingFeedbackUI.cs
│   └── TimingWindowUI.cs
│
├── Combat/Timing/
│   ├── TimingWindow.cs
│   ├── TimingSystem.cs
│   └── TimingInputHandler.cs
│
├── Combat/Managers/
│   ├── CombatSceneManager.cs
│   ├── CombatBridge.cs
│   └── ActionAnimationController.cs
│
├── Visual/
│   ├── CharacterView.cs
│   ├── CharacterAnimator.cs
│   ├── EnemyView.cs
│   ├── CharacterViewFactory.cs
│   ├── TelegraphVisual.cs
│   └── VFX/VFXController.cs
│
└── Audio/
    └── AudioController.cs
```

---

## 4. Luồng runtime chuẩn trong Sprint 2

1. CombatSceneManager khởi tạo team/combat context.
2. CombatUIController bind data entity vào HUD và panel.
3. CharacterViewFactory spawn visual cho player/enemy.
4. Mỗi view đăng ký với CombatBridge bằng entityId.
5. CombatFlowController chạy turn loop.
6. EventBus phát event damage/heal/turn/skill.
7. Bridge + UI + Visual cùng phản hồi:
   - HUD cập nhật HP/MP.
   - FloatingText hiển thị damage/heal.
   - Animator phát attack/hurt/death.
   - TimingSystem mở window khi telegraph hoàn tất.
   - TimingFeedbackUI hiển thị Perfect/Good/Miss.

---

## 5. Kết quả hoàn thành

- ✅ Dev A hoàn tất các module UI/Timing/Bridge/Audio
- ✅ Dev B hoàn tất các module Visual/Animation/VFX/Scene integration
- ✅ Hai nhánh đã merge
- ✅ Joint integration test đã pass theo checklist Sprint02
- ✅ Sprint02 đạt mục tiêu "Visual Combat Layer"

---

## 6. Tài liệu nên đọc tiếp

- `WorkPlan_Sprint02.md` — kế hoạch gốc và chia unit
- `WorkPlan_Sprint02_Progress.md` — tiến độ cuối cùng sau merge
- `ProjectDescription_Sprint02.md` — mô tả sản phẩm Sprint02
- `TechnicalDescription_Sprint02.md` — mô tả kỹ thuật Sprint02
- `QuickSetupGuide.md` — setup nhanh cho dev/QA

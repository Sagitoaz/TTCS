# 🧠 Sprint 02 — Technical Description

> **Phạm vi kỹ thuật**: Combat visual runtime architecture cho TTCS

---

## 1. Kiến trúc kỹ thuật tổng quan

Sprint 02 dùng mô hình tách lớp:

- Core combat logic giữ nguyên từ Sprint 1.
- Visual layer subscribe/bridge từ EventBus để phản hồi theo sự kiện.
- UI layer chịu trách nhiệm hiển thị trạng thái và input người chơi.

```
Core Engine (Sprint 1)
  ↓ publishes
EventBus
  ↓ consumed by
CombatBridge + UI Controllers + AudioController
  ↓ drives
CharacterView/Animator + HUD + VFX + Timing Feedback
```

---

## 2. Các module chính

### UI Combat
- CombatUIController: entrypoint điều phối UI, bind entity data.
- BattleHUD: HP/MP bars cho cả 2 phe.
- SkillButtonPanel/SkillButton: tương tác skill và trạng thái cooldown/cost.
- TurnOrderDisplay/TurnOrderSlot: hiển thị queue actor hiện tại và sắp tới.
- ActionResultDisplay/FloatingText: hiển thị damage/heal pop-up.
- TimingFeedbackUI: phản hồi grade timing.

### Timing System
- TimingWindow: mô hình dữ liệu cho cửa sổ timing.
- TimingSystem: vòng đời window, input registration, result dispatch.
- TimingInputHandler: nhận input guard và gửi vào TimingSystem.

### Visual/Animation
- CharacterView/EnemyView: wrapper cho cấu trúc part-based character.
- CharacterAnimator: phát animation state và callback hit frame.
- TelegraphVisual: cảnh báo pre-attack để người chơi chuẩn bị timing.
- CharacterViewFactory: spawn và cấu hình visual object theo entity.
- VFXController: chạy hiệu ứng visual theo combat events.

### Integration & Scene
- CombatBridge: map event-to-visual action; giữ interface animator bridge.
- ActionAnimationController: dàn dựng sequence attack/hit/return.
- CombatSceneManager: bootstrap scene, gắn các subsystem.

### Audio
- AudioController: phát SFX/BGM theo event-driven model.

---

## 3. Integration contract giữa Dev A và Dev B

- Dùng `ICharacterAnimatorBridge` để bridge animation call.
- Giao tiếp theo `entityId` để liên kết combat entity với visual object.
- `CombatBridge.RegisterView(...)` và `CombatUIController.RegisterEntityPosition(...)` là hai điểm nối bắt buộc.

Lợi ích:
- Giảm coupling giữa engine và class visual cụ thể.
- Dễ thay đổi prefab/animator mà không sửa engine flow.
- Dễ test độc lập từng phía trước khi merge.

---

## 4. Runtime sequence rút gọn

1. Scene init tạo entities.
2. Visual factory spawn views và đăng ký bridge.
3. Turn loop chạy trong combat flow.
4. Khi skill cast/damage/heal xảy ra, event được publish.
5. UI + animator + VFX + audio cập nhật đồng thời.
6. Với enemy telegraph, TimingSystem mở guard window.
7. Timing result trả về và ảnh hưởng damage resolution.

---

## 5. Kỹ thuật đáng chú ý

- Event-driven visual updates thay vì polling liên tục.
- Interface-based bridge để tách ownership module theo developer.
- Timing thresholds để định lượng phản xạ người chơi.
- UI object pooling (floating text/slots) để giảm allocate runtime.
- DOTween/Animator phối hợp cho animation cảm giác mượt.

---

## 6. Khả năng mở rộng sang Sprint 03+

- Có thể thêm animation states nâng cao mà không đổi engine API.
- Có thể mở rộng timing theo từng skill/enemy archetype.
- Có thể thêm camera system, post-process, combo feedback trên bridge hiện tại.
- Có thể nâng cấp audio routing (mixer buses, category volumes) mà không phá flow hiện có.

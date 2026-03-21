# 📦 Sprint 02 — Dev B Deliverables Summary

> **Dự án**: TTCS — Those at The Crossroads of Story  
> **Developer**: Developer B — Character Visual + Animation + VFX  
> **Hoàn thành**: Sprint 2 merge completed (2026-03-21)  
> **Trạng thái**: ✅ Hoàn thành và đã tích hợp với Dev A

---

## 1. Tổng quan những gì đã build

Dev B triển khai toàn bộ visual representation của actor trong combat:

```
View Layer        → CharacterView, EnemyView
Animation Layer   → CharacterAnimator
Factory Layer     → CharacterViewFactory
Combat Sequences  → ActionAnimationController
Telegraph         → TelegraphVisual
Effects           → VFXController
```

Các thành phần này được nối với hệ thống Dev A thông qua bridge + entity mapping.

---

## 2. Cấu trúc file chính

```
Assets/Scripts/Visual/
├── CharacterView.cs
├── CharacterAnimator.cs
├── EnemyView.cs
├── CharacterViewFactory.cs
├── TelegraphVisual.cs
└── VFX/
    └── VFXController.cs

Assets/Scripts/Combat/Managers/
└── ActionAnimationController.cs
```

---

## 3. Vai trò kỹ thuật từng module

### CharacterView / EnemyView
- Giữ reference đến các thành phần visual của actor.
- Cung cấp API mức view cho highlight, facing, và bridge registration.

### CharacterAnimator
- Điều phối animation state theo action combat.
- Tách logic animation khỏi flow manager để dễ test và thay prefab.

### CharacterViewFactory
- Spawn và cấu hình visual objects theo entity data.
- Chuẩn hóa flow tạo actor visual trong scene.

### TelegraphVisual
- Hiển thị cảnh báo trước enemy action.
- Phối hợp thời gian với timing window để tạo nhịp phản xạ.

### VFXController
- Điểm vào cho hit effect/skill effect trong combat.
- Giảm coupling khi thay đổi hiệu ứng giữa các enemy/skill.

### ActionAnimationController
- Thực thi sequence tấn công ở mức scene runtime.
- Đồng bộ attacker, target, hit frame, feedback và return motion.

---

## 4. Điểm tích hợp với hệ thống Dev A

- Đăng ký view vào bridge qua `entityId`.
- Mapping world transform cho UI feedback positioning.
- Tương tác telegraph với timing system qua callback bridge.
- Đồng bộ animation sequence với combat events.

---

## 5. Kết quả sau merge

- ✅ Character visual pipeline hoạt động với combat runtime
- ✅ Animation + damage/heal feedback được đồng bộ
- ✅ Telegraph + timing feedback flow khớp design Sprint02
- ✅ Các điểm nối với CombatBridge và CombatUIController đã ổn định

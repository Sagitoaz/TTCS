# Sprint 02 — Kiến Trúc Hệ Thống

> **Tài liệu này mô tả toàn bộ kiến trúc code của Sprint 02.**
> Sprint 02 xây dựng Visual Combat Layer — thêm UI, VFX, Animation, Audio để hiển thị combat engine từ Sprint 01.

---

## Mục lục

| File | Nội dung |
|------|----------|
| [01_System_Overview.md](./01_System_Overview.md) | Tổng quan kiến trúc, design pattern, nguyên tắc thiết kế |
| [02_Dependency_Map.md](./02_Dependency_Map.md) | Sơ đồ phụ thuộc giữa Visual Layer và Sprint 01 Combat Engine |
| [03_Visual_Framework.md](./03_Visual_Framework.md) | CharacterView, CharacterAnimator, TelegraphVisual, VFXController |
| [04_Animation_System.md](./04_Animation_System.md) | ActionAnimationController, AnimationStateManager, Timing & Sequencing |
| [05_UI_Layer.md](./05_UI_Layer.md) | CombatUI, ActionPanel, StatusDisplay, EffectIndicator, UIManager |
| [06_Audio_System.md](./06_Audio_System.md) | AudioManager, SoundEffectController, MusicController, Timing Sync |
| [07_Event_Integration.md](./07_Event_Integration.md) | Bridge Pattern — kết nối Visual Layer với Sprint 01 EventBus |
| [08_Runtime_Flows.md](./08_Runtime_Flows.md) | Call chain chi tiết khi skill được thực thi, animation, VFX, UI update |

---

## Cách đọc khuyến nghị

1. **Đọc `01`** để có bức tranh tổng thể vai trò của Visual Layer.
2. **Đọc `02`** để thấy độc lập nằm ở đâu giữa Sprint 01 và Sprint 02.
3. **Đọc `03–06`** để hiểu từng module visual (View, Animation, UI, Audio).
4. **Đọc `07`** để hiểu cách kết nối hai sprint.
5. **Đọc `08`** để trace call chain theo từng visual scenario.

---

## Namespace map (Sprint 02 additions)

| Namespace | Thư mục | Mô tả |
|-----------|---------|-------|
| `TTCS.Visual.Views` | `Scripts/Visual/Views/` | Character views, Entity visuals |
| `TTCS.Visual.Animation` | `Scripts/Visual/Animation/` | Animation controllers, state machines |
| `TTCS.Visual.Effects` | `Scripts/Visual/Effects/` | VFX, particle effects, telegraph |
| `TTCS.Visual.UI` | `Scripts/Visual/UI/` | Combat UI panels, HUD elements |
| `TTCS.Visual.Audio` | `Scripts/Visual/Audio/` | Audio management, sound effects |
| `TTCS.Visual.Bridge` | `Scripts/Visual/Bridge/` | Event bridge, integration with Sprint01 |

---

## Design Principles — Sprint 02

1. **Decoupling**: Visual layer hoàn toàn độc lập từ Combat Engine (Sprint 01). Tất cả communication qua EventBus.
2. **Event-Driven**: Không gọi trực tiếp Combat → Visual. Thay vào đó combat publish event, visual subscribe.
3. **Timing Windows**: Visual effect có exact timing window tương ứng với combat logic.
4. **Pooling & Performance**: UI elements, VFX, Audio được pool để tránh allocation/GC spike.
5. **Scalability**: Design để dễ mở rộng sang Sprint 03 (Network, Replay, etc).

---

## Key Dependencies

```
Sprint01 (Combat Engine)
    ↓ publish events
EventBus
    ↑ subscribe
Sprint02 (Visual Layer)
    ├─ Visual.Views (Character, Enemy rendering)
    ├─ Visual.Animation (Movement, Attack sequences)
    ├─ Visual.Effects (VFX, Telegraph, Particle effects)
    ├─ Visual.UI (Panels, HUD, Information display)
    ├─ Visual.Audio (Sound, Music sync)
    └─ Visual.Bridge (Event adapters, synchronization)
```

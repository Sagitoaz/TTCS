# Sprint 01 — Kiến Trúc Hệ Thống

> **Tài liệu này mô tả toàn bộ kiến trúc code của Sprint 01.**
> Sprint 01 xây dựng combat engine hoàn chỉnh chạy độc lập (không cần UI/Visual).

---

## Mục lục

| File | Nội dung |
|------|----------|
| [01_System_Overview.md](./01_System_Overview.md) | Tổng quan kiến trúc, cấu trúc thư mục, nguyên tắc thiết kế |
| [02_Dependency_Map.md](./02_Dependency_Map.md) | Sơ đồ phụ thuộc giữa tất cả class (Mermaid diagrams) |
| [03_Data_Layer.md](./03_Data_Layer.md) | DataManager, DataCache, DataValidator, DataModel, SaveManager |
| [04_Entity_Component.md](./04_Entity_Component.md) | CombatEntity, Character, Enemy, EntityFactory, 3 Component, StatusEffect |
| [05_Combat_Managers.md](./05_Combat_Managers.md) | CombatFlowController, TurnManager, SkillManager — state machine và CTB |
| [06_Action_Pipeline_AI.md](./06_Action_Pipeline_AI.md) | IAction, SkillAction, ActionValidator, ActionResolver, StatCalculator, AI |
| [07_Infrastructure.md](./07_Infrastructure.md) | EventBus, RNGService, Constants, DebugLogger, CombatLogger |
| [08_Runtime_Flows.md](./08_Runtime_Flows.md) | Call chain chi tiết của tất cả luồng quan trọng |

---

## Cách đọc khuyến nghị

1. **Đọc `01`** để có bức tranh tổng thể.
2. **Đọc `02`** để thấy dependency giữa các class.
3. **Đọc `03–06`** để hiểu từng nhóm module.
4. **Đọc `08`** để trace call chain theo từng scenario.

---

## Namespace map

| Namespace | Thư mục |
|-----------|---------|
| `TTCS.Core.Events` | `Scripts/Core/Events/` |
| `TTCS.Core.Data` | `Scripts/Core/Data/` |
| `TTCS.Core.Save` | `Scripts/Core/Save/` |
| `TTCS.Core.Utilities` | `Scripts/Core/Utilities/` |
| `TTCS.Combat.Entities` | `Scripts/Combat/Entities/` |
| `TTCS.Combat.Components` | `Scripts/Combat/Components/` |
| `TTCS.Combat.Effects` | `Scripts/Combat/Effects/` |
| `TTCS.Combat.Stats` | `Scripts/Combat/Stats/` |
| `TTCS.Combat.Actions` | `Scripts/Combat/Actions/` |
| `TTCS.Combat.AI` | `Scripts/Combat/AI/` |
| `TTCS.Combat.Managers` | `Scripts/Combat/Managers/` |
| `TTCS.Data` | `Scripts/Data/` |
| `TTCS.Debugging` | `Scripts/Debug/` |

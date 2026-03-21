# 02 — Dependency Map (Không Dùng Sơ Đồ)

Tài liệu này trình bày dependency theo dạng bảng và danh sách để dễ đọc, dễ trace impact khi chỉnh sửa class.

Quy ước:
- Outbound = class này gọi/phụ thuộc vào class khác.
- Inbound = class khác đang gọi/phụ thuộc vào class này.
- Mức độ: `High` (chỉnh dễ lan rộng), `Medium`, `Low`.

---

## 1. Dependency theo module (mức cao)

| Module | Outbound dependencies chính | Inbound dependencies chính |
|-------|------------------------------|----------------------------|
| Core Infrastructure | Hầu như độc lập (`EventBus`, `RNGService`) | Hầu hết module khác dùng |
| Data Layer | `EventBus`, cache/validator nội bộ | Combat managers, factory, save/load |
| Entity & Component | `EventBus`, status effects | Action resolver, combat flow |
| Combat Managers | Data, AI, Action pipeline, Core infra | Scene/bootstrap/entry points |
| Action Pipeline | Skill manager, stat calculator, event bus | Combat flow controller |
| AI System | Skill manager, target selector | Combat flow controller |

---

## 2. Class index: Outbound / Inbound / Coupling

| Class | Outbound (uses) | Inbound (used by) | Coupling |
|------|------------------|-------------------|----------|
| `EventBus` | — | Gần như toàn bộ hệ thống | High |
| `RNGService` | — | `CombatFlowController`, `StatCalculator`, `TargetSelector` (random rule) | Medium |
| `DataManager` | `DataCache<T>`, `DataValidator`, `EventBus` | `EntityFactory`, `SkillManager`, `CombatFlowController`, loader classes | High |
| `DataCache<T>` | — | `DataManager` | Low |
| `DataValidator` | — | `DataManager` | Low |
| `SaveManager` | `EventBus`, `SaveData` | Save/load UI flow, scene flow | Medium |
| `EntityFactory` | `DataManager`, `Character`, `Enemy` | `CombatFlowController`, scene loader | Medium |
| `CombatEntity` | `HealthComponent`, `StatsComponent`, `EffectComponent`, `EntityStats` | `ActionResolver`, `CombatFlowController`, AI snapshots | High |
| `Character` | kế thừa `CombatEntity` | `EntityFactory`, `CombatFlowController` | Medium |
| `Enemy` | kế thừa `CombatEntity`, `AIBehavior` | `EntityFactory`, `CombatFlowController`, `AIController` | Medium |
| `HealthComponent` | `EventBus` | `CombatEntity`, `ActionResolver`, effects | Medium |
| `StatsComponent` | `EntityStats`, `StatModifier` | `CombatEntity`, `StatCalculator` | Medium |
| `EffectComponent` | `StatusEffect`, `EventBus` | `CombatEntity`, `ActionResolver` | Medium |
| `StatusEffect` + derived effects | `CombatEntity`/components | `EffectComponent`, `ActionResolver` | Medium |
| `CombatFlowController` | `TurnManager`, `SkillManager`, `DataManager`, `AIController`, `SkillAction`, `EventBus`, `RNGService` | Scene/boot/loader entry points | High |
| `TurnManager` | `EventBus` | `CombatFlowController` | Medium |
| `SkillManager` | `DataManager`, `EventBus` | `CombatFlowController`, `SkillAction`, `ActionValidator`, `AIController` | High |
| `IAction` | contract only | `SkillAction` | Low |
| `SkillAction` | `ActionValidator`, `ActionResolver`, `SkillManager`, `EventBus` | `CombatFlowController` | High |
| `ActionValidator` | `SkillManager` | `SkillAction` | Medium |
| `ActionResolver` | `StatCalculator`, `EventBus`, effects/components | `SkillAction` | High |
| `StatCalculator` | `RNGService` | `ActionResolver` | Medium |
| `AIController` | `AIBehavior`, `TargetSelector`, `SkillManager` | `CombatFlowController` | High |
| `TargetSelector` | `RNGService` (rule random), entity list | `AIController` | Medium |
| `AIBehavior` | data asset (ScriptableObject) | `Enemy`, `AIController` | Low |

---

## 3. Dependency theo nhóm chi tiết

### 3.1 Data Layer

| Class | Outbound | Inbound |
|------|----------|---------|
| `DataManager` | `DataCache<CharacterDataModel>`, `DataCache<SkillDataModel>`, `DataCache<EnemyDataModel>`, `DataCache<StageDataModel>`, `DataValidator`, `EventBus` | `EntityFactory`, `SkillManager`, `CombatFlowController` |
| `DataCache<T>` | — | `DataManager` |
| `DataValidator` | — | `DataManager` |
| `SaveManager` | `SaveData`, `EventBus` | Save/load flows |

Ghi chú:
- Điểm hub của data là `DataManager`.
- Đổi schema model có thể tác động dây chuyền tới factory, skill checks, và combat setup.

### 3.2 Entity + Component

| Class | Outbound | Inbound |
|------|----------|---------|
| `CombatEntity` | `HealthComponent`, `StatsComponent`, `EffectComponent`, `EntityStats` | `ActionResolver`, `CombatFlowController`, AI |
| `Character` | base `CombatEntity` | `EntityFactory`, combat team setup |
| `Enemy` | base `CombatEntity`, `AIBehavior` | `EntityFactory`, `AIController` |
| `HealthComponent` | `EventBus` (`DamageTakenEvent`, `HealingReceivedEvent`, `EntityDeathEvent`) | `CombatEntity`, resolver/effects |
| `StatsComponent` | `EntityStats`, modifiers | `CombatEntity`, `StatCalculator` |
| `EffectComponent` | `StatusEffect`, `EventBus` (`StatusEffectAppliedEvent`, `StatusEffectRemovedEvent`) | `CombatEntity`, resolver |
| `EntityFactory` | `DataManager`, concrete entities | `CombatFlowController`, loader |

### 3.3 Combat Managers

| Class | Outbound | Inbound |
|------|----------|---------|
| `CombatFlowController` | `TurnManager`, `SkillManager`, `DataManager`, `AIController`, `SkillAction`, `EventBus`, `RNGService` | Entry points từ scene/test loader |
| `TurnManager` | `EventBus` (`TurnStartedEvent`, `TurnEndedEvent`, `TimelineUpdatedEvent`) | `CombatFlowController` |
| `SkillManager` | `DataManager`, `EventBus` (`ManaChangedEvent`) | `CombatFlowController`, `SkillAction`, `ActionValidator`, `AIController` |

### 3.4 Action Pipeline

| Class | Outbound | Inbound |
|------|----------|---------|
| `IAction` | — | `SkillAction` implement |
| `SkillAction` | `ActionValidator`, `ActionResolver`, `SkillManager`, `EventBus` (`SkillCastEvent`, `ActionExecutedEvent`) | `CombatFlowController` |
| `ActionValidator` | `SkillManager` | `SkillAction` |
| `ActionResolver` | `StatCalculator`, entity components, `EventBus` | `SkillAction` |
| `StatCalculator` | `RNGService` | `ActionResolver` |

### 3.5 AI System

| Class | Outbound | Inbound |
|------|----------|---------|
| `AIController` | `AIBehavior`, `TargetSelector`, `SkillManager` | `CombatFlowController` |
| `TargetSelector` | entity list, `RNGService` (rule random) | `AIController` |
| `AIBehavior` | data fields/skill priorities | `Enemy`, `AIController` |

---

## 4. Event dependency (publisher/subscriber view)

| Event | Publisher chính | Subscriber chính |
|------|------------------|------------------|
| `CombatStartedEvent` | `CombatFlowController` | `RNGService`, log/bridge |
| `CombatEndedEvent` | `CombatFlowController` | bridge/result/save flow |
| `TurnStartedEvent` | `TurnManager` | effect tick, UI, cooldown tick |
| `TurnEndedEvent` | `TurnManager` | UI, timeline |
| `TimelineUpdatedEvent` | `TurnManager` | timeline UI |
| `SkillCastEvent` | `SkillAction` | VFX/Audio bridge |
| `ActionExecutedEvent` | `SkillAction` | logger/analytics |
| `DamageTakenEvent` | `HealthComponent` | HP UI, logger |
| `HealingReceivedEvent` | `HealthComponent` | HP UI |
| `ManaChangedEvent` | `SkillManager` | Mana UI |
| `EntityDeathEvent` | `HealthComponent` | `CombatFlowController` cleanup |
| `StatusEffectAppliedEvent` | `EffectComponent` / resolver | Status UI |
| `StatusEffectRemovedEvent` | `EffectComponent` | Status UI |
| `TimingInputEvent` | timing/input layer | `CombatFlowController` |
| `DataLoadedEvent` | `DataManager` | loader/init systems |
| `GameSavedEvent` | `SaveManager` | save UI |
| `GameLoadedEvent` | `SaveManager` | scene/data refresh |

---

## 5. Change impact map (khi sửa class nào sẽ ảnh hưởng đâu)

| Nếu sửa | Cần kiểm tra lại |
|--------|-------------------|
| `DataManager` API / model loading | `EntityFactory`, `SkillManager`, combat start sequence |
| `CombatEntity` contract (`IsDead`, stats, component access) | `ActionResolver`, AI decision, turn/cleanup flow |
| `TurnManager` logic gauge/threshold | Toàn bộ thứ tự lượt, timeline preview, combat pacing |
| `SkillManager` rules cooldown/mana | validator, AI chọn skill, UI mana/cooldown |
| `ActionResolver` damage/heal/effect | balance, death flow, event stream, VFX triggers |
| `StatCalculator` formulas | Tất cả output combat numbers |
| `AIController` decision tree | nhịp trận enemy, độ khó |
| `EventBus` behavior | toàn bộ event-driven integration |

---

## 6. Phụ thuộc quan trọng cần giữ ổn định

1. `CombatFlowController -> TurnManager` là xương sống của vòng lặp combat.
2. `SkillAction -> SkillManager -> ActionResolver` là pipeline thực thi action.
3. `ActionResolver -> HealthComponent/EffectComponent` là nơi tạo side effects gameplay.
4. `DataManager -> EntityFactory` quyết định object runtime đúng với data config.
5. `EventBus` là kênh tích hợp chéo cho UI/Logger/Bridge, thay đổi ở đây có blast radius lớn.

---

## 7. Checklist review nhanh dependency

- Có class mới tham gia combat chưa được khai báo inbound/outbound trong file này?
- Có dependency vòng lặp không mong muốn (manager gọi ngược lại flow controller)?
- Có class domain đang phụ thuộc ngược lên UI layer không?
- Có event mới phát sinh nhưng chưa có subscriber rõ ràng?
- Có API data thay đổi mà factory/validator chưa cập nhật?

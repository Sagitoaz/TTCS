# 08 - Danh Mục Toàn Bộ Class/Interface/Enum Trong Dự Án

Mục tiêu:
- Liệt kê đầy đủ thành phần mã nguồn để không sót class.
- Cho biết vai trò ngắn gọn và vị trí file.

## A. Nhóm Combat/Managers

- `CombatSceneManager` - khởi tạo scene combat, tạo entity, dựng view, khởi chạy battle.
- `CombatFlowController` - state machine và vòng lặp trận đấu.
- `TurnManager` - điều phối lượt theo CTB gauge.
- `SkillManager` - quản lý mana/cooldown/giới hạn dùng skill.
- `CombatBridge` - cầu nối event combat sang visual layer.
- `CombatBridge.ICharacterAnimatorBridge` - interface chuẩn cho view animation bridge.
- `ActionAnimationController` - điều phối animation dựa trên event.
- `CombatTestLoader` - helper chạy test combat nhanh trong scene.

## B. Nhóm Combat/Actions

- `IAction` - contract action.
- `ActionValidationResult` - kết quả validate action.
- `ActionValidator` - kiểm tra tính hợp lệ trước execute.
- `ActionResolver` - áp dụng damage/heal/effects.
- `SkillAction` - action cụ thể cho skill cast.

## C. Nhóm Combat/Entities

- `CombatEntity` - abstract base entity.
- `Character` - entity phe người chơi.
- `Enemy` - entity phe AI.
- `EntityFactory` - tạo entity từ dữ liệu.

## D. Nhóm Combat/Components

- `IEntityComponent` - interface cho component thực thể.
- `HealthComponent` - HP/shield/damage/heal.
- `StatsComponent` - stat gốc + modifiers.
- `EffectComponent` - chứa và tick status effects.

## E. Nhóm Combat/Effects

- `TickTiming` - enum thời điểm tick effect.
- `StatusEffect` - abstract base cho effect.
- `BleedEffect`, `BurnEffect`, `HealEffect`, `ShieldEffect`, `StunEffect` - effect cụ thể.

## F. Nhóm Combat/Timing

- `TimingWindow` - dữ liệu cửa sổ timing + evaluate grade.
- `TimingSystem` - quản lý vòng đời timing window.
- `TimingInputHandler` - nhận input và đẩy vào TimingSystem.

## G. Nhóm Combat/AI

- `AIDecision` - struct lưu quyết định AI.
- `AIController` - decision tree chọn skill/target.
- `AIBehavior` - profile ScriptableObject của AI.
- `CombatEntitySnapshot` - snapshot để AI xử lý.
- `TargetSelector` - chọn target theo rule.

## H. Nhóm Combat/Stats

- `EntityStats` - dữ liệu chỉ số thực thể.
- `StatCalculator` - công thức damage/heal/defense.
- `StatType`, `ModifierType`, `StatModifier` - hệ modifier stat.
- `DamageType`, `Element`, `ActionSpeed` - enum combat liên quan damage/hành động.

## I. Nhóm UI/Combat

- `CombatUIController` - root controller của toàn UI combat.
- `BattleHUD` - hiển thị HP/MP và trạng thái slot.
- `BattleHUD.HUDSlot` - lớp nội bộ cho từng ô HUD.
- `SkillButtonPanel` - panel kỹ năng theo lượt player.
- `SkillButton` - nút kỹ năng đơn lẻ.
- `TurnOrderDisplay` - hiển thị thứ tự lượt.
- `TurnOrderSlot` - item hiển thị trong turn order.
- `ActionResultDisplay` - hiển thị số damage/heal.
- `FloatingText` - đối tượng text bay.
- `TimingWindowUI` - UI cửa sổ parry.
- `TimingFeedbackUI` - UI phản hồi Perfect/Good/Miss.
- `TimingFeedbackUI.TimingGrade` - enum grade timing (UI side).

## J. Nhóm Visual

- `CharacterView` - container hiển thị nhân vật.
- `EnemyView` - mở rộng CharacterView cho enemy.
- `CharacterAnimator` - phát animation tấn công/đau/chết/thắng.
- `CharacterViewFactory` - tạo view từ prefab/data.
- `TelegraphVisual` - hiển thị telegraph trước đòn đánh.
- `VFXController` - quản lý và phát VFX bằng object pool.

## K. Nhóm Audio

- `AudioController` - phát SFX/BGM và nghe event combat.

## L. Nhóm Core/Data

- `DataManager` - load/cache dữ liệu JSON.
- `DataCache<T>` - cache generic.
- `DataValidator` - validate data models.

## M. Nhóm Core/Events

- `GameEvent` - lớp gốc event.
- `EventBus` - hệ pub/sub.

### M1. Combat events (`Core/Events/CombatEvents.cs`)
- `CombatStartedEvent`, `CombatEndedEvent`
- `TurnStartedEvent`, `TurnEndedEvent`, `TimelineUpdatedEvent`
- `ActionExecutedEvent`, `SkillCastEvent`
- `DamageTakenEvent`, `HealingReceivedEvent`, `ManaChangedEvent`, `EntityDeathEvent`
- `StatusEffectAppliedEvent`, `StatusEffectRemovedEvent`
- `TimingInputEvent`, `TimingInputEvent.TimingGrade`

### M2. System events (`Core/Events/SystemEvents.cs`)
- `DataLoadedEvent`
- `GameSavedEvent`
- `GameLoadedEvent`

## N. Nhóm Core/Save

- `SaveData` - mô hình dữ liệu lưu game.
- `SaveSlot` - metadata cho từng slot.
- `SaveManager` - save/load/delete dữ liệu.

## O. Nhóm Core/Utilities và Constants

- `Constants` - hằng số hệ thống.
- `RNGService` - random service có seed.
- `GameUtils` - utility helpers.

## P. Nhóm Data (ScriptableObject + DataModel)

### P1. ScriptableObject wrappers
- `CharacterData`
- `EnemyData`
- `SkillData`
- `StageData`
- `WaveConfig`

### P2. Character models
- `CharacterDataModel`, `CharacterMetadata`, `CharacterBaseStats`, `CharacterGrowthCurve`, `CharacterPassive`, `CharacterVisual`, `CharacterAIHints`

### P3. Enemy models
- `EnemyDataModel`, `EnemyBaseStats`, `EnemyResistances`, `EnemyMove`, `EnemyMoveCondition`, `EnemyPhase`, `EnemyTelegraph`, `EnemyVisual`, `EnemyRewards`, `EnemyDropItem`

### P4. Skill models
- `SkillDataModel`, `SkillTargetRule`, `SkillCost`, `SkillDamage`, `SkillEffect`, `SkillTiming`, `SkillTimingWindow`, `SkillActionCost`, `SkillVisual`

### P5. Stage models
- `StageDataModel`, `StageRequirements`, `StageEncounter`, `StageEnemy`, `StageEnvironment`, `StageRewards`, `StageRewardFirstClear`, `StageRewardRepeat`, `StageRewardItem`

## Q. Nhóm Debug

- `DebugLogger`, `DebugLogger.LogCategory`
- `CombatLogger`

## R. Nhóm Utils

- `Singleton<T>`

## S. Nhóm Debug/Test (test harness)

- `ActionPipelineTest`
- `AIControllerTest`
- `AudioControllerTest`
- `BattleHUDTest`
- `CombatBridgeTest`
- `CombatBridgeTest.MockAnimatorBridge`
- `CombatSceneManagerTest`
- `CombatUIControllerTest`
- `ComponentTest`
- `DamageCalcTest`
- `DataManagerTest`
- `EntityFactoryTest`
- `FloatingTextPoolTest`
- `SaveManagerTest`
- `SkillButtonPanelTest`
- `SkillManagerTest`
- `TimingSystemTest`
- `TimingWindowTest`
- `TurnManagerTest`
- `TurnOrderDisplayTest`

## T. Ghi chú sử dụng file này

- Nếu bạn chỉ cần runtime classes để đọc nghiệp vụ: dùng mục A đến R.
- Nếu bạn cần cả lớp test/debug để vấn đáp kỹ thuật: đọc thêm mục S.
- Để biết class tham gia luồng nào, mở file `07_Flow_Class_Method_Map.md`.

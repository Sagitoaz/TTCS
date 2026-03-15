# 07 - Bản Đồ Luồng Class -> Method (Đầy Đủ)

Tài liệu này là bản trace chi tiết theo thứ tự gọi hàm.
Ký hiệu: `Class.Method -> Class.Method`.

## I. Luồng vòng đời scene và manager

### 1) Khởi tạo singleton và subscribe
- `DataManager.Awake -> DataManager.LoadAllData`
- `SaveManager.Awake`
- `EventBus.Instance` (lazy create nếu chưa có)
- `AudioController.Awake -> DontDestroyOnLoad`
- `TurnManager.Awake -> DontDestroyOnLoad`
- `SkillManager.Awake -> DontDestroyOnLoad`
- `CombatFlowController.Awake -> DontDestroyOnLoad`
- `CombatBridge.OnEnable -> SubscribeEvents`
- `ActionAnimationController.OnEnable -> EventBus.Subscribe(...)`
- `VFXController.Start -> SubscribeEvents`
- `CombatUIController.Initialize -> EventBus.Subscribe(...)`

### 2) Hủy đăng ký khi disable/destroy
- `CombatBridge.OnDisable/OnDestroy -> UnsubscribeEvents`
- `ActionAnimationController.OnDisable -> EventBus.Unsubscribe(...)`
- `VFXController.OnDisable -> UnsubscribeEvents`
- `AudioController.OnDisable/OnDestroy -> UnsubscribeEvents`
- `BattleHUD.OnDestroy -> UnsubscribeEvents`
- `TurnOrderDisplay.OnDisable -> Unsubscribe`
- `SkillButtonPanel.OnDisable -> Unsubscribe`

## II. Luồng setup combat scene

1. `CombatSceneManager.Start` (nếu auto start)
2. `CombatSceneManager.InitializeCombat(stageId, partyIds, seed)`
3. `DataManager.LoadStage(stageId)`
4. `CombatSceneManager.GetFirstWaveEnemyIds(stage)`
5. `EntityFactory.CreateParty(partyIds)`
6. `EntityFactory.CreateWave(enemyIds)`
7. Gán AI behavior từ `EnemyData` asset vào `Enemy.Behavior`
8. `SkillManager.ResetCombat`
9. `SkillManager.RegisterEntity` cho toàn bộ entities
10. `CombatUIController.Initialize(allies, enemies)`
11. `CombatSceneManager.SpawnAndRegisterViews`
12. `ActionAnimationController.RegisterCharacterView/RegisterEnemyView`
13. `CombatBridge.RegisterView`
14. `CombatUIController.RegisterEntityPosition`
15. `CombatFlowController.StartBattle(players, enemies, seed)`

## III. Luồng battle loop

1. `CombatFlowController.CombatLoop`
2. `CombatFlowController.InitBattle`
3. `RNGService.Initialize(seed)`
4. `TurnManager.ResetCombat + RegisterEntity`
5. `SkillManager.ResetCombat + RegisterEntity`
6. `EventBus.Publish(CombatStartedEvent)`
7. Vòng lặp mỗi lượt:
   - `TurnManager.GetNextActor`
   - `TurnManager.StartTurn`
   - `CombatEntity.OnTurnStart`
   - Nhánh player hoặc enemy
   - `CombatEntity.OnTurnEnd`
   - `SkillManager.TickCooldowns`
   - `TurnManager.EndTurn`
   - `CombatFlowController.CleanupDeadEntities`
8. Kết thúc:
   - `CombatFlowController.EndBattle`
   - `EventBus.Publish(CombatEndedEvent)`

## IV. Luồng turn người chơi

1. `CombatFlowController.PlayerTurnRoutine`
2. Chờ `_playerInputReceived`
3. Input đến từ:
   - `SkillButton.OnClick`
   - `SkillButtonPanel.OnSkillSelected`
   - `CombatFlowController.SubmitPlayerAction`
4. `CombatFlowController.ExecuteAction(actor, skillId, targetIds, TimingGrade.Miss)`
5. `DataManager.LoadSkill`
6. `CombatFlowController.ResolveTargets`
7. `SkillAction.Validate`
8. `ActionValidator.ValidateSkillAction`
9. `SkillAction.Execute`
10. `SkillManager.UseSkill`
11. `ActionResolver.Resolve`

## V. Luồng turn enemy AI

1. `CombatFlowController.EnemyTurnRoutine`
2. `CombatFlowController.BuildSnapshots`
3. `AIController.DecideAction`
4. `AIController.TryUseSkill`
5. `TargetSelector.SelectTargets`
6. Nếu attack:
   - `TimingSystem.OpenWindow`
   - `TimingInputHandler.OnGuardPerformed / Update fallback`
   - `TimingSystem.RegisterInput`
   - `TimingSystem.WindowLifecycle`
   - `TimingSystem.FinishWindow`
7. `CombatFlowController.ExecuteAction(enemy, skillId, targetIds, guardGrade)`

## VI. Luồng action pipeline

1. `SkillAction.Validate`
2. `ActionValidator.ValidateSkillAction`
3. `SkillAction.Execute`
4. `SkillManager.UseSkill`
5. `EventBus.Publish(SkillCastEvent)`
6. `EventBus.Publish(ManaChangedEvent)`
7. `ActionResolver.Resolve`
8. `EventBus.Publish(ActionExecutedEvent)`

## VII. Luồng damage/heal/effects (chi tiết)

### Attack
- `ActionResolver.ResolveAttack`
- `StatCalculator.CalculateDamage`
- Nếu target player: áp multiplier từ `TimingGrade`
- `CombatEntity.TakeDamage`
- `HealthComponent.TakeDamage`
- `EventBus.Publish(DamageTakenEvent)`
- Nếu chết: `EventBus.Publish(EntityDeathEvent)`

### Heal
- `ActionResolver.ResolveHeal`
- `StatCalculator.CalculateHeal`
- `CombatEntity.Heal`
- `HealthComponent.Heal`
- `EventBus.Publish(HealingReceivedEvent)`

### Effect
- `ActionResolver.TryApplyEffect`
- `RNGService.RollChance`
- `ActionResolver.BuildStatusEffect`
- `CombatEntity.ApplyEffect`
- `EffectComponent.AddEffect`
- `EventBus.Publish(StatusEffectAppliedEvent)`

## VIII. Luồng tick status theo lượt

- Đầu lượt:
  - `CombatEntity.OnTurnStart`
  - `EffectComponent.TickEffects(owner, TickTiming.StartTurn)`
  - `StatsComponent.TickModifiers`
- Cuối lượt:
  - `CombatEntity.OnTurnEnd`
  - `EffectComponent.TickEffects(owner, TickTiming.EndTurn)`
- Hết hạn:
  - `EffectComponent.RemoveEffect`
  - `EventBus.Publish(StatusEffectRemovedEvent)`

## IX. Luồng UI theo event

### 1) HUD
- `BattleHUD.InitializeSlots`
- `BattleHUD.OnDamageTaken`
- `BattleHUD.OnHealingReceived`
- `BattleHUD.OnManaChanged`
- `BattleHUD.OnEntityDeath`

### 2) Skill panel
- `CombatUIController.OnPlayerTurnStarted`
- `SkillButtonPanel.Initialize`
- `SkillButtonPanel.ShowForTurn`
- `SkillButtonPanel.OnTurnEnded` -> hide

### 3) Turn order
- `TurnOrderDisplay.OnTimelineUpdated`
- `TurnOrderDisplay.Refresh`
- `TurnManager.GetTimelinePreview`

### 4) Floating text
- `ActionResultDisplay.OnDamageTaken/OnHealingReceived`
- `ActionResultDisplay.SpawnText`
- `FloatingText.Show`

### 5) Timing UI
- `TimingWindowUI.HandleWindowOpened`
- `TimingWindowUI.HandleWindowClosed`
- `CombatUIController.ShowTimingResult`
- `TimingFeedbackUI.ShowResult`

## X. Luồng animation, bridge, VFX, audio

### 1) Animation controller
- `ActionAnimationController.OnSkillCast -> PlayAttackSequence`
- `ActionAnimationController.OnDamageTaken -> PlayHurt` (trường hợp nguồn effect)
- `ActionAnimationController.OnEntityDeath -> PlayDeath`
- `ActionAnimationController.OnTurnStart -> SetHighlight`
- `ActionAnimationController.OnCombatEnded -> PlayVictory`

### 2) CombatBridge
- `CombatBridge.RegisterView`
- `CombatBridge.OnSkillCast -> view.PlayAttack`
- `CombatBridge.OnDamageTaken -> view.PlayHurt`
- `CombatBridge.OnEntityDeath -> view.PlayDeath`
- `CombatBridge.OnCombatEnded -> view.PlayVictory`
- `CombatBridge.NotifyTelegraphComplete -> TimingSystem.OpenWindow`

### 3) VFX
- `VFXController.OnDamageTaken -> PlayHitVFX`
- `VFXController.OnHealingReceived -> PlayHitVFX("Heal")`
- `VFXController.OnEntityDeath -> PlayDeathVFX`
- `VFXController.PlayFromPool -> ReturnToPool`

### 4) Audio
- `AudioController.OnDamageTaken -> PlaySFX(hit/crit)`
- `AudioController.OnHealingReceived -> PlaySFX(heal)`
- `AudioController.OnEntityDeath -> PlaySFX(death)`
- `AudioController.OnSkillCast -> PlaySFX(skillCast)`
- `AudioController.OnCombatStarted -> PlayBGM(combat)`
- `AudioController.OnCombatEnded -> StopBGM + PlayBGM(victory/defeat)`

## XI. Luồng save/load

### Save
- `SaveManager.Save(slot)`
- Serialize `CurrentSave`
- Ghi file JSON
- `EventBus.Publish(GameSavedEvent)`

### Load
- `SaveManager.Load(slot)`
- Đọc file JSON
- Deserialize
- Cập nhật `CurrentSave` + `ActiveSlotIndex`
- `EventBus.Publish(GameLoadedEvent)`

## XII. Luồng debug/test

- Test scripts trong `Debug/Test` gọi trực tiếp API để kiểm tra:
  - `AIControllerTest`, `ActionPipelineTest`, `TurnManagerTest`, `TimingSystemTest`, ...
- `DebugLogger.Log` theo category.
- `CombatLogger` ghi log combat có cấu trúc.

## XIII. Luồng xử lý lỗi thường gặp (trace nhanh)

- Skill không bấm được:
  - `SkillButtonPanel.OnSkillSelected -> SkillManager.CanUseSkill`
- Không tìm thấy target:
  - `ResolveTargets` hoặc `TargetSelector.SelectTargets`
- Timing có log nhưng UI không hiện:
  - `TimingSystem.OpenWindow -> TimingWindowUI.TrySubscribe`
- Enemy chết nhưng vẫn đến lượt:
  - `CleanupDeadEntities -> TurnManager.RemoveEntity`

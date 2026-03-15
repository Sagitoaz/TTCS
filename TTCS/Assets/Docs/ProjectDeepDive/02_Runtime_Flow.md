# 02 - Luồng Runtime Toàn Dự Án

File này mô tả đầy đủ các luồng đang có trong dự án, gồm luồng chính và luồng phụ.

## 1. Luồng khởi tạo trận đấu

Chuỗi gọi hàm:
1. `CombatSceneManager.Start()`
2. `CombatSceneManager.InitializeCombat(...)`
3. `DataManager.LoadStage(...)`
4. `EntityFactory.CreateParty(...)`
5. `EntityFactory.CreateWave(...)`
6. `CombatUIController.Initialize(...)`
7. `CombatSceneManager.SpawnAndRegisterViews()`
8. `CombatFlowController.StartBattle(...)`

Kết quả:
- Scene có player/enemy runtime entities.
- UI đã biết danh sách entity.
- Battle loop bắt đầu chạy.

## 2. Luồng khởi tạo nội bộ battle loop

Chuỗi gọi hàm:
1. `CombatFlowController.StartBattle(...)`
2. `CombatFlowController.CombatLoop()`
3. `CombatFlowController.InitBattle()`
4. `RNGService.Initialize(seed)`
5. `TurnManager.ResetCombat()` + `RegisterEntity(...)`
6. `SkillManager.ResetCombat()` + `RegisterEntity(...)`
7. `EventBus.Publish(CombatStartedEvent)`

## 3. Luồng chọn actor đến lượt (CTB)

Chuỗi gọi hàm:
1. `TurnManager.GetNextActor()`
2. `CombatFlowController.FindEntity(actorId)`
3. `TurnManager.StartTurn(actorId)`
4. `CombatEntity.OnTurnStart()`
5. Rẽ nhánh `PlayerTurnRoutine` hoặc `EnemyTurnRoutine`

Đặc điểm:
- Đây là turn dựa trên gauge/tốc độ (CTB), không phải luân phiên cứng.

## 4. Luồng người chơi chọn kỹ năng

Chuỗi gọi hàm:
1. `SkillButton.OnClick()`
2. `SkillButtonPanel.OnSkillSelected(skillId)`
3. `SkillButtonPanel.ResolveTargets(skillModel)`
4. `CombatFlowController.SubmitPlayerAction(skillId, targetIds)`
5. `CombatFlowController.ExecuteAction(...)`
6. `SkillAction.Validate(...)`
7. `SkillAction.Execute(...)`
8. `SkillManager.UseSkill(...)`
9. `ActionResolver.Resolve(...)`

## 5. Luồng enemy AI

Chuỗi gọi hàm:
1. `CombatFlowController.EnemyTurnRoutine(enemy)`
2. `CombatFlowController.BuildSnapshots()`
3. `AIController.DecideAction(...)`
4. `TargetSelector.SelectTargets(...)`
5. Nếu có action hợp lệ -> `ExecuteAction(...)`

## 6. Luồng timing/parry

Chuỗi gọi hàm:
1. `EnemyTurnRoutine` xác định skill dạng tấn công.
2. `TimingSystem.OpenWindow(window)`
3. `TimingInputHandler.OnGuardPerformed(...)` hoặc fallback key -> `TimingSystem.RegisterInput(...)`
4. `TimingSystem.WindowLifecycle()`
5. `TimingSystem.FinishWindow(grade)`
6. `CombatFlowController` nhận grade và truyền vào `ExecuteAction(..., guardGrade)`
7. `CombatUIController.ShowTimingResult(grade)` -> `TimingFeedbackUI.ShowResult(grade)`

## 7. Luồng resolve damage/heal/effect

Chuỗi gọi hàm:
1. `ActionResolver.Resolve(...)`
2. Attack:
   - `ActionResolver.ResolveAttack(...)`
   - `StatCalculator.CalculateDamage(...)`
   - `HealthComponent.TakeDamage(...)`
3. Heal:
   - `ActionResolver.ResolveHeal(...)`
   - `StatCalculator.CalculateHeal(...)`
   - `HealthComponent.Heal(...)`
4. Effects:
   - `ActionResolver.TryApplyEffect(...)`
   - `EffectComponent.AddEffect(...)`

## 8. Luồng tick effect đầu/cuối lượt

Chuỗi gọi hàm:
1. Đầu lượt: `CombatEntity.OnTurnStart()` -> `EffectComponent.TickEffects(StartTurn)`
2. Cuối lượt: `CombatEntity.OnTurnEnd()` -> `EffectComponent.TickEffects(EndTurn)`
3. Hết hạn: `EffectComponent.RemoveEffect(...)` + publish event remove

## 9. Luồng event -> HUD

Chuỗi gọi hàm:
1. `DamageTakenEvent` -> `BattleHUD.OnDamageTaken(...)`
2. `HealingReceivedEvent` -> `BattleHUD.OnHealingReceived(...)`
3. `ManaChangedEvent` -> `BattleHUD.OnManaChanged(...)`
4. `EntityDeathEvent` -> `BattleHUD.OnEntityDeath(...)`

## 10. Luồng event -> FloatingText

Chuỗi gọi hàm:
1. `DamageTakenEvent` -> `ActionResultDisplay.OnDamageTaken(...)`
2. `HealingReceivedEvent` -> `ActionResultDisplay.OnHealingReceived(...)`
3. `ActionResultDisplay.SpawnText(...)` -> `FloatingText.Show(...)`

## 11. Luồng event -> Turn Order UI

Chuỗi gọi hàm:
1. `TimelineUpdatedEvent` -> `TurnOrderDisplay.OnTimelineUpdated(...)`
2. `TurnOrderDisplay.Refresh()`
3. `TurnManager.GetTimelinePreview(...)`
4. `TurnOrderSlot.SetData(...)`

## 12. Luồng event -> Animation bridge

Chuỗi gọi hàm:
1. `SkillCastEvent` -> `ActionAnimationController.OnSkillCast(...)`
2. `DamageTakenEvent` -> `ActionAnimationController.OnDamageTaken(...)`
3. `EntityDeathEvent` -> `ActionAnimationController.OnEntityDeath(...)`
4. `TurnStartedEvent` -> `ActionAnimationController.OnTurnStart(...)`
5. `CombatEndedEvent` -> `ActionAnimationController.OnCombatEnded(...)`

Luồng phối hợp qua CombatBridge:
- `CombatBridge` cũng subscribe event để gọi `PlayAttack/PlayHurt/PlayDeath/PlayVictory` trên view đã đăng ký.

## 13. Luồng event -> VFX

Chuỗi gọi hàm:
1. `DamageTakenEvent` -> `VFXController.OnDamageTaken(...)` -> `PlayHitVFX(...)`
2. `HealingReceivedEvent` -> `VFXController.OnHealingReceived(...)` -> `PlayHitVFX("Heal")`
3. `EntityDeathEvent` -> `VFXController.OnEntityDeath(...)` -> `PlayDeathVFX(...)`

## 14. Luồng event -> Audio

Chuỗi gọi hàm:
1. `DamageTakenEvent` -> `AudioController.OnDamageTaken(...)`
2. `HealingReceivedEvent` -> `AudioController.OnHealingReceived(...)`
3. `EntityDeathEvent` -> `AudioController.OnEntityDeath(...)`
4. `SkillCastEvent` -> `AudioController.OnSkillCast(...)`
5. `CombatStartedEvent` -> `AudioController.OnCombatStarted(...)` (BGM combat)
6. `CombatEndedEvent` -> `AudioController.OnCombatEnded(...)` (BGM kết quả)

## 15. Luồng kết thúc lượt và cleanup

Chuỗi gọi hàm:
1. `CombatEntity.OnTurnEnd()`
2. `SkillManager.TickCooldowns(actorId)`
3. `TurnManager.EndTurn(actorId, timelineCost)`
4. `CombatFlowController.CleanupDeadEntities()`
5. `TurnManager.RemoveEntity(deadId)`
6. `SkillManager.UnregisterEntity(deadId)`

## 16. Luồng kết thúc trận

Chuỗi gọi hàm:
1. `CombatFlowController.CheckVictory()` hoặc `CheckDefeat()`
2. `CombatFlowController.EndBattle(victory)`
3. `EventBus.Publish(CombatEndedEvent)`
4. `CombatUIController.OnCombatEnded(...)` hiện result panel
5. `CombatUIController.OnResultButtonClicked()` reload scene hiện tại

## 17. Luồng dữ liệu (load/cache)

Chuỗi gọi hàm:
1. `DataManager.Awake()` -> `LoadAllData()`
2. `LoadFolder<T>(...)` cho Characters/Skills/Enemies/Stages
3. `DataValidator.ValidateX(...)`
4. `DataCache<T>.Set(...)`
5. Runtime query: `LoadCharacter/LoadSkill/LoadEnemy/LoadStage`

## 18. Luồng save/load

### Save
1. `SaveManager.Save(slot)`
2. Serialize `CurrentSave`
3. Ghi file JSON
4. `EventBus.Publish(GameSavedEvent)`

### Load
1. `SaveManager.Load(slot)`
2. Đọc file JSON
3. Deserialize `SaveData`
4. Cập nhật `CurrentSave`
5. `EventBus.Publish(GameLoadedEvent)`

## 19. Luồng test/debug

- Test harness trong `Assets/Scripts/Debug/Test/` gọi trực tiếp API manager/component để kiểm tra logic.
- `DebugLogger` và `CombatLogger` ghi log theo category.
- Một số script test chạy trong `Start()` hoặc coroutine.

## 20. Bản đồ nhanh theo nhu cầu tra cứu

- Muốn biết vì sao actor này đến lượt trước: xem `TurnManager` + `CombatFlowController`.
- Muốn biết vì sao skill không cast được: xem `SkillManager` + `ActionValidator`.
- Muốn biết vì sao có damage/heal sai: xem `ActionResolver` + `StatCalculator` + `HealthComponent`.
- Muốn biết vì sao UI không update: xem `EventBus`, `BattleHUD`, `ActionResultDisplay`, `TurnOrderDisplay`.
- Muốn biết vì sao animation/vfx/audio không chạy: xem `ActionAnimationController`, `CombatBridge`, `VFXController`, `AudioController`.

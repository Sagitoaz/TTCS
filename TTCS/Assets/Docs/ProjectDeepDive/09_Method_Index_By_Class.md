# 09 - Method Index By Class (Tra Cứu Nhanh)

Tài liệu này liệt kê theo từng class:
- Hàm điểm vào (entry-point): nơi class khác/UI/event có thể gọi vào.
- Vòng đời Unity (nếu có): để biết lúc nào logic chạy.
- Mô tả 1 dòng cho từng hàm để tra nhanh.

## A. Combat/Managers

### CombatSceneManager
- `Start()` - tự động khởi tạo combat khi bật auto-start.
- `InitializeCombat(stageId, partyIds, seed)` - điểm vào chính để dựng toàn bộ trận đấu.
- `StartDefaultCombat()` - chạy combat bằng cấu hình mặc định từ Inspector.
- `OnCombatComplete(victory)` - hook xử lý sau khi combat kết thúc.
- `GetFirstWaveEnemyIds(stage)` - lấy danh sách enemy wave đầu từ stage data.

### CombatFlowController
- `StartBattle(players, enemies, seed)` - bắt đầu battle loop.
- `SubmitPlayerAction(skillId, targetIds)` - nhận action từ UI khi đến lượt player.
- `SkipPlayerTurn()` - bỏ lượt player theo fallback.
- `GetPlayerTeam()` - trả về team player hiện tại.
- `GetEnemyTeam()` - trả về team enemy hiện tại.
- `GetAllEntities()` - trả về toàn bộ entity đang còn trong battle list.
- `GetCurrentActor()` - trả về actor đang ở lượt hiện tại.
- `IsPlayerTurn()` - kiểm tra có đang ở pha player turn hay không.

### TurnManager
- `RegisterEntity(entityId, speed)` - đăng ký entity vào timeline CTB.
- `RemoveEntity(entityId)` - loại entity khỏi timeline.
- `GetNextActor()` - chọn actor đến lượt tiếp theo theo gauge/tốc độ.
- `StartTurn(entityId)` - đánh dấu bắt đầu lượt và publish event.
- `EndTurn(entityId, actionCost)` - kết thúc lượt, trừ gauge theo action cost.
- `GetTimelinePreview(previewCount)` - xem trước thứ tự lượt phục vụ UI.
- `GetGauge(entityId)` - lấy gauge hiện tại của entity.
- `SetGauge(entityId, gauge)` - chỉnh gauge (thường dùng mở lượt đầu).
- `ResetCombat()` - xóa toàn bộ state timeline.
- `InitializeCombat(list)` - tiện ích reset + register nhiều entity.

### SkillManager
- `RegisterEntity(entityId, maxMana, startingMana)` - đăng ký state mana/cooldown cho entity.
- `UnregisterEntity(entityId)` - gỡ entity khỏi skill state.
- `CanUseSkill(entityId, skillId)` - kiểm tra skill có dùng được không.
- `CanUseSkill(entityId, skillData)` - overload kiểm tra bằng model đã load.
- `UseSkill(entityId, skillId)` - commit dùng skill (mana/cd/usage).
- `UseSkill(entityId, skillData)` - overload commit bằng model.
- `TickCooldowns(entityId)` - giảm cooldown cuối lượt entity.
- `SetCooldown(entityId, skillId, turns)` - gán cooldown thủ công.
- `GetCooldown(entityId, skillId)` - lấy cooldown còn lại.
- `IsOnCooldown(entityId, skillId)` - kiểm tra skill đang hồi chiêu.
- `ResetAllCooldowns(entityId)` - xóa toàn bộ cooldown của entity.
- `GetMana(entityId)` - lấy mana hiện tại.
- `GetMaxMana(entityId)` - lấy mana tối đa.
- `RestoreMana(entityId, amount)` - hồi mana.
- `DrainMana(entityId, amount)` - trừ mana trực tiếp.
- `FullRestoreMana(entityId)` - hồi đầy mana.
- `GetUsageCount(entityId, skillId)` - lấy số lần dùng skill trong trận.
- `ResetCombat()` - reset toàn bộ state skill manager.

### CombatBridge
- `RegisterView(entityId, view)` - đăng ký view để bridge gọi animation.
- `UnregisterView(entityId)` - gỡ view khỏi bridge.
- `NotifyTelegraphComplete(enemyId, windowDuration)` - thông báo telegraph xong để mở timing.
- `GetView(entityId)` - lấy view bridge theo entity id.

### ActionAnimationController
- `RegisterCharacterView(entityId, view)` - đăng ký CharacterView.
- `RegisterEnemyView(entityId, view)` - đăng ký EnemyView.
- `GetViewForEntity(entityId)` - truy xuất view theo entity id.
- `ClearRegistry()` - xóa registry view khi reset combat.

### CombatTestLoader
- `StartTestCombat()` - kích hoạt luồng test combat nhanh.
- `DebugSubmitAction(skillId, targets)` - ép gửi action cho debug.
- `DebugSkipTurn()` - ép skip lượt cho debug.

## B. Combat/Actions

### SkillAction
- `Validate(actor, targets, skillManager)` - kiểm tra action hợp lệ trước execute.
- `Execute(actor, targets, skillManager, guard)` - thực thi action skill.
- `GetSkillData()` - lấy model skill gốc đang dùng.

### ActionValidator
- `ValidateSkillAction(actor, skill, targets, skillManager)` - validate đầy đủ actor/skill/resource/targets.
- `AreTargetsValid(targets, requireAlive)` - validate danh sách target độc lập.

### ActionResolver
- `Resolve(actor, targets, skill, guard)` - điểm vào chính để resolve toàn bộ kết quả skill.

## C. Combat/Entities

### CombatEntity
- `TakeDamage(amount, sourceId)` - nhận sát thương.
- `Heal(amount, sourceId)` - hồi máu.
- `ApplyEffect(effect)` - áp status effect.
- `RemoveEffect(effect)` - gỡ status effect.
- `OnTurnStart()` - hook đầu lượt (tick effect/modifier).
- `OnTurnEnd()` - hook cuối lượt (tick effect).
- `ResetForBattle()` - reset entity về trạng thái trận mới.
- `ToSnapshot()` - tạo snapshot cho AI.

### Character
- `HasSkill(skillId)` - kiểm tra character có sở hữu skill không.

### Enemy
- (chủ yếu dùng properties) `Behavior`, `SkillIds`, `RewardGold`, `RewardXP`.

### EntityFactory
- `CreateCharacter(model, index)` - tạo character từ model.
- `CreateCharacter(characterId, index)` - tạo character từ id.
- `CreateParty(characterIds)` - tạo cả party.
- `CreateEnemy(model, index)` - tạo enemy từ model.
- `CreateEnemy(enemyId, index)` - tạo enemy từ id.
- `CreateWave(enemyIds)` - tạo wave enemy.

## D. Combat/Components

### HealthComponent
- `Initialize(entityId)` - gắn component với entity id.
- `Reset()` - reset HP/shield.
- `SetMaxHP(max)` - đặt max HP.
- `TakeDamage(rawDamage, sourceId)` - nhận damage và publish events.
- `Heal(amount, sourceId)` - hồi HP và publish event.
- `AddShield(amount)` - cộng shield.
- `RemoveShield(amount)` - trừ shield.
- `ClearShield()` - xóa toàn bộ shield.

### StatsComponent
- `Initialize(entityId)` - gắn component với entity id.
- `Reset()` - xóa modifier runtime.
- `SetBaseStats(stats)` - gán stat gốc.
- `GetEffectiveStat(type)` - lấy stat hiệu dụng sau modifier.
- `AddModifier(mod)` - thêm modifier.
- `RemoveModifier(mod)` - gỡ modifier cụ thể.
- `RemoveModifiersBySource(sourceId)` - gỡ modifier theo nguồn.
- `ClearModifiers()` - xóa toàn bộ modifier.
- `TickModifiers()` - giảm duration modifier.
- `GetActiveModifiers()` - lấy danh sách modifier hiện hoạt.

### EffectComponent
- `Initialize(entityId)` - gắn component với entity id.
- `Reset()` - xóa effect list.
- `AddEffect(effect, owner)` - thêm hoặc stack effect.
- `RemoveEffect(effect, owner)` - remove effect.
- `TickEffects(owner, timing)` - tick effects theo thời điểm.
- `HasEffect(effectId)` - kiểm tra effect theo id.
- `HasEffect<T>()` - kiểm tra effect theo kiểu.
- `GetEffect<T>()` - lấy effect theo kiểu.
- `GetEffect(effectId)` - lấy effect theo id.
- `GetActiveEffects()` - lấy list effect active.
- `ClearEffects(owner)` - xóa toàn bộ effects.

## E. Combat/Effects

### StatusEffect
- `OnApply(owner)` - logic khi effect được áp vào mục tiêu.
- `OnTick(owner)` - logic tick theo lượt.
- `OnRemove(owner)` - logic khi effect bị gỡ.
- `OnStack(newInstance)` - xử lý khi effect cùng loại stack.

### BleedEffect / BurnEffect / HealEffect / ShieldEffect / StunEffect
- `OnApply/OnTick/OnRemove/OnStack` - override theo từng loại effect cụ thể.

## F. Combat/Timing

### TimingSystem
- `OpenWindow(window)` - mở cửa sổ timing mới.
- `RegisterInput(inputGameTime)` - nhận thời điểm input từ player.
- `ForceClose()` - đóng cưỡng bức cửa sổ timing.

### TimingInputHandler
- `OnGuardPerformed(ctx)` - nhận input guard từ Input System.
- `RegisterInput()` - chuyển input vào `TimingSystem`.

### TimingWindow
- `CreateDefault(openTime, duration)` - tạo window mặc định.
- `EvaluateInput(inputTime)` - chấm điểm Perfect/Good/Miss.

## G. Combat/AI

### AIController
- `DecideAction(self, allEntities, behavior, skillManager)` - quyết định action của enemy.
- `CreateSnapshot(id, hp, maxHp, isAlly)` - tạo snapshot tiện ích cho test.

### AIBehavior
- `IsValid()` - kiểm tra profile behavior có cấu hình đủ chưa.
- `GetPrioritizedSkills()` - lấy danh sách skill theo ưu tiên.

### TargetSelector
- `SelectTargets(casterId, targetRuleType, count, allEntities)` - chọn target theo target rule.
- `ValidateTargets(targetIds, allEntities)` - kiểm tra target list hợp lệ.

## H. Combat/Stats

### StatCalculator
- `CalculateDamage(attacker, defender, skillMultiplier, element, critMultiplier)` - tính damage cuối.
- `CalculateHeal(caster, skillMultiplier)` - tính heal.
- `CheckCrit(critRate)` - roll crit.
- `GetElementMultiplier(attackElement, defenseElement)` - hệ số hệ (hiện tại stub).
- `CalculateDefenseReduction(defense)` - tỷ lệ giảm damage theo DEF.
- `CalculateEffectiveHP(entity)` - tính EHP phục vụ đánh giá.

### EntityStats
- `SetCurrentHP(hp)` - set HP hiện tại.
- `ModifyHP(delta)` - tăng/giảm HP.
- `Heal(amount)` - hồi HP.
- `TakeDamage(amount)` - trừ HP.
- `FullHeal()` - hồi đầy.
- `Reset()` - reset trạng thái.
- `Clone()` - sao chép stats.

### StatModifier
- `TickDuration()` - giảm duration modifier theo lượt.

## I. UI/Combat

### CombatUIController
- `Initialize(playerTeam, enemyTeam)` - dựng toàn bộ context UI combat.
- `RegisterEntityPosition(entityId, transform)` - map entity tới vị trí world.
- `GetEntityWorldPos(entityId)` - lấy world pos cho VFX/FloatingText.
- `GetEntityHPPercent(entityId)` - lấy %HP hiện tại cho HUD.
- `ShowTimingResult(grade)` - hiển thị feedback timing.

### BattleHUD
- `InitializeSlots(allies, enemies)` - gắn entity vào slot HUD.

### SkillButtonPanel
- `Initialize(entityId, skillIds, enemies)` - setup panel theo actor hiện tại.
- `OnSkillSelected(skillId)` - nhận chọn skill từ button.
- `ShowForTurn()` - hiện panel khi đúng lượt.
- `Hide()` - ẩn panel.

### SkillButton
- `Setup(skillId, displayName, manaCost, ownerPanel)` - setup dữ liệu nút.
- `Hide()` - ẩn nút.
- `Refresh(canUse, cooldown, maxCooldown)` - cập nhật trạng thái click/cooldown.

### TurnOrderDisplay
- `RegisterEntity(entityId, displayName, isPlayer)` - đăng ký info hiển thị timeline.
- `Refresh()` - refresh danh sách preview lượt.

### TurnOrderSlot
- `SetData(entityId, displayName, isPlayer, isCurrent)` - set nội dung slot.
- `SetHighlight(active)` - bật/tắt highlight slot.
- `Clear()` - reset slot.

### ActionResultDisplay
- `OnDamageTaken(event)` - spawn text damage.
- `OnHealingReceived(event)` - spawn text heal.

### FloatingText
- `Show(text, color, worldPos, isCritical)` - hiển thị floating text.
- `Cancel()` - hủy animation text.

### TimingWindowUI
- `HandleWindowOpened(duration)` - hiện UI cửa sổ timing.
- `HandleWindowClosed()` - ẩn UI cửa sổ timing.

### TimingFeedbackUI
- `ShowResult(grade)` - phát animation feedback timing.

## J. Visual

### CharacterView
- `SetFacing(faceRight)` - đặt hướng nhìn sprite.
- `SetHighlight(active)` - highlight actor hiện tại.
- `SetAlpha(alpha)` - set alpha toàn bộ parts.
- `SetAllPartsColor(color)` - set màu toàn bộ parts.
- `ResetPartsColor()` - reset màu mặc định.
- `GetAllRenderers()` - lấy toàn bộ renderer parts.
- `RebuildRendererCache()` - rebuild cache renderer.
- `PlayAttack/PlayHurt/PlayDeath/PlayVictory()` - forward gọi animation.
- `GetWorldTransform()` - trả transform cho bridge.

### EnemyView
- `ShowTelegraph()` - bật telegraph.
- `HideTelegraph()` - tắt telegraph.

### CharacterAnimator
- `PlayAttack()` - phát animation tấn công.
- `NotifyAttackHitFrame()` - callback frame trúng đòn.
- `PlayHurt()` - phát animation trúng đòn.
- `PlayDeath()` - phát animation chết.
- `PlayVictory()` - phát animation thắng.
- `PlaySkillCast()` - phát animation cast skill.
- `StopAll()` - dừng toàn bộ sequence hiện tại.

### CharacterViewFactory
- `CreateCharacterView(model, parent)` - tạo view cho character.
- `CreateEnemyView(model, parent)` - tạo view cho enemy.

### TelegraphVisual
- `Play(duration)` - phát hiệu ứng telegraph.
- `Stop()` - dừng telegraph.

### VFXController
- `PlayHitVFX(worldPosition, damageType)` - phát hit VFX theo loại damage.
- `PlayDeathVFX(worldPosition)` - phát death VFX.
- `PlaySkillVFX(skillId, worldPosition, element)` - phát skill VFX.

## K. Audio

### AudioController
- `PlaySFX(clip)` - phát one-shot SFX.
- `PlayBGM(clip)` - phát BGM loop.
- `StopBGM()` - dừng BGM.
- `SetSFXVolume(volume)` - set volume SFX.
- `SetBGMVolume(volume)` - set volume BGM.
- `PlayTimingResult(grade)` - phát SFX theo kết quả timing.

## L. Core/Data

### DataManager
- `LoadAllData()` - load toàn bộ dữ liệu JSON.
- `LoadCharacter(id)` - lấy character model.
- `GetAllCharacters()` - lấy toàn bộ character models.
- `LoadSkill(id)` - lấy skill model.
- `GetAllSkills()` - lấy toàn bộ skill models.
- `LoadEnemy(id)` - lấy enemy model.
- `GetAllEnemies()` - lấy toàn bộ enemy models.
- `LoadStage(id)` - lấy stage model.
- `GetAllStages()` - lấy toàn bộ stage models.

### DataCache<T>
- `Set(id, data)` - lưu object vào cache.
- `Get(id)` - lấy object từ cache.
- `Has(id)` - kiểm tra key tồn tại.
- `Clear()` - xóa cache.
- `GetAll()` - lấy toàn bộ values.

### DataValidator
- `ValidateCharacter(model)` - validate character.
- `ValidateSkill(model)` - validate skill.
- `ValidateEnemy(model)` - validate enemy.
- `ValidateStage(model)` - validate stage.

## M. Core/Events

### EventBus
- `Subscribe<T>(handler)` - đăng ký nhận event.
- `Unsubscribe<T>(handler)` - hủy đăng ký event.
- `Publish<T>(eventData)` - phát event.
- `ClearAll()` - xóa toàn bộ subscribers.
- `Clear<T>()` - xóa subscriber theo loại event.
- `GetSubscriberCount<T>()` - đếm số subscriber theo loại.

## N. Core/Save

### SaveManager
- `NewGame()` - tạo save mặc định trong memory.
- `Save(slotIndex)` - lưu current save ra file.
- `Load(slotIndex)` - load save từ file.
- `DeleteSave(slotIndex)` - xóa save file slot.
- `HasSaveData(slotIndex)` - kiểm tra slot có save không.
- `GetSlotInfo(slotIndex)` - lấy metadata slot.
- `GetAllSlots()` - lấy toàn bộ slots.

### SaveData
- `GetCharacterLevel(characterId)` - lấy level character trong save.
- `SetCharacterLevel(characterId, level)` - cập nhật level character.
- `IsFirstClear(stageId)` - kiểm tra stage đã first clear chưa.
- `MarkFirstClear(stageId)` - đánh dấu first clear.

### SaveSlot
- `Empty(slotIndex)` - tạo slot rỗng.
- `FromSaveData(slotIndex, saveData)` - tạo slot từ save data.

## O. Core/Utilities

### RNGService
- `Initialize(seed)` - khởi tạo RNG theo seed.
- `InitializeWithTimestamp()` - khởi tạo RNG theo thời gian.
- `Range(min, max)` - random int.
- `Value()` - random float 0..1.
- `RollChance(chance)` - roll theo tỉ lệ 0..1.
- `RollChancePercent(percent)` - roll theo phần trăm.
- `RollCrit(critRate)` - roll chí mạng.
- `SaveState()` - lưu state RNG.
- `RestoreState(state)` - khôi phục state RNG.

### GameUtils
- `Clamp/Clamp01/Lerp/Percentage` - toán tử tiện ích.
- `FormatNumber/FormatTime` - format hiển thị.
- `ParseInt/ParseFloat` - parse an toàn.
- `GenerateId/GenerateShortId` - tạo id tiện ích.
- `IsNull(obj)` - kiểm tra null an toàn.

## P. Debug/Logger

### DebugLogger
- `Log/LogWarning/LogError` - ghi log theo category.
- `LogCombat/LogEvent/LogData/LogSave/LogUI/LogAI` - shortcut log theo ngữ cảnh.

### CombatLogger
- `LogAction/LogTurnStart/LogTurnEnd` - log lifecycle hành động/lượt.
- `LogDamage/LogHeal/LogSkillUse/LogSkillFailed` - log kết quả skill.
- `LogStatus/LogDeath/LogRevive` - log trạng thái entity.
- `LogCombatStart/LogCombatResult/LogWaveStart/LogTimeline` - log cấp trận.
- `LogError` - log lỗi combat.

## Q. Debug/Test (chỉ mục nhanh)

Các class test chính:
- `AIControllerTest`, `ActionPipelineTest`, `TurnManagerTest`, `SkillManagerTest`, `TimingSystemTest`.
- `DataManagerTest`, `SaveManagerTest`, `EntityFactoryTest`, `ComponentTest`, `DamageCalcTest`.
- `CombatSceneManagerTest`, `CombatUIControllerTest`, `BattleHUDTest`, `SkillButtonPanelTest`, `TurnOrderDisplayTest`, `FloatingTextPoolTest`, `AudioControllerTest`, `CombatBridgeTest`, `TimingWindowTest`.

Mục đích: script test/harness giúp verify nhanh từng module độc lập.

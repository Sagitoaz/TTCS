# 08 — Runtime Flows & Call Chains

Chi tiết các luồng chạy thực tế theo từng kịch bản trong Sprint 01.

---

## Flow 1: Khởi động ứng dụng & load data

```
[Scene Load → GameBootstrap.Awake()]
    │
    ├── Khởi tạo tất cả Singletons (do scene setup):
    │     EventBus, DataManager, SaveManager,
    │     CombatFlowController, TurnManager, SkillManager, RNGService
    │
    └── DataManager.LoadAllData()
            ├── LoadCharacters()  → JSON parse → DataCache._characters
            ├── LoadSkills()      → JSON parse → DataCache._skills
            ├── LoadEnemies()     → JSON parse → DataCache._enemies
            └── LoadStages()      → JSON parse → DataCache._stages
                    └── EventBus.Publish(DataLoadedEvent(4, 12, 8, 3))
                                ↓
                    CombatTestLoader.OnDataLoaded() → có thể AutoStart
```

---

## Flow 2: Bắt đầu trận chiến

```
[CombatTestLoader / CombatSceneManager].StartCombat(stageId, partyIds)
    │
    ├── DataManager.GetStage(stageId) → StageDataModel
    ├── foreach enemyId in stage.enemies:
    │     DataManager.GetEnemy(enemyId) → EnemyDataModel
    │     EntityFactory.CreateEnemy(model)
    │           ├── new Enemy(model.id, "enemy")
    │           ├── entity.Stats = new EntityStats(model.stats)
    │           ├── entity.HealthComponent = new HealthComponent(model.stats.hp)
    │           ├── entity.StatsComponent  = new StatsComponent(model.stats)
    │           ├── entity.EffectComponent = new EffectComponent()
    │           └── entity.AIBehavior = LoadAIBehavior(model.aiProfileId)
    │
    ├── foreach charId in partyIds:
    │     DataManager.GetCharacter(charId) → CharacterDataModel
    │     EntityFactory.CreateCharacter(model)
    │           └── (tương tự, IsPlayer=true)
    │
    └── CombatFlowController.StartBattle(playerTeam, enemyTeam, seed)
                └── StartCoroutine(CombatLoop())
```

---

## Flow 3: Khởi tạo combat trong CombatLoop

```
CombatLoop():
  └── InitBattle()
        ├── RNGService.Initialize(seed)
        ├── _allEntities = _playerTeam + _enemyTeam
        ├── TurnManager.ResetCombat()
        │     └── foreach entity in _allEntities:
        │           TurnManager.RegisterEntity(id, entity.Stats.SPD)
        │               _gauges[id]  = 0f   (hoặc pre-seeded nếu forceFirst)
        │               _speeds[id]  = SPD
        │               _activeEntities.Add(id)
        │
        ├── SkillManager.ResetCombat()
        │     └── foreach entity in _allEntities:
        │           SkillManager.RegisterEntity(id, DEFAULT_MAX_MANA)
        │               _maxMana[id]     = 100
        │               _currentMana[id] = 80  (80% start)
        │               _cooldowns[id]   = {}
        │               _usageCount[id]  = {}
        │
        └── EventBus.Publish(new CombatStartedEvent { Seed = seed })
                ↓
             (bên ngoài) RNGService.OnCombatStarted() — double-init (idempotent)
             (bên ngoài) CombatLogger.OnCombatStarted() — log
```

---

## Flow 4: CTB — Chọn actor tiếp theo

```
TurnManager.GetNextActor():
  1. Tính minTicksNeeded:
     foreach id in _activeEntities:
       ticks = (TURN_THRESHOLD - _gauges[id]) / _speeds[id]
     minTicks = min(tất cả ticks)

  2. Advance gauges:
     foreach id in _activeEntities:
       _gauges[id] += minTicks * _speeds[id]

  3. Tìm actor:
     candidates = [id where _gauges[id] >= TURN_THRESHOLD]
     nextActor  = candidates.OrderByDescending(gauge).First()
     return nextActor

  // Ví dụ với 2 entities:
  // EntityA: gauge=0, SPD=80 → ticks=(100-0)/80=1.25
  // EntityB: gauge=0, SPD=50 → ticks=(100-0)/50=2.00
  // minTicks = 1.25
  // After advance: A=100, B=62.5
  // A đến lượt → return A
```

---

## Flow 5: Lượt Player

```
CombatLoop → [actor.IsPlayer == true]
  └── StartCoroutine(PlayerTurnRoutine(actor))
        │
        ├── _state = CombatState.PlayerTurn
        ├── _playerInputReceived = false
        ├── EventBus.Publish(TurnStartedEvent(actorId, TurnNumber))
        │       ↓
        │   EffectComponent.OnTurnStarted() — tick DOT/HOT effects
        │   SkillManager.TickCooldowns(actorId) — giảm cooldown
        │   HUD.OnTurnStarted() — highlight player
        │
        ├── WaitUntil(() => _playerInputReceived)
        │   [UI gọi CombatFlowController.SubmitPlayerAction(skillId, targetIds)]
        │     └── _pendingSkillId  = skillId
        │         _pendingTargetIds = targetIds
        │         _playerInputReceived = true
        │
        └── ExecuteAction(actor, _pendingSkillId, _pendingTargetIds, grade)
              └── (xem Flow 7)
```

---

## Flow 6: Lượt AI Enemy

```
CombatLoop → [actor.IsPlayer == false]
  └── StartCoroutine(EnemyTurnRoutine(actor as Enemy))
        │
        ├── _state = CombatState.EnemyTurn
        ├── EventBus.Publish(TurnStartedEvent(actorId, TurnNumber))
        │       ↓ (subscribers tick effects/cooldowns)
        │
        ├── BuildEntitySnapshots(all entities)
        │     → List<EntitySnapshot> cho AIController
        │
        ├── decision = AIController.DecideAction(
        │         actor, _allEntities, actor.AIBehavior, SkillManager)
        │
        ├── [if skill.type == "attack"]
        │     timingWindow.Open(_guardWindowDuration)
        │     guardGrade = WaitForTimingInput()  // player có thể bấm guard
        │
        └── ExecuteAction(actor, decision.SkillId, decision.Targets.Ids, guardGrade)
              └── (xem Flow 7)
```

---

## Flow 7: Thực thi một Action (SkillAction pipeline)

```
CombatFlowController.ExecuteAction(actor, skillId, targetIds, grade):
  │
  ├── skillData = DataManager.GetSkill(skillId)
  ├── targets   = ResolveTargetEntities(skillData, targetIds)
  ├── action    = new SkillAction(skillData)
  │
  ├── result = action.Validate(actor, targets, SkillManager)
  │     └── ActionValidator.Validate(actor, targets, skillData, SkillManager)
  │           ├── actor != null && !actor.IsDead
  │           ├── targets non-empty
  │           ├── skillData != null
  │           └── SkillManager.CanUseSkill(actorId, skillData)
  │                   ├── !IsOnCooldown(actorId, skillId)
  │                   ├── currentMana >= skill.cost.mana
  │                   └── usageCount < skill.cost.limitPerFight (nếu có)
  │
  ├── [if !result.IsValid] → log, abort, end turn gracefully
  │
  └── action.Execute(actor, targets, SkillManager, grade)
        ├── SkillManager.UseSkill(actorId, skillData)
        │     ├── _currentMana[id] -= skill.cost.mana
        │     ├── _cooldowns[id][skillId] = skill.cost.cooldown
        │     ├── _usageCount[id][skillId]++
        │     └── EventBus.Publish(ManaChangedEvent(id, current, max))
        │
        ├── ActionResolver.Resolve(actor, targets, skillData, grade)
        │     └── (xem Flow 8, 9, hoặc 10)
        │
        ├── EventBus.Publish(SkillCastEvent(actorId, skillId, targetIds))
        └── EventBus.Publish(ActionExecutedEvent(actorId, "skill", targetIds))
```

---

## Flow 8: Resolve tấn công (Attack)

```
ActionResolver.ResolveAttack(actor, target, skillData, defenderGrade):
  │
  ├── attackMult = skillData.attackMultiplier (default 1.0)
  ├── timingMult = grade==Perfect?1.5 : grade==Good?1.2 : 1.0
  ├── finalMult  = attackMult * timingMult
  │
  ├── (damage, isCrit) = StatCalculator.CalculateDamage(
  │         actor, target, finalMult, skillData.element)
  │     ├── baseDmg   = actor.Stats.ATK * finalMult
  │     ├── defFactor = target.Stats.DEF / (target.Stats.DEF + 100)
  │     │                = clamp(result, 0, 0.75)
  │     ├── netDmg    = baseDmg * (1 - defFactor)
  │     ├── isCrit    = RNGService.RollCrit(actor.Stats.CritRate)
  │     ├── if isCrit: netDmg *= 1.5
  │     └── return (max(1, (int)netDmg), isCrit)
  │
  ├── guardMult  = defenderGrade==Perfect?0.2 : defenderGrade==Good?0.6 : 1.0
  ├── finalDmg   = (int)(damage * guardMult)
  │
  ├── target.HealthComponent.TakeDamage(finalDmg, actor.EntityId)
  │     ├── _currentHP = max(0, _currentHP - amount)
  │     ├── EventBus.Publish(DamageTakenEvent(entityId, sourceId, amount, isCrit))
  │     └── if _currentHP == 0:
  │           IsDead = true
  │           EventBus.Publish(EntityDeathEvent(entityId, target.IsPlayer))
  │
  └── if skillData.onHitEffects.Count > 0:
        ApplyEffects(actor, target, skillData.onHitEffects)
              └── (xem Flow 10)
```

---

## Flow 9: Resolve hồi máu (Heal)

```
ActionResolver.ResolveHeal(actor, target, skillData):
  │
  ├── healAmt = StatCalculator.CalculateHeal(actor, skillData.healMultiplier)
  │     = max(1, (int)(actor.Stats.ATK * skillData.healMultiplier))
  │
  └── target.HealthComponent.ReceiveHeal(healAmt)
        ├── _currentHP = min(MaxHP, _currentHP + amount)
        └── EventBus.Publish(HealingReceivedEvent(entityId, sourceId, amount))
```

---

## Flow 10: Áp dụng hiệu ứng (StatusEffect)

```
EffectComponent.ApplyEffect(effectData, sourceId):
  │
  ├── Tạo instance đúng loại:
  │     "bleed"  → new BleedEffect(data)
  │     "burn"   → new BurnEffect(data)
  │     "heal"   → new HealOverTimeEffect(data)
  │     "shield" → new ShieldEffect(data)
  │     "stun"   → new StunEffect(data)
  │
  ├── [Stack logic]
  │     existing = _activeEffects.Find(e => e.EffectId == id)
  │     if existing && effect.CanStack: _activeEffects.Add(newEffect)
  │     else if existing:               existing.Refresh(duration)
  │     else:                           _activeEffects.Add(newEffect)
  │
  └── EventBus.Publish(StatusEffectAppliedEvent(entityId, effectId, duration))

EffectComponent.OnTurnStarted(TurnStartedEvent e):
  // Gọi khi nhận TurnStartedEvent của ENTITY NÀY
  foreach effect in _activeEffects.ToList():
    effect.OnTurnStart(owner)
          // BleedEffect:     owner.HealthComponent.TakeDamage(dmg, sourceId)
          // BurnEffect:      owner.HealthComponent.TakeDamage(dmg, sourceId)
          // HealOverTime:    owner.HealthComponent.ReceiveHeal(amt)
          // ShieldEffect:    giảm dmg tiếp theo (intercept)
          // StunEffect:      owner.IsStunned = true
    effect.DecrementDuration()
    if effect.Duration <= 0:
      _activeEffects.Remove(effect)
      EventBus.Publish(StatusEffectRemovedEvent(entityId, effectId))
```

---

## Flow 11: Kết thúc lượt & cleanup

```
[Sau ExecuteAction()]
  │
  ├── yield return new WaitForSeconds(_actionDelay)
  │
  ├── actor.OnTurnEnd()
  │     └── EffectComponent.OnTurnEnd() — tick end-of-turn effects nếu có
  │
  ├── CleanupDeadEntities()
  │     foreach e in _allEntities where e.IsDead:
  │       TurnManager.RemoveEntity(e.EntityId)
  │             └── _activeEntities.Remove(id)
  │                 EventBus.Publish(TimelineUpdatedEvent())
  │       SkillManager.UnregisterEntity(e.EntityId)
  │       _allEntities.Remove(e)
  │       _playerTeam / _enemyTeam remove accordingly
  │
  ├── TurnManager.EndTurn(actorId, action.TimelineCost)
  │     ├── _gauges[actorId] -= actionCost  (default 100)
  │     ├── _turnCounter++
  │     └── EventBus.Publish(TurnEndedEvent(actorId))
  │
  └── yield return new WaitForSeconds(_betweenTurnDelay)
```

---

## Flow 12: Kiểm tra thắng/thua

```
CheckVictory():
  allEnemiesDead  = _enemyTeam.All(e => e.IsDead)
  allPlayersDead  = _playerTeam.All(e => e.IsDead)
  
  if allEnemiesDead:
    EndBattle(victory=true)
  else if allPlayersDead:
    EndBattle(victory=false)

EndBattle(isVictory):
  _state = CombatState.BattleEnd
  EventBus.Publish(new CombatEndedEvent { IsVictory = isVictory })
        ↓
  (bên ngoài) CombatBridge.OnCombatEnded() — scene transition
  (bên ngoài) CombatLogger.OnCombatEnded() — log summary
  
  // loop kết thúc, CombatLoop coroutine return
```

---

## Flow 13: Save game

```
SaveManager.Save(slotIndex):
  │
  ├── Collect game state:
  │     data.partyIds     = CombatFlowController.GetPlayerTeam().Select(c => c.EntityId)
  │     data.completedStages = StageProgressTracker.GetCompleted()
  │     data.saveTimestamp = DateTime.Now.ToString()
  │     data.slotIndex    = slotIndex
  │
  ├── json = JsonUtility.ToJson(data)
  ├── path = Application.persistentDataPath + "/save_{slotIndex}.json"
  ├── File.WriteAllText(path, json)
  └── EventBus.Publish(new GameSavedEvent { SlotIndex = slotIndex })
```

---

## Flow 14: Load game

```
SaveManager.Load(slotIndex):
  │
  ├── path = Application.persistentDataPath + "/save_{slotIndex}.json"
  ├── [if !File.Exists(path)] return null
  │
  ├── json = File.ReadAllText(path)
  ├── data = JsonUtility.FromJson<SaveData>(json)
  └── EventBus.Publish(new GameLoadedEvent { SlotIndex = slotIndex })
      return data
```

---

## Flow 15: DataManager load pipeline chi tiết

```
DataManager.LoadAllData():
  │
  ├── LoadCharacters():
  │     textAssets = Resources.LoadAll<TextAsset>("Characters")
  │     foreach asset:
  │       model = JsonUtility.FromJson<CharacterDataModel>(asset.text)
  │       DataCache._characters[model.id] = model
  │
  ├── LoadSkills():
  │     textAssets = Resources.LoadAll<TextAsset>("Skills")
  │     foreach asset:
  │       model = JsonUtility.FromJson<SkillDataModel>(asset.text)
  │       DataCache._skills[model.id] = model
  │
  ├── LoadEnemies():
  │     textAssets = Resources.LoadAll<TextAsset>("Enemies")
  │     foreach asset:
  │       model = JsonUtility.FromJson<EnemyDataModel>(asset.text)
  │       DataCache._enemies[model.id] = model
  │
  ├── LoadStages():
  │     (tương tự)
  │
  ├── DataValidator.ValidateAll(DataCache)
  │     → log warnings nếu có data thiếu/invalid
  │
  └── EventBus.Publish(DataLoadedEvent(charCount, skillCount, enemyCount, stageCount))
```

---

## Flow 16: Damage với AOE (All enemies)

```
// Skill có targeting.rule = "all_enemies"
// CombatFlowController.ResolveTargets() → danh sách tất cả enemy

ExecuteAction(actor, aoeSkillId, allEnemyIds, grade):
  └── ActionResolver.Resolve(actor, [e1, e2, e3], skillData, grade)
        ├── foreach target in [e1, e2, e3]:
        │     ResolveAttack(actor, target, skillData, grade)
        └── // damage áp dụng độc lập cho từng target
```

---

## Flow 17: Stun effect ngăn entity hành động

```
StunEffect.OnTurnStart(owner):
  owner.IsStunned = true  (property trên CombatEntity)

// Trong CombatLoop, sau GetNextActor():
actor = FindEntity(actorId)
if actor.IsStunned:
  actor.OnTurnEnd()             // end turn ngay
  TurnManager.EndTurn(actorId)
  actor.IsStunned = false       // stun consume 1 turn
  continue // → lấy actor tiếp theo
```

---

## Flow 18: Timeline preview

```
// UI muốn hiển thị 5 entity tiếp theo:
List<string> preview = TurnManager.GetTimelinePreview(5)

GetTimelinePreview(n):
  1. Clone _gauges, _speeds, _activeEntities (deep copy)
  2. Simulate GetNextActor() n lần:
     foreach iteration:
       advance gauges (virtual tick)
       add nextActor to result
       subtract 100 from winner gauge (simulate end turn)
  3. return result (List<string> entityIds theo thứ tự)
```

---

## Flow 19: ShieldEffect chặn damage

```
ShieldEffect.OnTurnStart(owner):
  owner.hasShield = true
  owner.shieldAmount = _shieldValue

// Trong HealthComponent.TakeDamage():
if owner.hasShield && owner.shieldAmount > 0:
  absorbed = min(shieldAmount, incomingDamage)
  shieldAmount -= absorbed
  incomingDamage -= absorbed
  if shieldAmount <= 0:
    owner.hasShield = false
    // EffectComponent sẽ remove khi Duration == 0 lúc end turn
netDamage = max(0, incomingDamage)
_currentHP = max(0, _currentHP - netDamage)
```

---

## Tóm tắt luồng chính (one-page summary)

```
App start
  └── DataManager.LoadAllData()

Scene chọn → StartCombat(stageId, partyIds)
  └── EntityFactory creates all entities
  └── CombatFlowController.StartBattle()
        └── CombatLoop() coroutine
              ├── InitBattle: TurnManager+SkillManager.Reset, RNG.Init
              │
              └── LOOP while !IsOver:
                    actor = TurnManager.GetNextActor()    ← CTB gauge tick
                    TurnManager.StartTurn()               → TurnStartedEvent
                    EffectComponent ticks (DOT, stun, shield)
                    
                    if player:  wait SubmitPlayerAction()
                    if enemy:   AIController.DecideAction()
                                TargetSelector.SelectTargets()
                    
                    optionally: open timing window → TimingGrade
                    
                    SkillAction.Validate()                → check resource
                    SkillAction.Execute()
                      SkillManager.UseSkill()             → consume mana/cooldown
                      ActionResolver.Resolve()            → damage/heal/effects
                        StatCalculator.CalculateDamage()  → DEF formula
                        HealthComponent.TakeDamage()      → EntityDeathEvent?
                    
                    SkillManager.TickCooldowns()
                    TurnManager.EndTurn()                 → gauge -= 100
                    CleanupDeadEntities()
                    CheckVictory()
              
              EndBattle() → CombatEndedEvent
```

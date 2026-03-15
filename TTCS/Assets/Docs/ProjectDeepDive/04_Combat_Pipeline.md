# 04 - Combat Pipeline Chuyên Sâu

## 1. Pipeline chuẩn cho một hành động

1. Chọn actor theo CTB.
2. Vào routine theo phe (player/enemy).
3. Thu thập input hoặc AI decision.
4. `ExecuteAction`.
5. `Validate`.
6. `UseSkill` (commit resource).
7. `Resolve` (damage/heal/effect).
8. Tick cooldown + end turn + cleanup.

## 2. Chi phí hành động và nhịp lượt

- Mỗi skill có `actionCost.timelineUnits`.
- `TurnManager.EndTurn(actorId, actionCost)` trừ gauge actor.
- Action cost càng cao -> actor quay lại lượt càng chậm.

## 3. Chi tiết công thức và modifier

### Damage
- `base = ATK * multiplier`
- `defFactor = DEF/(DEF+100)` (cap 75%)
- crit multiplier
- element multiplier (hiện tại stub 1.0)
- clamp min damage = 1

### Timing ảnh hưởng damage
- Khi enemy đánh player:
  - Perfect: x0.2
  - Good: x0.6
  - Miss: x1.0
- Khi player đánh (logic hiện tại):
  - Perfect: x1.2
  - Good: x1.0
  - Miss: x0.8

## 4. Pipeline effect

- `TryApplyEffect` -> roll chance -> build effect -> apply.
- Tick theo `TickTiming` ở đầu hoặc cuối lượt.
- Hết hạn thì remove và publish event remove.

## 5. Pipeline cleanup entity chết

- Remove khỏi `TurnManager`.
- Remove khỏi `SkillManager`.
- Remove khỏi `_allEntities`.
- Mục tiêu: tránh actor chết vẫn được xét lượt và tránh warning lặp.

## 6. Pipeline đồng bộ UI/Visual/Audio

- Combat engine publish event.
- UI update (HUD, turn order, floating text).
- Visual update (animation, vfx).
- Audio update (sfx, bgm).

## 7. Playbook debug theo giai đoạn pipeline

- Sai target: kiểm tra `ResolveTargets` và `TargetSelector`.
- Sai resource: kiểm tra `CanUseSkill`/`UseSkill`.
- Sai damage: kiểm tra `StatCalculator` + timing grade.
- Sai lượt: kiểm tra `GetNextActor` và `EndTurn(actionCost)`.

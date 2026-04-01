# Integration Test Instructions - Sprint 3

## Goal
Validate full integration between:
- SaveManager
- DataManager
- MetaServiceHub (Progression/Team/Gacha/Inventory)
- CombatRewardBridge
- CombatSceneManager

## Scenario 1: New Player Tutorial Gate
1. Start with clean save slot.
2. Ensure `tutorialCompleted = false`.
3. Enter boot flow.
4. Expected:
- Tutorial flow is selected.
- After completion, `tutorialCompleted = true`.
- Returning flow skips tutorial.

## Scenario 2: Level Entry Gate
1. Open Level Select.
2. Query `CanEnterLevel(levelId)` for locked/unlocked levels.
3. Expected:
- Locked level cannot enter.
- Unlock condition met then level becomes enterable.

## Scenario 3: Combat Result -> Reward Bridge
1. Enter a valid level and finish combat with victory.
2. Ensure `CombatEndedEvent(victory=true)` is published.
3. Expected:
- Level completion persisted via `ProgressionService`.
- Inventory receives configured reward items.
- Gacha progression feed applies roll result.

## Scenario 4: Save/Load Persistence
1. Save after completing a level and applying rewards.
2. Reload same slot.
3. Expected:
- `levelProgress`, `inventoryItems`, `gachaPity`, `lineup`, `tutorialCompleted` restored correctly.

## Regression Checks
1. Existing combat flow still starts and ends normally.
2. No null-reference exceptions in reward bridge path.
3. Skill icon fallback path resolves for missing skill icon entries.

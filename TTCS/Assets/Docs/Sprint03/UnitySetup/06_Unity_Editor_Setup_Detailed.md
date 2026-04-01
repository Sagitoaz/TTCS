# Unity Editor Setup Guide - Sprint 3 Dev A (Detailed)

## 1. Prepare Scene and Core Managers
1. Open Unity project `TTCS`.
2. Open target working scene (recommend a dedicated integration scene).
3. In Hierarchy, ensure these GameObjects exist:
- `EventBus`
- `DataManager`
- `SaveManager`
- `MetaServiceHub`
- `CombatSceneManager`
- `CombatFlowController`
- `CombatBridge`
- `AudioController`
4. If missing:
- Create Empty GameObject with exact name.
- Add corresponding script component.
5. Mark persistence managers (`EventBus`, `DataManager`, `SaveManager`, `MetaServiceHub`) as runtime singletons through existing script behavior (`DontDestroyOnLoad` already implemented).

## 2. Data Folder and JSON Validation
1. In Project panel, verify these folders exist:
- `Assets/Data/Chapters`
- `Assets/Data/Levels`
- `Assets/Data/Gacha`
- `Assets/Data/Items`
- `Assets/Data/Meta`
2. Confirm sample files exist:
- `chapter_01.json`, `chapter_02.json`
- `chapter_01_level_01.json`, `chapter_01_level_02.json`, `chapter_02_level_01.json`
- `pool_standard.json`
- `item_potion.json`, `item_energy.json`
- `skill_icon_map.json`
3. Press Play and check console for `DataManager` load summary log. It should include non-zero counts for chapters/levels/gacha/items.

## 3. SaveManager Baseline Setup
1. Select GameObject with `SaveManager`.
2. Enter Play mode.
3. In debug console (or via temporary test button), call `NewGame()`.
4. Verify default Sprint 3 states:
- `schemaVersion = 1`
- `tutorialCompleted = false`
- `unlockedChapters` contains `chapter_01`
- `lineup` has at least `char_warrior`
5. Call `Save(0)` and then `Load(0)` to verify no migration/defaulting exceptions.

## 4. Meta Service Initialization
1. Select `MetaServiceHub` GameObject.
2. Ensure script enabled.
3. Enter Play mode and verify no null warnings for:
- `InventoryService`
- `GachaService`
- `ProgressionService`
- `TeamService`
4. Optional: attach `MetaServicesSmokeTest` to an empty object and run context menu command to verify service chain.

## 5. Combat Reward Bridge Wiring
1. Create Empty GameObject named `CombatRewardBridge`.
2. Add component `CombatRewardBridge`.
3. Ensure `EventBus` singleton is active before combat starts.
4. Run one combat and force victory.
5. Verify in Console:
- Reward bridge log appears.
- Save state updates for progression/inventory/gacha.

## 6. CombatSceneManager Link Verification
1. Select `CombatSceneManager`.
2. Verify fields:
- `_defaultStageId` points to valid stage.
- `_defaultPartyIds` contains valid character IDs.
- Enemy data assets assigned if needed.
3. Press Play and start default combat.
4. Verify `GetCurrentStageData()` is valid during combat result phase.

## 7. Skill Icon Pipeline Setup
1. Ensure `Assets/Data/Meta/skill_icon_map.json` has entries for active skills.
2. Ensure fallback icon path is set.
3. In any UI binder script from Dev B side, use `DataManager.ResolveSkillIcon(skillId)`.
4. Test with:
- Existing skill ID (expect mapped icon path).
- Missing skill ID (expect fallback icon path).

## 8. Manual Verification Script Order (Recommended)
1. Run `SaveManager.NewGame()`.
2. Run `MetaServicesSmokeTest`.
3. Enter level once and complete combat.
4. Save and reload slot.
5. Re-open level select and verify unlock/progress state.
6. Perform one gacha roll and verify inventory/roster update.

## 9. Common Mistakes and Fixes
1. Missing singleton object in scene:
- Symptom: null warnings in console.
- Fix: add missing manager GameObject and component.
2. Invalid JSON field names:
- Symptom: object loads null/empty.
- Fix: match exact property names with data models.
3. Reward bridge not triggered:
- Symptom: combat ends but inventory/progression unchanged.
- Fix: ensure `CombatRewardBridge` object exists and enabled in active scene.
4. Save fields reset unexpectedly:
- Symptom: lineup/chapter unlock disappears after reload.
- Fix: verify save slot index and check migration/default path in `SaveManager`.

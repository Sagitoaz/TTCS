# Unit Test Instructions - Sprint 3

## Automated/Script-Level Checks
1. Open scene that contains `SaveManager` and `MetaServiceHub`.
2. Attach `MetaServicesSmokeTest` to an empty GameObject.
3. In Inspector, click context menu `Run Meta Services Smoke Test`.
4. Verify Console outputs PASS log from `MetaServicesSmokeTest`.

## Service-Level Manual Unit Checks
1. `TeamService`:
- Validate lineup with duplicate character must fail.
- Validate lineup with unlocked character must pass.
2. `InventoryService`:
- Add item then use item in `menu` context.
- Ensure quantity is decreased and not negative.
3. `GachaService`:
- Roll `pool_standard` multiple times.
- Verify pity count increases and resets on rare pull.
4. `ProgressionService`:
- Mark level complete.
- Verify stars and best score are persisted (max merge logic).

## Save Migration Check
1. Create old save-like file (without new Sprint 3 fields).
2. Call `SaveManager.Load(slot)`.
3. Confirm default migration fields are populated:
- `schemaVersion == 1`
- `unlockedChapters` initialized
- `lineup` synchronized from `currentParty` when needed

## Pass Criteria
- No exceptions thrown in test path.
- Service outputs match expected state transitions.

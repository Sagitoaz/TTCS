# Sprint 3 Dev A - Unity Editor Setup (Core Services)

This guide is the operational setup reference for Dev A Sprint 3 core services.
For full test-oriented checklist, also follow:
`aidlc-docs/construction/build-and-test/unity-editor-setup-sprint3-devA.md`

## Quick Execution Order
1. Ensure manager objects exist: EventBus, DataManager, SaveManager, MetaServiceHub.
2. Ensure combat chain objects exist: CombatSceneManager, CombatFlowController, CombatBridge, CombatRewardBridge.
3. Verify Sprint 3 JSON folders/files under `Assets/Data`.
4. Enter Play mode and run:
- New game initialization
- Meta service smoke test
- One combat victory path
- Save/Load validation
5. Confirm no red errors in Console.

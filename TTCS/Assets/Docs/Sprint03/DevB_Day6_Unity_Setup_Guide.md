# Sprint 03 - Day 6 Unity Setup Guide (Dev B)

Sprint: Sprint 03
Developer: Dev B (Flow/UI)
Scope Day 6: Inventory UI, Level Select wiring, Character Collection v1

## 1. Scripts added for Day 6

- Assets/Scripts/Flow/Inventory/InventoryUIController.cs
- Assets/Scripts/Flow/LevelSelect/LevelSelectUIController.cs
- Assets/Scripts/Flow/CharacterCollection/CharacterCollectionUIController.cs

Related updates:
- Assets/Scripts/Core/Save/SaveData.cs (current mana persistence)
- Assets/Scripts/Core/Save/SaveManager.cs (default current mana initialization)
- Assets/Scripts/Flow/Common/FlowController.cs (OpenCharacterCollection)
- Assets/Scripts/Flow/Common/IFlowController.cs (OpenCharacterCollection contract)
- Assets/Scripts/Flow/MainMenu/MainMenuController.cs (optional Character Collection button)

## 2. Inventory scene setup

Scene name suggestion: InventoryScene

1. Create an empty GameObject named InventoryUIRoot.
2. Add component InventoryUIController to InventoryUIRoot.
3. Build list area:
- Create ScrollView content root and assign to Item List Root.
- Create item row prefab with Button + TMP_Text child and assign to Item Row Prefab.
4. Build detail area:
- Add TMP_Text fields and assign to Item Name Text, Item Description Text, Item Quantity Text, Feedback Text.
- Add Use button and assign to Use Item Button.
5. Add Back button and assign to Back Button.
6. Play test:
- Click item row to open detail.
- Click Use Item to consume item using InventoryService.UseItem(..., "menu").
- Click Back to return Main Menu.

## 3. Level Select scene setup

Scene name suggestion: LevelSelectScene

1. Create GameObject LevelSelectUIRoot.
2. Add component LevelSelectUIController.
3. Chapter tabs:
- Create horizontal layout root and assign to Chapter Tab Root.
- Create chapter tab prefab with Button + TMP_Text and assign to Chapter Tab Prefab.
4. Level grid:
- Create grid root and assign to Level Grid Root.
- Create level card prefab with Button + TMP_Text and assign to Level Card Prefab.
5. Assign Back button.
6. Play test:
- Verify chapter tabs render.
- Verify each chapter shows level cards.
- Click level card to call FlowController.EnterCombat(levelId, lineup).

Notes:
- If Data/Chapters or Data/Levels is missing, controller auto-falls back to mock 3 chapters x 3 levels.

## 4. Character Collection scene setup

Scene name suggestion: CharacterCollectionScene

1. Create GameObject CharacterCollectionUIRoot.
2. Add component CharacterCollectionUIController.
3. Card list:
- Create grid root and assign to Card Grid Root.
- Create card prefab with Button + TMP_Text and assign to Character Card Prefab.
4. Search and sorting:
- Add TMP_InputField and assign to Search Input.
- Wire UI toggles/dropdowns to:
  - OnSortRareChanged(bool ascending)
  - OnSortLevelChanged(bool ascending)
5. Detail panel:
- Assign Name/Rarity/Level/Stats/HP/Mana TMP_Text fields.
- Assign Portrait Image and Skill Icon Image.
6. Actions:
- Assign Feed Button and Back Button.
- Optionally assign Feedback Text.

## 5. Rule validation for Day 6

Implemented rule:
- Every level-up from feed action restores full HP and full Mana.

How to verify:
1. Open Character Collection scene.
2. Select a character.
3. Click Feed button.
4. Confirm Level increments by 1 and HP/Mana both become current=max immediately.

## 6. Main Menu optional wiring

MainMenuController now supports optional Character Collection button:
- Field: Character Collection Button
- If assigned, click will call FlowController.OpenCharacterCollection().

If not assigned, no runtime error occurs.

## 7. Quick smoke checklist (Day 6)

- Inventory: list -> detail -> use item -> back works.
- Level Select: chapter tab -> level card -> EnterCombat route works.
- Character Collection:
- list renders unlocked characters
- search by name/id works
- sort by rarity and level works
- click card shows detail with HP/Mana
- feed updates level and restores full HP/Mana
- Main Menu optional button opens Character Collection scene.

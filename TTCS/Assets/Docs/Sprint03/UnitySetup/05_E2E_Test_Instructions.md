# End-to-End Test Instructions - Sprint 3

## E2E-01 New Player Full Loop
1. Start from empty save.
2. Enter game -> tutorial.
3. Finish tutorial -> main menu.
4. Select chapter/level -> enter combat.
5. Win combat -> apply rewards -> save.
6. Restart game and load slot.
7. Expected:
- Tutorial is skipped on next boot.
- Level progress and rewards are preserved.

## E2E-02 Returning Player Loop
1. Load an existing progressed save.
2. Navigate main menu -> team setup -> level select.
3. Enter combat with saved lineup.
4. Win/lose and return to flow.
5. Expected:
- No forced tutorial.
- Team lineup and inventory state remain coherent.

## E2E-03 Gacha + Inventory Consistency
1. From main menu, perform gacha roll.
2. Verify reward reflected in roster or inventory.
3. Use one consumable item in menu context.
4. Save and reload.
5. Expected:
- Gacha pity and rewards persisted.
- Item quantity persisted after use.

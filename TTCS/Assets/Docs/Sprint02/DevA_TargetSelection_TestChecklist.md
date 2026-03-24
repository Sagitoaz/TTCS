# Dev A - Target Selection Manual Test Checklist

> Feature: Manual target selection for single-target skills
> Scope: Player actions only (`single_enemy`, `single_ally`)
> Date: 2026-03-24

## 1) Test Goal

Validate these behaviors:
- Player can choose enemy target for single-target attack skills instead of auto-target.
- Player can choose ally target for single-target buff/heal skills, excluding caster.
- On entering target-selection mode, camera focuses default target.
- Player can cycle targets with A/D.
- Player can confirm with Space.
- Player can cancel with X and return to skill menu.
- If only one valid target remains, action auto-confirms.

## 2) Setup

1. Open combat scene used for Sprint 2 integration.
2. Ensure at least:
- 2 alive enemies for enemy-target cycle tests.
- 2 alive player characters for ally-target cycle tests.
3. Ensure tested skills exist:
- One `single_enemy` attack skill (example: slash/fireball style skill).
- One `single_ally` buff/heal skill (example: heal/shield style skill).
4. Enter Play Mode.

## 3) Input Contract

- `A`: previous target
- `D`: next target
- `Space`: confirm selected target
- `X`: cancel target selection (return to skill menu)

Fallback accepted during testing:
- `Escape` may also cancel in current implementation.

## 4) Test Cases

### TC-01: Single Enemy Skill enters manual selection

Steps:
1. Start player turn.
2. Click a `single_enemy` skill.

Expected:
- Skill is not executed immediately.
- Target-selection mode is active.
- One enemy is highlighted as current selection.
- Camera focuses selected enemy.
- Hint box appears (A/D, Space, X).

Pass/Fail: ____

### TC-02: Default enemy target is first valid enemy by list order

Steps:
1. During player turn, click the same `single_enemy` skill.
2. Observe first highlighted enemy before pressing any key.

Expected:
- Default target is enemy index 0 in current valid candidate list.
- Camera focus matches highlighted enemy.

Pass/Fail: ____

### TC-03: Enemy cycling with A/D

Steps:
1. Enter enemy target-selection mode.
2. Press `D` repeatedly.
3. Press `A` repeatedly.

Expected:
- `D` moves highlight to next enemy (wrap-around at end).
- `A` moves highlight to previous enemy (wrap-around at start).
- Camera follows currently highlighted enemy.

Pass/Fail: ____

### TC-04: Confirm enemy target with Space

Steps:
1. Enter enemy target-selection mode.
2. Move to non-default enemy with A/D.
3. Press `Space`.

Expected:
- Selected target receives the skill effect.
- Mana/cooldown are consumed once.
- Turn proceeds with normal combat flow.

Pass/Fail: ____

### TC-05: Cancel enemy target with X

Steps:
1. Enter enemy target-selection mode.
2. Press `X`.

Expected:
- No skill execution.
- No mana consumption, no cooldown trigger.
- Return to skill menu for same acting character.
- Target highlight and camera focus reset.

Pass/Fail: ____

### TC-06: Single Ally skill excludes caster

Steps:
1. Start turn as character A.
2. Click a `single_ally` skill.

Expected:
- Candidate list contains alive allies except character A.
- Character A is never highlighted as selectable target.

Pass/Fail: ____

### TC-07: Ally cycling and confirm

Steps:
1. Enter `single_ally` target-selection mode.
2. Use A/D to switch between valid allies.
3. Press `Space` to confirm.

Expected:
- Correct ally receives buff/heal.
- Caster is still excluded.
- Camera and highlight follow selected ally while choosing.

Pass/Fail: ____

### TC-08: Cancel ally selection with X

Steps:
1. Enter `single_ally` target-selection mode.
2. Press `X`.

Expected:
- Return to skill menu.
- No resource spending.
- No cast event should be visible in gameplay results.

Pass/Fail: ____

### TC-09: Auto-confirm with one valid enemy

Steps:
1. Ensure only 1 enemy remains alive.
2. Click a `single_enemy` skill.

Expected:
- Skill executes immediately without entering manual selection mode.
- Correct enemy is targeted.

Pass/Fail: ____

### TC-10: Auto-confirm with one valid ally target

Steps:
1. Ensure only one alive ally exists besides caster.
2. Click a `single_ally` skill.

Expected:
- Skill executes immediately without entering manual selection mode.
- Only valid ally is targeted.

Pass/Fail: ____

### TC-11: Turn end cleanup while selecting target

Steps:
1. Enter target-selection mode.
2. Force turn transition (debug path or battle state change).

Expected:
- Selection mode exits safely.
- Highlight cleared.
- Camera restored.
- No stuck input state in next turn.

Pass/Fail: ____

## 5) Regression Quick Check

- `all_enemies` skill still targets all alive enemies.
- `self` skill still targets caster directly.
- Enemy AI targeting behavior is unchanged.
- Existing timing/telegraph flow still works.

Result: ____

## 6) Known Notes for QA

- Current implementation uses candidate list order for A/D cycle and default selection.
- If visual left/right order differs from candidate list order, validate spawn/registration order in scene setup.

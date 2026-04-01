# Sprint 3 Dev B - UI Elements Checklist

> **Ngày tạo**: 2026-04-01 (Ngày 1)  
> **Mục đích**: Liệt kê tất cả UI element cần render trong từng scene

---

## 📋 UI Element Inventory

### Tutorial Scene UI Elements

- [ ] **Step Text Panel**
  - [ ] title (e.g., "Step 1: Welcome")
  - [ ] description text
  - [ ] animated text reveal (optional)

- [ ] **Navigation Buttons**
  - [ ] Next button (enabled on all steps except last)
  - [ ] Skip button (always enabled)
  - [ ] Complete button (last step only)

- [ ] **Visual Elements**
  - [ ] Character portrait (optional)
  - [ ] Background image
  - [ ] Step indicator (1/8, 2/8, ...)

---

### Main Menu Scene UI Elements

- [ ] **Header Panel**
  - [ ] Game title or welcome text
  - [ ] Player level display
  - [ ] Coins/currency display

- [ ] **Navigation Buttons**
  - [ ] Team button
  - [ ] Gacha button
  - [ ] Inventory button
  - [ ] Level Select button
  - [ ] Settings button (optional)

- [ ] **Visual Elements**
  - [ ] Background image
  - [ ] Character portrait or banner
  - [ ] Button glow animation on hover

---

### Team Formation Scene UI Elements

- [ ] **Characters List Panel**
  - [ ] Scrollable list container
  - [ ] Character card prefab (icon, name, rarity, level)
  - [ ] Selection highlight

- [ ] **Lineup Display Panel**
  - [ ] 3 Slot displays (Slot 1, Slot 2, Slot 3)
  - [ ] Selected character display per slot
  - [ ] Remove character button (per slot)

- [ ] **Action Buttons**
  - [ ] Validate button
  - [ ] Back button

- [ ] **Feedback Panel**
  - [ ] Error message display (red text)
  - [ ] Success message (optional)
  - [ ] Validation status indicator

---

### Gacha Scene UI Elements

- [ ] **Pool Selector Panel**
  - [ ] Dropdown UI for pool selection
  - [ ] Pool info display (rates, pity counter)

- [ ] **Roll Buttons Panel**
  - [ ] Roll 1x button
  - [ ] Roll 10x button
  - [ ] Button state (enabled/disabled)

- [ ] **Result Modal Panel**
  - [ ] Result background image
  - [ ] Character portrait/icon
  - [ ] Rarity display (stars)
  - [ ] Character name
  - [ ] Confirm button (OK)
  - [ ] Animation trigger (spinning wheel, card flip, etc.)

- [ ] **Other Elements**
  - [ ] Back button
  - [ ] History/Log button (optional)
  - [ ] Balance display (currency, pity count)

---

### Inventory Scene UI Elements

- [ ] **Items List Panel**
  - [ ] Scrollable list container
  - [ ] Item card prefab (icon, name, quantity, rarity)
  - [ ] Selection highlight

- [ ] **Item Detail Panel**
  - [ ] Item icon (enlarged)
  - [ ] Item name and description
  - [ ] Item quantity display
  - [ ] Item usage rule (e.g., "Can use outside combat")
  - [ ] Use button
  - [ ] Quantity input (if applicable)

- [ ] **Confirmation Dialog**
  - [ ] Dialog background (semi-transparent)
  - [ ] Confirmation message
  - [ ] Confirm button
  - [ ] Cancel button

- [ ] **Other Elements**
  - [ ] Back button
  - [ ] Empty state display (if no items)
  - [ ] Item usage result feedback

---

### Level Select Scene UI Elements

- [ ] **Chapter Selector Panel**
  - [ ] Chapter tab buttons (Chapter 1, 2, 3, ...)
  - [ ] Active/inactive tab styling
  - [ ] Smooth transition between chapters

- [ ] **Level Grid Panel**
  - [ ] Grid layout (3x3 or flexible)
  - [ ] Level card prefab per level
    - [ ] Level number/name (e.g., "1-1")
    - [ ] Level icon/image
    - [ ] Stars earned display (0-3, or not attempted)
    - [ ] Lock icon (if locked)
    - [ ] Unlock requirement (tooltip on hover)

- [ ] **Level Detail Panel (Optional)**
  - [ ] Level description
  - [ ] Enemy lineup (if known)
  - [ ] Recommended power level
  - [ ] Difficulty indicator

- [ ] **Action Buttons**
  - [ ] Back button
  - [ ] Enter Level button (if level detail panel exists)

- [ ] **Visual Elements**
  - [ ] Chapter background
  - [ ] Progress bar (chapters completed)
  - [ ] Level completion percentage

---

### Combat Result Scene UI Elements

- [ ] **Result Banner**
  - [ ] Victory/Defeat text (large, animated)
  - [ ] Background color (green for victory, red for defeat)

- [ ] **Result Summary Panel**
  - [ ] Stars earned display (0-3)
  - [ ] Score/time display
  - [ ] Performance feedback (e.g., "Good", "Great", "Perfect")

- [ ] **Reward Display Panel**
  - [ ] XP earned
  - [ ] Gold earned
  - [ ] Items earned (icon + quantity)
  - [ ] Characters earned (if any)

- [ ] **Action Buttons**
  - [ ] Continue button (or "Back to Menu")
  - [ ] Alternative navigation (optional: "Retry Level")

- [ ] **Visual Elements**
  - [ ] Reward icons/images
  - [ ] Animated number pop-ups (for XP, gold)
  - [ ] Sparkle effects on rewards

---

### Boot Scene UI Elements

- [ ] **Loading Screen (Optional)**
  - [ ] Loading bar or spinner
  - [ ] Loading text (e.g., "Initializing...")
  - [ ] Game logo or splash image

---

## 🎨 Shared UI Components

These components appear in multiple scenes:

- [ ] **Back Button** (persistent style)
  - [ ] Location: Top-left or bottom-left
  - [ ] Behavior: Navigate to previous scene
  - [ ] Styling: Consistent across all scenes

- [ ] **Panel Containers**
  - [ ] Semi-transparent background (optional)
  - [ ] Border/frame styling
  - [ ] Consistent padding and margins

- [ ] **Button Styles**
  - [ ] Idle state (color, scale)
  - [ ] Hover state (glow, scale up)
  - [ ] Pressed state (scale, sound)
  - [ ] Disabled state (grayed out)

- [ ] **Text Styles**
  - [ ] Title (large, bold)
  - [ ] Body (normal size)
  - [ ] Small (for hints/tooltips)
  - [ ] Error (red color)
  - [ ] Success (green color)

- [ ] **Icons/Sprites**
  - [ ] Rarity stars (1-5 stars)
  - [ ] Lock icon (for locked levels)
  - [ ] Currency icons (gold, gems, etc.)
  - [ ] UI buttons (generic button icons)
  - [ ] Character portraits (placeholder or actual)

---

## 🎬 Animation/FX Checklist

These effects enhance UX:

- [ ] **Fade In/Out Transitions**
  - [ ] Between scenes
  - [ ] Panel reveals

- [ ] **Button Animations**
  - [ ] Scale on hover
  - [ ] Color tint on press
  - [ ] Bounce/shake feedback

- [ ] **Text Animations**
  - [ ] Typewriter effect (tutorial steps)
  - [ ] Fade in (result display)

- [ ] **Reward Animations**
  - [ ] Gacha card flip
  - [ ] Reward pop-up numbers (XP, gold)
  - [ ] Sparkle/shine effects

- [ ] **Audio Cues**
  - [ ] Button click sound
  - [ ] Success/error sound
  - [ ] Roll/gacha sound effect
  - [ ] Victory fanfare

---

## 📐 Layout / Canvas Settings

All scenes should follow consistent canvas scaling:

- [ ] **Canvas Settings**
  - [ ] Render Mode: Screen Space - Overlay
  - [ ] UI Scale Mode: Scale with Screen Size
  - [ ] Reference Resolution: 1920x1080 (or game's standard)
  - [ ] Safe Area enabled (for mobile devices)

- [ ] **Panel Layout**
  - [ ] Use anchors for responsive design (not hardcoded positions)
  - [ ] Maintain safe margins (especially for mobile)
  - [ ] Test on different aspect ratios (16:9, 4:3, etc.)

---

## ✅ UI Element Completion Checklist (Ngày 1-2)

### Sáng Ngày 1
- [ ] List out all UI elements (done - this doc)
- [ ] Prioritize elements by scene
- [ ] Identify placeholders vs. final assets needed

### Chiều Ngày 1
- [ ] Itemize skill icon requirements
  - [ ] Where skill icons appear in UI (Team, Combat, etc.)
  - [ ] Size requirements (32x32, 64x64, etc.)
  - [ ] Fallback icon design

### Ngày 2
- [ ] Setup Canvas and EventSystem in each scene
- [ ] Create button prefabs with consistent styling
- [ ] Create panel container prefabs
- [ ] Create character/item card prefabs
- [ ] Setup placeholder texts and images

---

## 🔗 Dependencies on Dev A

| UI Element | Dev A Responsibility | What Dev B Needs |
|---|---|---|
| Character icons | Asset production | Final sprite sheet + data mapping |
| Item icons | Asset production | Final sprite sheet + data mapping |
| Level thumbnails | Asset/level design | Thumbnail images or auto-generate |
| Skill icons | Asset production + mapping | Sprite sheet + skill_icon_map.json |
| Rarity stars | Asset production | Star sprite sheet |
| Lock icon | Asset production | Locked state indicator sprite |

---

**Created**: 2026-04-01  
**Dev**: Dev B  
**Status**: Draft - Ready for prioritization on Ngày 2

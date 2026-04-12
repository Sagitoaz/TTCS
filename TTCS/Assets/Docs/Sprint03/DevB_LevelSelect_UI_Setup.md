# DevB Level Select UI Setup

## 1) Scripts da them/sua
- Level Select controller moi: Assets/Scripts/Flow/LevelSelect/LevelSelectUIController.cs
- Chapter prefab view: Assets/Scripts/Flow/LevelSelect/ChapterView.cs
- Level prefab view: Assets/Scripts/Flow/LevelSelect/LevelView.cs
- Enemy prefab view: Assets/Scripts/Flow/LevelSelect/LevelEnemyView.cs
- Reward item prefab view: Assets/Scripts/Flow/LevelSelect/LevelRewardItemView.cs
- Stage data model mo rong: Assets/Scripts/Data/StageDataModel.cs

## 2) Cau truc Resources can tao
Tat ca sprite deu load bang Resources.Load, khong keo tha sprite vao SerializeField.

Tao cac folder sau:
- Assets/Resources/UI/LevelSelect/Chapter
- Assets/Resources/UI/LevelSelect/Level
- Assets/Resources/UI/Enemy
- Assets/Resources/UI/Border
- Assets/Resources/UI/Reward
- Assets/Resources/UI/Star

Da co san icon element/role:
- Assets/Resources/UI/Element
- Assets/Resources/UI/Role

## 3) Quy uoc ten file sprite
### 3.1 Chapter background
- UI/LevelSelect/Chapter/{chapterId}_unlocked
- UI/LevelSelect/Chapter/{chapterId}_locked

Vi du:
- chapter_01_unlocked.png
- chapter_01_locked.png

### 3.2 Level background
- UI/LevelSelect/Level/level_unlocked
- UI/LevelSelect/Level/level_locked

Vi du:
- level_unlocked.png
- level_locked.png

### 3.3 Enemy card border/background theo loai
- UI/Enemy/enemy_border_common
- UI/Enemy/enemy_border_elite
- UI/Enemy/enemy_border_boss
- UI/Enemy/enemy_bg_common
- UI/Enemy/enemy_bg_elite
- UI/Enemy/enemy_bg_boss

### 3.4 Reward item border/background theo rarity
- UI/Border/{rarity}_border
- UI/Border/{rarity}_bg

Theo file hien co trong project:
- UI/Border/r_border
- UI/Border/sr_border
- UI/Border/ssr_border

### 3.5 Reward icon cho tien te
- UI/Reward/icon_gold
- UI/Reward/icon_exp

### 3.6 Star icon
- UI/Star/star_unlocked
- UI/Star/star_locked

## 4) Setup Scene LevelSelect
Gan script LevelSelectUIController len GameObject root (VD: LevelSelectUIRoot).

### 4.1 Chapter Scroll (Vertical)
- Tao Scroll View vertical
- Content la RectTransform (VD: ChapterContent)
- Prefab item la ChapterView prefab
- Gan vao:
  - Chapter Content -> ChapterContent
  - Chapter View Prefab -> prefab ChapterView
  - Chapter Spacing/Padding -> tuy UI

### 4.2 Level Scroll (Vertical)
- Tao Scroll View vertical
- Content la RectTransform (VD: LevelContent)
- Prefab item la LevelView prefab
- Gan vao:
  - Level Content -> LevelContent
  - Level View Prefab -> prefab LevelView
  - Level Spacing/Padding -> tuy UI

### 4.3 Level Details
Gan cac field:
- Detail Level Title Text
- Enemy Root
- Enemy View Prefab (LevelEnemyView)
- Star Condition Text 1/2/3
- Star State Image 1/2/3

### 4.4 Reward Scroll (Horizontal)
- Tao Scroll View horizontal cho reward
- Content la RectTransform (VD: RewardContent)
- Prefab item la LevelRewardItemView prefab
- Gan vao:
  - Reward Content
  - Reward Item View Prefab
  - Reward Spacing/Padding

### 4.5 Buttons
- Fight Button
- Back Button

## 5) Setup Prefab ChapterView
Component can co:
- Button
- TMP_Text chapter index
- TMP_Text chapter name
- Image background

Gan vao script ChapterView:
- _button
- _chapterIndexText
- _chapterNameText
- _backgroundImage

## 6) Setup Prefab LevelView
Component can co:
- Button
- TMP_Text order (Chapter x - y)
- TMP_Text level name
- TMP_Text recommend level
- Transform enemyElementRoot
- Image enemyElementIconPrefab (prefab icon nho)
- Image background
- GameObject completed marker (icon da clear man)

Gan vao script LevelView:
- _button
- _orderText
- _levelNameText
- _recommendLevelText
- _enemyElementRoot
- _enemyElementIconPrefab
- _backgroundImage
- _completedObject

## 7) Setup Prefab LevelEnemyView
Component can co:
- Image border
- Image background
- TMP_Text level
- Image element icon
- Image role icon
- Image portrait

Gan vao script LevelEnemyView:
- _borderImage
- _backgroundImage
- _enemyLevelText
- _elementIconImage
- _roleIconImage
- _portraitImage

## 8) Setup Prefab LevelRewardItemView
Component can co:
- Image border
- Image background
- Image item icon
- TMP_Text quantity label (VD: Exp x1200)

Gan vao script LevelRewardItemView:
- _borderImage
- _backgroundImage
- _itemIconImage
- _quantityText

## 9) Data hien thi
- Level -> Stage thong qua level.stageId
- Enemy list + enemy level lay tu stage.encounters[].enemies[] (toi da 3 card)
- Star condition lay tu stage.rewards.stars[0..2].condition
- Trang thai sao lay tu save best stars (ProgressionService.GetLevelState(levelId).Stars)
- Reward hien thi tu stage.rewards.firstClear (exp/gold/items)

## 10) Luu y
- Controller tu dong tinh lai chieu cao content chapter/level theo so luong item (vertical)
- Controller tu dong tinh lai chieu rong content reward theo so luong item (horizontal)
- Neu thieu sprite theo naming o tren, UI se tu dong an image (alpha = 0)

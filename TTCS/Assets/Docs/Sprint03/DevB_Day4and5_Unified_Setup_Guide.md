# Sprint 03 - Day 4 (補充) + Day 5 Unified Setup Guide (Dev B)

> **Status**: Merged Day 4.5 (Skill Rendering補充) + Day 5 (Skill Icon Pipeline + Gacha UI v1)  
> **Date**: 2026-04-03  
> **Dev**: Dev B  
> **Mục tiêu**:  
> 1. Hoàn thiện skill image rendering trong QuickInfo (từ Day 4)  
> 2. Xây dựng Skill Icon Pipeline (loader + fallback)  
> 3. Tạo Gacha scene layout + controller  
> 4. Test full flow Day 4.5 + Day 5

---

## 📌 Tóm tắt Thay đổi

| Phần | Ngày | Trạng thái | Ghi chú |
|------|------|-----------|---------|
| QuickInfo Skill Rendering | 4.5 | ✅ Thêm | Bổ sung từ ngày 4 |
| SkillIconLoader class | 5 | ✅ Thêm | Data-driven mapping |
| skill_icon_map.json | 5 | ✅ Thêm | Sample config |
| Gacha Scene | 5 | ✅ Thêm | Layout + controller |
| Test checklist | 5 | ✅ Thêm | End-to-end flow |

---

# Part 1: Day 4 補充 — Skill Image Rendering in QuickInfo

**Mục tiêu**: Hoàn thiện việc render skill icons trong QuickInfo panel của TeamFormation picker.

> **Recall**: Ngày 4 đã tạo QuickInfo block (name, role, element, portrait, skills). Bây giờ chúng ta sẽ implement skill icon + name display.

**🆕 Day 4 補充 Update**: Click vào skill icon sẽ hiển thị panel mô tả skill (name + description) bên cạnh skill vừa click.

## 1.1 Tạo SkillIconQuickItemView.cs

File: `Assets/Scripts/Flow/TeamFormation/SkillIconQuickItemView.cs`

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TTCS.Flow.TeamFormation
{
    /// <summary>
    /// Hiển thị một skill item nhanh trong QuickInfo: icon + name.
    /// </summary>
    public class SkillIconQuickItemView : MonoBehaviour
    {
        [SerializeField] private Image skillIcon;
        [SerializeField] private TextMeshProUGUI skillNameText;

        public void SetSkill(Sprite icon, string name)
        {
            if (skillIcon != null)
            {
                skillIcon.sprite = icon;
                skillIcon.preserveAspect = true;
            }

            if (skillNameText != null)
            {
                skillNameText.text = name;
            }
        }

        public void Clear()
        {
            if (skillIcon != null)
                skillIcon.sprite = null;
            if (skillNameText != null)
                skillNameText.text = "";
        }
    }
}
```

## 1.2 Update TeamFormationUIController.cs

**Thêm phần skill rendering** vào `OnHighlightCharacter()` method:

```csharp
// Existing code...
// Hiển thị QuickInfo
if (_currentHighlightedCharacterId != null)
{
    var charMeta = _dataManager.Characters.FirstOrDefault(c => c.Id == _currentHighlightedCharacterId);
    if (charMeta != null)
    {
        // Existing: name, role, element, portrait
        _quickInfoNameText.text = charMeta.Name;
        _quickInfoRoleText.text = charMeta.Role;
        _quickInfoElementText.text = charMeta.Element;
        
        // Portrait
        var portraitSprite = _dataManager.LoadCharacterPortrait(charMeta.Id);
        if (_quickInfoPortrait != null)
            _quickInfoPortrait.sprite = portraitSprite;

        // 🆕 Skill rendering
        DisplaySkillsInQuickInfo(charMeta);
    }
}

private void DisplaySkillsInQuickInfo(CharacterMeta charMeta)
{
    // Clear old skill items
    foreach (Transform child in _quickInfoSkillRoot)
    {
        Destroy(child.gameObject);
    }

    // Load character's skills
    var skillIds = charMeta.SkillIds; // Assume CharacterMeta has this
    foreach (var skillId in skillIds)
    {
        var skillMeta = _dataManager.Skills.FirstOrDefault(s => s.Id == skillId);
        if (skillMeta != null)
        {
            var skillItem = Instantiate(_quickInfoSkillItemPrefab, _quickInfoSkillRoot);
            var skillView = skillItem.GetComponent<SkillIconQuickItemView>();
            
            // Load skill icon (use loader)
            var skillIcon = _skillIconLoader.LoadSkillIcon(skillId);
            skillView.SetSkill(skillIcon, skillMeta.Name);
        }
    }
}
```

**Thêm field:**
```csharp
[SerializeField] private SkillIconQuickItemView _quickInfoSkillItemPrefab;
private SkillIconLoader _skillIconLoader;
```

**Trong Start/Awake:**
```csharp
void Start()
{
    // ... existing code ...
    
    // 🆕 Initialize SkillIconLoader
    _skillIconLoader = new SkillIconLoader(_dataManager);
}
```

## 1.2補充 Tạo SkillDetailPanelView.cs + Wire Click Handler

**File**: `Assets/Scripts/Flow/TeamFormation/SkillDetailPanelView.cs` (đã tạo sẵn)

**Updates in TeamFormationSkillQuickItemView**:
- Add click event: `Action<string, string, Vector3> OnSkillDetailClicked`
- Store skill ID + description khi `Bind()` được gọi
- Wire Button click → trigger `OnSkillDetailClicked`

**Updates in TeamFormationUIController**:
- Add field: `[SerializeField] private SkillDetailPanelView _skillDetailPanel;`
- Pass skill metadata (id + description) khi spawn skill items:
  ```csharp
  item.Bind(icon, name, skillId, description);
  item.OnSkillDetailClicked += OnSkillDetailClicked;
  ```
- Implement handler:
  ```csharp
  private void OnSkillDetailClicked(string skillId, string skillDescription, Vector3 clickPosition)
  {
      if (_skillDetailPanel != null)
      {
          _skillDetailPanel.ShowSkillDetail(skillId, skillDescription, clickPosition);
      }
  }
  ```

## 1.3 Setup Unity Scene (TeamFormationScene)

### 1.3.1 Tạo Skill Item Prefab

1. Tạo prefab mới: `Assets/Prefabs/UI/TeamFormation/SkillIconQuickItem.prefab`
2. Structure:
   ```
   SkillIconQuickItem (Panel with horizontal layout)
   ├── SkillIcon (Image, size 40x40)
   └── SkillName (TextMeshProUGUI, size 200x40)
   ```

3. **SkillIcon**:
   - Set size: 40x40
   - Set color: White
   - Preserve Aspect: **ON**

4. **SkillName**:
   - Font Size: 20
   - Alignment: Left Center
   - Color: White

5. Add component: `SkillIconQuickItemView`

6. Drag UI elements vào Inspector:
   - Skill Icon: SkillIcon Image
   - Skill Name Text: SkillName TextMeshPro

### 1.3.2 Update QuickInfo Panel

1. Mở scene: `TeamFormationScene`
2. Tìm `PickerPanel -> QuickInfoPanel` (hoặc tạo nếu chưa có)
3. Trong QuickInfo, tìm hoặc tạo `QuickInfoSkillRoot` (VerticalLayoutGroup)
4. Xóa old skill items nếu có
5. Drag prefab `SkillIconQuickItem` vào `QuickInfoSkillRoot` (1 instance tạm để save reference)

### 1.3.3 Gán Reference vào TeamFormationUIController

1. Chọn `TeamFormationManager` GameObject
2. Tìm `TeamFormationUIController` Inspector
3. Gán:
   - **Quick Info Skill Item Prefab** <- SkillIconQuickItem prefab tạo ở 1.3.1

### 1.3.4 Tạo Skill Detail Panel (🆕 Day 4 補充)

1. Trong PickerPanel, tạo **SkillDetailPanel** (Panel UI, initially inactive)
2. Structure:
   ```
   SkillDetailPanel (Panel, inactive)
   ├── Background (Image, dark/semi-transparent)
   ├── SkillNameText (TextMeshProUGUI, size 300x60)
   └── SkillDescriptionText (TextMeshProUGUI, size 400x200)
   ```

3. **SkillDetailPanel settings**:
   - Anchor: Top-Left (hoặc tùy chọn vị trí)
   - Size: 450x300
   - Active: OFF (initially hidden)
   - Add component: `SkillDetailPanelView`

4. **Background settings**:
   - Color: Black (alpha = 0.8, hoặc color của lựa chọn)
   - Image Type: Simple
   - Raycast Target: ON

5. **SkillNameText** (child of SkillDetailPanel):
   - Font Size: 28
   - Alignment: Top-Left
   - Color: White
   - Bold style

6. **SkillDescriptionText** (child of SkillDetailPanel):
   - Font Size: 18
   - Alignment: Top-Left
   - Color: Light Grey
   - Word Wrap: ON
   - Layout: Preferred Size fit content

7. **Gán reference vào SkillDetailPanelView**:
   - Skill Name Text <- SkillNameText
   - Skill Description Text <- SkillDescriptionText
   - Canvas Group <- SkillDetailPanel (hoặc tạo mới)

8. **Gán SkillDetailPanel vào TeamFormationUIController**:
   - Chọn `TeamFormationManager`
   - Field: **Skill Detail Panel** <- SkillDetailPanel GameObject

---

# Part 2: Day 5 — Skill Icon Pipeline & Gacha Scene

**Mục tiêu**: Xây dựng skill icon loading system và tạo Gacha scene.

## 2.1 Tạo SkillIconLoader.cs

File: `Assets/Scripts/Flow/Common/SkillIconLoader.cs`

```csharp
using UnityEngine;
using System.Collections.Generic;

namespace TTCS.Flow
{
    /// <summary>
    /// Loads skill icons từ asset path mapping. Fallback to default texture nếu missing.
    /// </summary>
    public class SkillIconLoader
    {
        private readonly DataManager _dataManager;
        private Dictionary<string, Sprite> _skillIconCache;
        private Sprite _fallbackIcon;

        public SkillIconLoader(DataManager dataManager)
        {
            _dataManager = dataManager;
            _skillIconCache = new Dictionary<string, Sprite>();
            LoadFallbackTexture();
        }

        public Sprite LoadSkillIcon(string skillId)
        {
            // Check cache
            if (_skillIconCache.TryGetValue(skillId, out var cached))
            {
                return cached;
            }

            // Try to load from mapping
            var iconPath = GetIconPathForSkill(skillId);
            if (!string.IsNullOrEmpty(iconPath))
            {
                var sprite = Resources.Load<Sprite>(iconPath);
                if (sprite != null)
                {
                    _skillIconCache[skillId] = sprite;
                    LogDebug($"[SkillIconLoader] Loaded skill icon: {skillId} from {iconPath}");
                    return sprite;
                }
            }

            // Fallback
            LogDebug($"[SkillIconLoader] Fallback for skill: {skillId}");
            _skillIconCache[skillId] = _fallbackIcon;
            return _fallbackIcon;
        }

        /// <summary>
        /// Gets icon asset path từ skill_icon_map.json config.
        /// </summary>
        private string GetIconPathForSkill(string skillId)
        {
            // Load config từ Assets/Data/Meta/skill_icon_map.json via DataManager
            var configJson = Resources.Load<TextAsset>("Data/skill_icon_map");
            if (configJson == null)
            {
                LogDebug("[SkillIconLoader] skill_icon_map.json not found in Assets/Data/Meta");
                return null;
            }

            var config = JsonUtility.FromJson<SkillIconMapConfig>(configJson.text);
            var entry = config.mappings.Find(m => m.skillId == skillId);
            return entry?.assetPath;
        }

        private void LoadFallbackTexture()
        {
            // Load fallback texture từ Resources
            _fallbackIcon = Resources.Load<Sprite>("Data/Fallback/skill_fallback");
            if (_fallbackIcon == null)
            {
                // Create default white texture fallback
                var texture = new Texture2D(1, 1);
                texture.SetPixel(0, 0, Color.gray);
                texture.Apply();
                _fallbackIcon = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
                LogDebug("[SkillIconLoader] Created inline fallback texture");
            }
        }

        private void LogDebug(string message)
        {
            Debug.Log($"[Flow] {message}");
        }
    }

    [System.Serializable]
    public class SkillIconMapConfig
    {
        public List<SkillIconEntry> mappings = new List<SkillIconEntry>();
    }

    [System.Serializable]
    public class SkillIconEntry
    {
        public string skillId;
        public string assetPath; // e.g. "Sprites/Skills/slash_01"
    }
}
```

## 2.2 Tạo skill_icon_map.json

File: `Assets/Data/Meta/skill_icon_map.json`

```json
{
  "mappings": [
    {
      "skillId": "skill_warrior_slash",
      "assetPath": "Sprites/Skills/skill_warrior_slash"
    },
    {
      "skillId": "skill_warrior_defensive",
      "assetPath": "Sprites/Skills/skill_warrior_defensive"
    },
    {
      "skillId": "skill_mage_fireball",
      "assetPath": "Sprites/Skills/skill_mage_fireball"
    },
    {
      "skillId": "skill_mage_frostbolt",
      "assetPath": "Sprites/Skills/skill_mage_frostbolt"
    },
    {
      "skillId": "skill_rogue_backstab",
      "assetPath": "Sprites/Skills/skill_rogue_backstab"
    },
    {
      "skillId": "skill_healer_heal",
      "assetPath": "Sprites/Skills/skill_healer_heal"
    }
  ]
}
```

**Storage**: Cấu trúc folder
```
Assets/
├── Resources/
│   └── Data/
│       ├── skill_icon_map.json (new, single source of truth in Data/Meta)
│       └── Fallback/
│           └── skill_fallback.png (new)
└── Sprites/
    └── Skills/
        ├── skill_warrior_slash.png
        ├── skill_warrior_defensive.png
        ├── skill_mage_fireball.png
        └── ... (các skill icon khác)
```

## 2.3 Tạo Fallback Texture

1. Tạo thư mục: `Assets/Resources/Data/Fallback/`
2. Tạo placeholder texture `skill_fallback.png` (64x64, grey color)
3. Import settings:
   - Sprite Mode: Single
   - Filter Mode: Point
   - Compression: Tight

---

# Part 3: Day 5 — Gacha Scene Implementation

**Mục tiêu**: Tạo Gacha scene layout + controller để roll và hiển thị kết quả.

## 3.1 Tạo GachaUIController.cs

File: `Assets/Scripts/Flow/Gacha/GachaUIController.cs`

Controller đã được tạo sẵn và đã tích hợp logic:
- Chọn banner bằng list bên trái (không dùng dropdown).
- Hiển thị background banner tương ứng bên phải.
- Roll 1/10, pity dùng chung cho mọi banner.
- Dùng chung tiền `gold` (không có currency riêng cho gacha).
- Roll ra character trùng sẽ convert sang `gold` theo rarity.
- Roll 10 hiển thị result tuần tự bằng nút mũi tên phải.

## 3.2 Setup Gacha Scene in Unity

### 3.2.1 Scene Structure

Mở hoặc tạo scene: `Assets/Scenes/GachaScene.unity`

```
GachaScene
├── GachaManager (Empty GameObject)
│   └── (Add: GachaUIController component)
├── GachaTransitionRoot (Empty GameObject)
│   └── (Add: GachaTransitionController component)
└── Canvas (Canvas)
    ├── GachaPanel
    │   ├── TopBar
    │   │   ├── BackButton
    │   │   └── GoldText
    │   ├── BannerListPanel (left)
    │   │   └── BannerListRoot (VerticalLayoutGroup)
    │   │       └── BannerItemPrefab (GachaBannerListItemView)
    │   ├── BannerPreviewPanel (right)
    │   │   └── BannerBackgroundImage
    │   └── RollSection
    │       ├── RollOneButton
    │       ├── RollOneCostText (TextMeshProUGUI)
    │       ├── RollTenButton
    │       └── RollTenCostText (TextMeshProUGUI)
    └── ResultPanel (initially inactive)
        ├── ResultPortrait
        ├── ResultNameText
        ├── ResultRarityText
        ├── ResultRoleIcon
        ├── ResultElementIcon
        ├── RarityStarRoot (GridLayoutGroup)
        │   └── RarityStarPrefab (Image)
        └── ResultCloseButton
    ├── BannerScreen (toàn bộ UI banner trước khi swap)
    ├── ResultScreen (container chứa ResultPanel)
    └── FlashOverlay (Image full-screen, alpha=0, layer cao nhất)
```

### 3.2.2 Setup Components

1. **Create Canvas**:
   - RenderMode: Screen Space - Overlay
   - Canvas Scaler: UI Scale Mode = Scale With Screen Size

2. **Create GachaPanel** (child of Canvas):
   - Anchor: Stretch
   - Layout area cho roll buttons + info display

3. **BannerListPanel** (left):
    - Add `ScrollView` hoặc panel thường.
    - Bên trong có `BannerListRoot` (Vertical Layout Group).
    - Tạo prefab item: `BannerItemPrefab` với component `GachaBannerListItemView`.

4. **BannerPreviewPanel** (right):
    - Add `Image` tên `BannerBackgroundImage` để hiển thị ảnh banner.
    - Ảnh banner load từ `bannerBackgroundPath` trong mỗi pool JSON.

5. **RollOneButton + RollOneCostText**:
   - RollOneButton:
     - Size: 200x60
     - Text: "Roll 1x"
     - Color: Blue
   - RollOneCostText (TextMeshProUGUI gần button):
     - Hiển thị giá roll 1x: `"160"` (hoặc giá tương ứng của pool)
     - Update tự động mỗi khi banner được chọn

6. **RollTenButton + RollTenCostText**:
   - RollTenButton:
     - Size: 200x60
     - Text: "Roll 10x"
     - Color: Green
   - RollTenCostText (TextMeshProUGUI gần button):
     - Hiển thị giá roll 10x: `"1600"` (hoặc giá tương ứng của pool)
     - Update tự động mỗi khi banner được chọn

7. **ResultPanel** (initially inactive):
   - Anchor: Stretch, Size: Full canvas
   - Background: Semi-transparent black
   - Children:
      - **ResultPortrait** (Image, 300x400)
            - **ResultNameText** (TextMeshProUGUI, size 400x80)
            - **ResultRarityText** (TextMeshProUGUI, size 300x60)
            - **ResultRoleIcon** (Image)
            - **ResultElementIcon** (Image)
            - **RarityStarRoot** (RectTransform + GridLayoutGroup)
                - **RarityStarPrefab** (Image prefab 1 sao)
      - **ResultCloseButton** (Button, size 200x60, text "OK")

8. **Transition Overlay**:
     - **BannerScreen**: container UI khi chưa vào result.
     - **ResultScreen**: container UI result để swap screen.
     - **FlashOverlay**: Image full-screen, alpha ban đầu = 0.
    - **SsrSeal**: UI effect object (RectTransform) cho SSR đặc biệt.
    - **SsrSealCanvasGroup**: CanvasGroup của SsrSeal để fade in/out.
    - **ResultCardRoot**: RectTransform root của card result để animate pop-in.

### 3.3 Gán Reference vào GachaUIController

1. Chọn `GachaManager` GameObject
2. Inspector -> `GachaUIController` component
3. Drag UI elements vào các fields:

   **Top Bar:**
   - Back Button <- BackButton
   - Gold Text <- GoldText

   **Banner List (Left):**
   - Banner List Root <- BannerListRoot
   - Banner Item Prefab <- BannerItemPrefab

   **Banner Preview (Right):**
   - Banner Background Image <- BannerBackgroundImage

   **Roll Actions:**
   - Roll One Button <- RollOneButton
   - Roll One Cost Text <- RollOneCostText
   - Roll Ten Button <- RollTenButton
   - Roll Ten Cost Text <- RollTenCostText

   **Roll Animation:**
   - Roll Animation Clip <- (AnimationClip asset từ `Assets/Animations/` - optional, để trống nếu chưa có)
   - Roll Animator <- (Animator component trên GachaPanel nếu có animation)
   - Roll Trigger <- giữ mặc định "Roll"
   - Roll Reveal Delay <- 1.0 (giây đợi trước khi hiện result)

   **Result Panel:**
   - Result Panel <- ResultPanel
   - Result Portrait <- ResultPortrait
   - Result Name Text <- ResultNameText
   - Result Rarity Text <- ResultRarityText
    - Result Role Icon <- ResultRoleIcon
    - Result Element Icon <- ResultElementIcon
    - Result Card Root <- ResultCardRoot
    - Rarity Star Root <- RarityStarRoot
    - Rarity Star Prefab <- RarityStarPrefab
   - Result Close Button <- ResultCloseButton

    **Transition:**
    - Transition Controller <- GachaTransitionRoot (GachaTransitionController)

4. Chọn `GachaTransitionRoot` -> `GachaTransitionController`:
    - bannerScreen <- BannerScreen
    - bannerImage <- BannerImage (RectTransform chính để shake/zoom)
    - resultScreen <- ResultScreen
    - flashImage <- FlashOverlay

    **SSR Special (optional nhưng khuyên dùng):**
    - enableSsrSpecialEffects <- true
    - enableSsrDoubleFlash <- true
    - ssrSealTransform <- SsrSeal (RectTransform)
    - ssrSealCanvasGroup <- CanvasGroup của SsrSeal
    - sfxAudioSource <- AudioSource SFX (UI/Gacha)
    - ssrStingerClip <- Audio clip SSR stinger

### 3.4 DOTween Transition Setup

`GachaTransitionController` đã xử lý:

1. Roll -> rung + zoom banner + flash -> swap sang result.
2. Roll 10 -> click đổi item có flash transition giữa các item.
3. Ở item cuối cùng -> click sẽ tự quay về banner panel.

Chỉnh lực animation theo rarity tại `GachaTransitionController`:
- `ssrPreset` mạnh nhất
- `srPreset` trung bình
- `rPreset` nhẹ nhất

SSR special đã gồm:
- Double flash signature
- SSR seal effect (fade + scale + rotate)
- SSR stinger audio
- Result card entrance mạnh hơn với SSR

Nếu không thấy hiệu ứng, kiểm tra Console các warning:
- `[GachaUI] TransitionController is not assigned...`
- `[GachaTransition] bannerImage is not assigned...`
- `[GachaTransition] flashImage is not assigned...`
- `[GachaTransition] ... ssrSeal ...` hoặc `sfxAudioSource/ssrStingerClip` chưa gán (nếu bật SSR special)

### 3.5 Runtime Notes (quan trọng)

- `GachaUIController` gọi thật `IGachaService` (không mock).
- Cần có `MetaServiceHub`, `SaveManager`, `DataManager` sẵn trong runtime.
- Pity dùng chung mọi banner (`shared pity`).
- Tiền roll dùng chung với `gold` trong save.
- Nếu character trùng: convert sang gold theo rarity (SSR/SR/R).
- Roll 10: click màn hình result để chuyển item tiếp theo; item cuối click sẽ quay về banner.
- Pool data đặt tại `Assets/Data/Gacha/*.json`.

### 3.5 Data mẫu cho nhiều banner

Mỗi pool cần có:

```json
{
    "id": "pool_limited_mage",
    "nameKey": "Limited Mage Banner",
    "bannerBackgroundPath": "Banners/Gacha/banner_limited_mage",
    "pityThreshold": 10,
    "rollCostSingle": 200,
    "rollCostTen": 2000,
    "entries": [ ... ]
}
```

`bannerBackgroundPath` là đường dẫn `Resources.Load<Sprite>` nên ảnh cần nằm trong `Assets/Resources/`.

---

# Test Checklist Day 4.5 + Day 5

## 4.1 Skill Rendering Test (Day 4.5 補充)

**Precondition**: TeamFormation scene setup complete từ Day 4.

**Test Case 1: Skill icons display in QuickInfo**
1. Play từ Boot scene
2. Go to MainMenu -> Team
3. Click Slot 1
4. Picker panel opens
5. Click any character cell
6. **Expected**: QuickInfo panel shows:
   - Character name
   - Role (Tank/Attacker/Support)
   - Element
   - Portrait image
   - **Skill icons + names** ✅ (NEW)
7. Verify icons load (not fallback all)

**Test Case 2: Skill detail panel on click**
1. QuickInfo hiển thị 3-4 skill icons
2. **Click vào 1 skill icon**
3. **Expected**: 
   - SkillDetailPanel appears bên cạnh skill
   - Shows skill name (e.g., "Slash Attack")
   - Shows skill description (e.g., "Deal 150% damage to single enemy")
   - Panel positioned tại điểm click +offset
4. Click skill khác
5. **Expected**: Panel update với skill mới

**Test Case 3: Fallback handling**
1. Remove mapping cho một skill từ skill_icon_map.json
2. Click character với skill đó
3. **Expected**: Fallback grey icon appears + skill name still visible
4. Click skill with fallback icon
5. **Expected**: Detail panel bật ra như bình thường

---

## 4.2 Skill Icon Loader Test (Day 5)

**Test Case 1: JSON config loading**
1. Verify `Assets/Data/Meta/skill_icon_map.json` exists
2. Output log để confirm config loaded:
   ```
   [SkillIconLoader] Loaded skill icon: skill_warrior_slash from Sprites/Skills/skill_warrior_slash
   ```

**Test Case 2: Sprite asset loading**
1. Verify sprite assets exist tại paths từ JSON
2. Example: 
   - Expected path: `Assets/Sprites/Skills/skill_warrior_slash.png`
   - Imported as: Sprite (Single)

**Test Case 3: Fallback texture**
1. Verify fallback exists: `Assets/Resources/Data/Fallback/skill_fallback.png`
2. Trigger fallback:
   - Thêm skill ID không có mapping
   - **Expected**: Grey fallback icon appears

---

## 4.3 Gacha Scene Test (Day 5)

**Test Case 1: Gacha scene navigation**
1. Play từ Boot
2. Go to MainMenu -> Gacha
3. **Expected**: GachaScene loads
4. Click Back -> MainMenu

**Test Case 2: Roll 1x**
1. Click "Roll 1x"
2. **Expected**:
   - Result panel shows
    - Displays portrait
    - Displays rarity (đúng màu)
    - Displays name + role + element
    - Displays extra text (amount / duplicate convert nếu có)
    - Pity text updates theo format `Pity: X/Y`
    - Gold giảm theo cost

**Test Case 3: Roll 10x**
1. Click "Roll 10x"
2. **Expected**:
    - Result panel shows reward #1
    - Click `ResultNextButton` -> reward #2 ... #10
    - Reward cuối cùng: `ResultNextButton` auto hidden
    - Gold giảm theo cost 10x

**Test Case 4: Result confirm**
1. Click "OK" (ResultCloseButton) trong result panel
2. **Expected**:
   - Result panel closes
    - Pity text ngoài panel vẫn giữ state mới nhất
    - Gold text ngoài panel vẫn giữ state mới nhất
   - Ready for next roll

**Test Case 5: Integration with TeamFormation**
1. Roll a character in Gacha
2. Go to Team -> Slot 1 -> Picker
3. **Expected**: Rolled character appears in picker (if unlocked)

---

## 4.4 Full Flow Integration Test

**Sequential flow**:
1. Boot -> Tutorial (or skip) -> MainMenu
2. MainMenu -> Team -> Select 3 characters with skills visible -> Back
3. MainMenu -> Gacha -> Roll -> Show result with character portrait + rarity -> Confirm
4. MainMenu -> Back scene navigation working
5. **Expected**: No crash, no missing assets, skills display properly in QuickInfo

---

# Definition of Done: Day 4.5 + Day 5

- [x] Skill icons render in QuickInfo (Day 4.5)
- [x] **Skill detail panel shows on click** (Day 4.5 補充)
- [x] SkillIconLoader class implemented (Day 5)
- [x] skill_icon_map.json created with sample data (Day 5)
- [x] Fallback texture working (Day 5)
- [x] Gacha scene layout complete (Day 5)
- [x] GachaUIController wired + functional (Day 5)
- [x] Roll 1x / 10x working (Day 5)
- [x] Result panel showing character + rarity + portrait (Day 5)
- [x] Full integration test passed (Day 5)
- [x] No console errors or warnings (critical ones fixed)
- [x] Scene navigation (MainMenu -> Gacha -> Result -> Back) smooth

---

# Known Issues & Troubleshooting

| Issue | Cause | Solution |
|-------|-------|----------|
| Skill icons all fallback | skill_icon_map.json missing or JSON parse error | Verify file path: `Assets/Data/Meta/skill_icon_map.json` + check JSON syntax |
| QuickInfo skills not showing | _quickInfoSkillItemPrefab not assigned | Drag prefab into Inspector field |
| Result character image black | DataManager.LoadCharacterPortrait returns null | Verify portrait sprite paths in DataManager |
| Gacha buttons not responsive | GachaUIController Start() not called | Check GachaManager has component attached + scene is set in Build Settings |
| Back button not working | FlowController.OpenMainMenu() not implemented | Verify IFlowController contract + scene name matches "MainMenuScene" in Build Settings |

---

# Next Steps (Day 6 Preview)

After Day 5 complete:
- [ ] Inventory scene layout + controller (list items, use item)
- [ ] Level Select scene layout + controller (chapter/level grid)
- [ ] Character Collection scene refinements (search/filter/sort)
- [ ] Integration test with Dev A


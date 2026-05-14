# BÁO CÁO MÔN THỰC TẬP CƠ SỞ (TTCS)

**Tên dự án:** TTCS – *Those at The Crossroads of Story*  
**Loại dự án:** Game nhập vai chiến đấu theo lượt (Turn-Based RPG) kết hợp Timing Mechanics  
**Nền tảng:** Unity 2021.3+ | C# | 2D  

---

## MỤC LỤC

1. [Các công nghệ sử dụng](#1-các-công-nghệ-sử-dụng)  
2. [Cơ sở dữ liệu](#2-cơ-sở-dữ-liệu)  
3. [Các chức năng đã cài đặt](#3-các-chức-năng-đã-cài-đặt)  
   - 3.1 [Start Menu & Main Menu (MainMenuController)](#31-start-menu--main-menu-mainmenucontroller)  
   - 3.2 [Level Select – Chọn màn chơi](#32-level-select--chọn-màn-chơi)  
   - 3.3 [Team Formation – Lập đội hình](#33-team-formation--lập-đội-hình)  
   - 3.4 [Gacha – Quay tướng](#34-gacha--quay-tướng)  
   - 3.5 [Inventory – Kho đồ](#35-inventory--kho-đồ)  
   - 3.6 [Character Collection – Quản lý nhân vật](#36-character-collection--quản-lý-nhân-vật)  
   - 3.7 [Chiến đấu (Combat System)](#37-chiến-đấu-combat-system)  
   - 3.8 [Timing System – Canh nhịp](#38-timing-system--canh-nhịp)  
   - 3.9 [Hệ thống Status Effect](#39-hệ-thống-status-effect)  
   - 3.10 [AI Enemy](#310-ai-enemy)  
   - 3.11 [Save/Load](#311-saveload)  
   - 3.12 [Event Bus & Core Services](#312-event-bus--core-services)  

---

## 1. Các công nghệ sử dụng

| Hạng mục | Công nghệ / Thư viện |
|---|---|
| Game Engine | **Unity 2021.3 LTS** |
| Ngôn ngữ lập trình | **C# (.NET Standard 2.1)** |
| Render Pipeline | **Universal Render Pipeline (URP)** |
| UI Framework | **Unity UI (uGUI) + TextMesh Pro** |
| Input | **Unity Input System (New Input System)** |
| Animation | **Unity Animator + Cinemachine** |
| Audio | **Unity Audio Mixer** |
| Sprite / 2D | **Unity 2D Sprite, 2D Animation, 2D IK** |
| Shader | **Unity Shader Graph (URP)** |
| Dữ liệu game | **JSON (tự parse bằng JsonUtility / Newtonsoft)** |
| Lưu trữ | **PlayerPrefs + File System (JSON save file)** |
| Quản lý phụ thuộc | **Singleton Pattern, Event Bus (Publish/Subscribe)** |
| Kiểm tra | **Unity Test Framework** |
| Version Control | **Git + Git LFS** |
| IDE | **Visual Studio 2022 / JetBrains Rider** |

---

## 2. Cơ sở dữ liệu

Dự án không sử dụng cơ sở dữ liệu quan hệ. Toàn bộ dữ liệu game được tổ chức dưới dạng **file JSON** nằm trong thư mục `Assets/Data/`, được load tại runtime bởi `DataManager`.

### Cấu trúc thư mục dữ liệu

```
Assets/Data/
├── Characters/      ← Định nghĩa nhân vật chơi được (char_arden.json, ...)
├── Skills/          ← Định nghĩa kỹ năng (skill_arden_sword_swing.json, ...)
├── Enemies/         ← Định nghĩa quái / boss (enemy_bandit.json, ...)
├── Levels/          ← Cấu hình màn chơi (level_01_01.json, ...)
├── Chapters/        ← Cấu hình chương (chapter_01.json, ...)
├── Stages/          ← Cấu hình stage / wave
├── Items/           ← Định nghĩa vật phẩm (item_potion.json, ...)
├── Gacha/           ← Cấu hình pool gacha (pool_standard.json, ...)
└── Meta/            ← Dữ liệu meta khác
```

### Các model dữ liệu chính

#### Nhân vật (`CharacterDataModel`)

| Trường | Kiểu | Mô tả |
|---|---|---|
| `id` | string | Định danh duy nhất (`char_arden`) |
| `nameKey` | string | Tên hiển thị |
| `metadata.rarity` | string | Độ hiếm: N / R / SR / SSR / UR |
| `metadata.roleTag` | string | Vai trò: Tank / DPS / Support |
| `metadata.element` | string | Nguyên tố: Physical / Fire / ... |
| `baseStats` | object | HP, ATK, DEF, SPD, Crit, CritDmg, Resist |
| `growthCurve` | object | Tăng trưởng chỉ số mỗi level |
| `skills` | string[] | Danh sách ID kỹ năng |
| `passive` | object | Kỹ năng bị động |

**Ví dụ – Arden (Tank SR):** HP 3600, ATK 250, DEF 260, SPD 95, Crit 10%

#### Kỹ năng (`SkillDataModel`)

| Trường | Kiểu | Mô tả |
|---|---|---|
| `id` | string | Định danh kỹ năng |
| `type` | string | `attack` / `heal` / `buff` / `shield` |
| `damage.formula` | string | Công thức tính sát thương |
| `cost.mana` | int | Chi phí mana |
| `cost.cooldown` | int | Cooldown (lượt) |
| `targetRule.type` | string | `single` / `all_enemies` / `self` |
| `effects` | array | Danh sách status effect áp dụng |
| `actionCost.timelineUnits` | int | Điểm timeline tiêu hao |

#### Vật phẩm (`ItemDataModel`)

| Trường | Kiểu | Mô tả |
|---|---|---|
| `id` | string | Định danh item |
| `itemType` | string | `consumable` / `accessory` |
| `effectType` | string | `heal_hp` / `restore_energy` / ... |
| `effectAmount` | int | Lượng hồi phục/buff |
| `rarity` | string | R / SR / SSR |
| Stat bonuses | object | Chỉ số phụ kiện bổ sung (ATK, DEF, MANA,...) |

#### Dữ liệu lưu trữ (`SaveData`)

Được serialize thành JSON và lưu trong `PlayerPrefs` hoặc file. Bao gồm:

| Nhóm | Nội dung |
|---|---|
| Player | `playerLevel`, `gold`, `isNewGame`, `lastSavedTimestamp` |
| Roster | `unlockedCharacters`, `lineup`, `currentParty` |
| Character state | HP, Mana, Level, Exp, Accessory (parallel lists) |
| Stage progress | `clearedStages`, `levelProgress` (stars, bestScore) |
| Inventory | `inventoryItems` (itemId + quantity) |
| Gacha | `gachaPity` (pity counter theo pool) |
| Settings | `bgmVolume`, `sfxVolume` |

> 📎 *Xem sơ đồ cấu trúc SaveData: file `puml/05_save_data.puml`*

---

## 3. Các chức năng đã cài đặt

> 📎 *Xem sơ đồ tổng quan hệ thống: file `puml/00_system_overview.puml`*

---

### 3.1 Start Menu & Main Menu (MainMenuController)

**Scripts:** `MainMenuController.cs`, `TutorialPanelSequenceController.cs`, `SettingsPanelController.cs`

> 📎 *Xem sơ đồ luồng: file `puml/01_main_menu_flow.puml`*

**Kiến trúc:**  
Start Menu và Main Menu được gộp chung trong **một scene duy nhất**, quản lý bởi `MainMenuController`. Controller này điều phối hai **root panel** con:

- `_startMenuRoot` – Panel màn hình khởi động (Start Menu)
- `_mainMenuRoot` – Panel hub điều hướng game (Main Menu)

Khi scene load, hệ thống kiểm tra `SaveManager.HasSaveData()`: nếu **chưa có save** hoặc chưa load save nào vào phiên hiện tại → hiển thị Start Menu; ngược lại → hiển thị thẳng Main Menu.

---

#### Panel 1 – Start Menu

Màn hình đầu tiên người chơi thấy khi mở game. Gồm các nút:

| Nút | Hành vi |
|---|---|
| **Continue** | Load save slot 0 → chuyển sang Main Menu (có hiệu ứng iris-wipe). Bị vô hiệu nếu chưa có file save. |
| **New Game** | Khởi tạo `SaveData` mặc định, ghi vào slot 0 → chuyển sang Main Menu. |
| **Hướng dẫn** | Mở `TutorialPanelSequenceController` – slideshow nhiều trang. |
| **Settings** | Mở `SettingsPanelController` – điều chỉnh âm thanh. |
| **Exit** | Thoát game (`Application.Quit()`). |

**Chuyển cảnh sang Main Menu** dùng `SceneTransitionController.PlayIrisTransition()` – hiệu ứng iris-wipe trước khi ẩn Start Panel và hiện Main Panel.

#### Tutorial Panel (`TutorialPanelSequenceController`)

Slideshow hướng dẫn cách chơi, dạng trình chiếu ảnh:
- Hiển thị `_panelSprites[currentIndex]` và số trang hiện tại (ví dụ "2 / 5").
- Nút **Continue** → slide tiếp (bị vô hiệu ở slide cuối).
- Nút **Back** → slide trước (bị vô hiệu ở slide đầu).
- Nút **Exit** → đóng panel, quay về Start Menu.

#### Settings Panel (`SettingsPanelController`)

Panel cài đặt âm thanh, dùng chung cho cả Start Menu lẫn Main Menu:
- Slider điều chỉnh **Master / SFX / UI / BGM** volume (0–100%).
- Thay đổi slider → `AudioController` áp dụng ngay lập tức.
- Nhấn **Close** → ẩn panel.

---

#### Panel 2 – Main Menu

Hub trung tâm điều hướng sau khi đã load save. Hiển thị **Gold** hiện tại và cung cấp các nút:

| Nút | Điều hướng đến |
|---|---|
| **Play** | Level Select |
| **Team** | Team Formation |
| **Gacha** | Màn hình Gacha |
| **Inventory** | Kho đồ |
| **Character Collection** | Quản lý nhân vật |
| **Settings** | `SettingsPanelController` (cùng panel với Start Menu) |

Mỗi lần vào Main Menu, `EnsureCurrentSave(0)` được gọi để đảm bảo session save luôn hợp lệ.

---

### 3.2 Level Select – Chọn màn chơi

**Scripts:** `LevelSelectUIController.cs`, `LevelSelectController.cs`, `ChapterView.cs`, `LevelView.cs`

**Nghiệp vụ:**  
Cho phép người chơi duyệt và chọn màn chơi được tổ chức theo **Chương (Chapter) → Màn (Level)**:

- Hiển thị danh sách chapter, mỗi chapter chứa nhiều level.
- Mỗi ô level hiển thị: tên level, số sao đạt được (tối đa 3), trạng thái (chưa mở khóa / đã clear / chưa clear).
- Người chơi chọn level → xem trước thông tin: danh sách enemy sẽ xuất hiện, phần thưởng hoàn thành.
- Nhấn **Start** → `FlowController` khởi tạo combat với dữ liệu stage tương ứng.
- Level bị khóa nếu chưa vượt qua level trước.

---

### 3.3 Team Formation – Lập đội hình

**Scripts:** `TeamFormationUIController.cs`, `TeamFormationPickerCellView.cs`, `SkillDetailPanelView.cs`

**Nghiệp vụ:**  
Người chơi lắp ráp đội hình tối đa **3 nhân vật** trước khi vào chiến đấu:

- Giao diện gồm 3 ô slot (Slot 1, 2, 3). Nhấn vào slot → mở **Picker Panel**.
- Picker Panel liệt kê toàn bộ nhân vật đã mở khóa (trừ nhân vật HP = 0 và nhân vật đang ở slot khác).
- Hỗ trợ **lọc** theo Role (Tank/DPS/Support) và **sắp xếp** theo Level hoặc Rarity.
- Chọn nhân vật → panel bên trái hiển thị thông tin nhanh: tên, rarity, role, element, portrait, danh sách kỹ năng.
- Nhấn vào kỹ năng → xem **Skill Detail Panel** (tên, mô tả, công thức sát thương, cooldown, mana cost).
- Nhấn **Confirm** → ghi lineup vào save thông qua `TeamService.SaveLineup()`.

---

### 3.4 Gacha – Quay tướng

**Scripts:** `GachaUIController.cs`, `GachaTransitionController.cs`, `GachaBannerListItemView.cs`

> 📎 *Xem sơ đồ luồng: file `puml/03_gacha_flow.puml`*

**Nghiệp vụ:**  
Hệ thống gacha cho phép người chơi dùng Gold để quay lấy nhân vật và vật phẩm:

- **Danh sách Banner (trái):** Liệt kê các pool gacha hiện có (Standard, Limited...). Chọn banner để xem chi tiết.
- **Roll x1 / Roll x10:** Tiêu tốn Gold (hiển thị chi phí thực). Kiểm tra đủ Gold trước khi cho phép roll.
- **Hệ thống Pity:** Mỗi pool có `pityThreshold` riêng. Khi đạt đủ lần roll mà chưa ra SSR, lần tiếp theo đảm bảo ra SSR.
- **Hiệu ứng chuyển cảnh (`GachaTransitionController`):** Màn hình fade/transition sang màn kết quả với màu sắc tương ứng rarity (UR = đỏ, SSR = vàng, SR = hồng, R = xanh cyan).
- **Màn hình kết quả:** Hiển thị từng phần thưởng tuần tự (tap để xem tiếp). Mỗi kết quả hiển thị portrait nhân vật/icon item, tên, rarity, sao rarity.
- **Lưu kết quả:** Nhân vật/item mới tự động vào `unlockedCharacters` / inventory; Gold bị trừ; pity counter cập nhật.

---

### 3.5 Inventory – Kho đồ

**Scripts:** `InventoryUIController.cs`, `InventoryItemCellView.cs`, `InventoryAccessoryStatLineView.cs`

**Nghiệp vụ:**  
Xem toàn bộ vật phẩm đang sở hữu:

- Danh sách item hiển thị icon, số lượng, độ hiếm (màu viền theo rarity).
- Chọn item → panel chi tiết bên phải: tên, rarity, mô tả, loại hiệu ứng.
- Nếu item là **phụ kiện (accessory)**: hiển thị thêm bảng chỉ số bổ sung (ATK, DEF, MANA, ...).
- Màu nền / viền panel chi tiết thay đổi theo rarity của item được chọn.
- Không dùng item trực tiếp từ Inventory; item tiêu hao chỉ dùng được **trong combat**.

---

### 3.6 Character Collection – Quản lý nhân vật

**Scripts:** `CharacterCollectionUIController.cs`, `AccessoryEquipPickerPanel.cs`

> 📎 *Xem sơ đồ luồng: file `puml/06_character_collection.puml`*


**Nghiệp vụ:**  
Màn hình xem và quản lý toàn bộ nhân vật đã mở khóa:

**Danh sách nhân vật:**
- Lọc theo: Rarity (UR/SSR/SR/R), Role, Element.
- Tìm kiếm theo tên (search field).
- Sắp xếp theo Level tăng/giảm dần.

**Chi tiết nhân vật (khi chọn):**
- Portrait, tên, level, rarity, role, element.
- Chỉ số đầy đủ: HP / Mana / ATK / DEF / SPD / Crit (có tính bonus từ phụ kiện).
- Thanh tiến trình EXP (current / next level).
- Danh sách kỹ năng (nhấn để xem skill detail popup).
- **Slot phụ kiện:** Hiển thị phụ kiện đang trang bị; nhấn để mở `AccessoryEquipPickerPanel` chọn phụ kiện khác hoặc tháo ra.

---

### 3.7 Chiến đấu (Combat System)

**Scripts:** `CombatFlowController.cs`, `TurnManager.cs`, `SkillManager.cs`, `ActionResolver.cs`, `CombatSceneManager.cs`

> 📎 *Xem sơ đồ luồng chiến đấu: file `puml/02_combat_flow.puml`*

**Nghiệp vụ:**  
Hệ thống chiến đấu lượt là cốt lõi của game, vận hành theo **vòng lặp coroutine**:

#### Khởi tạo trận đấu
- `CombatSceneManager` tạo entities (Character + Enemy) từ dữ liệu lineup và stage.
- `CombatFlowController.StartBattle()` đăng ký tất cả entity vào `TurnManager` và `SkillManager`.
- RNG seed được khởi tạo (có thể cố định để debug).

#### Timeline & Thứ tự lượt (TurnManager)
- Dùng hệ thống **CTB (Charge Time Battle)**: mỗi entity có gauge tăng dần theo SPD. Entity đầu tiên đạt 100 được đi.
- Sau mỗi action, gauge bị trừ đi `timelineUnits` tương ứng với kỹ năng sử dụng.
- `TurnOrderDisplay` hiển thị thứ tự lượt sắp tới trên HUD.

#### Lượt người chơi (PlayerTurn)
1. HUD kích hoạt `SkillButtonPanel` – hiển thị các kỹ năng của nhân vật đang hoạt động.
2. Người chơi chọn kỹ năng + mục tiêu → UI gọi `SubmitPlayerAction(skillId, targetIds)`.
3. Nếu là kỹ năng tấn công: mở **Timing Window** để người chơi canh nhịp tăng sát thương.
4. Người chơi có thể dùng **item tiêu hao** từ túi đồ thay cho kỹ năng (gọi `SubmitPlayerItemUse`).

#### Lượt AI (EnemyTurn)
1. `AIController.DecideAction()` phân tích snapshot toàn bộ entity → chọn kỹ năng + mục tiêu.
2. Nếu kỹ năng enemy là tấn công: mở **Guard Timing Window** để người chơi phòng thủ.
3. Action được resolve qua `ActionResolver`.

#### Thực thi action (ExecuteAction)
1. Publish `SkillCastEvent` → kích hoạt animation nhân vật.
2. Chờ **hit frame** (animation notify) → `ActionResolver.Resolve()` tính và áp dụng sát thương/hiệu ứng.
3. Chờ animation kết thúc → chuyển sang EndTurn.

#### Kết thúc trận
- **Thắng:** Toàn bộ enemy chết → kiểm tra wave tiếp theo; nếu hết wave → màn hình kết quả (EXP, item drop, stars).
- **Thua:** Toàn bộ player chết → màn hình thua.
- Dữ liệu HP/Mana sau trận được lưu vào `SaveData`.

#### HUD chiến đấu (`BattleHUD`, `CombatUIController`)
- Thanh HP / Mana cho mỗi nhân vật.
- Floating text sát thương / hồi máu.
- Portrait nhân vật đang cast kỹ năng (`SkillCastPortraitPanel`).
- Thứ tự lượt (`TurnOrderDisplay`).
- Phản hồi Timing (Perfect / Good / Miss – `TimingFeedbackUI`).
- Panel kết quả combat (`ResultCardController`).

---

### 3.8 Timing System – Canh nhịp

**Scripts:** `TimingSystem.cs`, `TimingInputHandler.cs`, `TimingWindow.cs`, `TimingWindowUI.cs`, `TimingFeedbackUI.cs`

> 📎 *Xem sơ đồ chi tiết: file `puml/04_timing_system.puml`*

**Nghiệp vụ:**  
Mechanic độc đáo biến chiến đấu theo lượt thành trải nghiệm tương tác thời gian thực:

#### Hai tình huống kích hoạt Timing Window:

| Tình huống | Mục tiêu | Kết quả |
|---|---|---|
| **Player tấn công** | Nhấn đúng nhịp khi thanh điếm ngược | Perfect/Good → tăng sát thương đánh ra |
| **Enemy tấn công** | Nhấn để **Guard** khi enemy chuẩn bị đánh | Perfect → giảm nhiều sát thương nhận vào; Good → giảm ít; Miss → nhận nguyên |

#### Cơ chế:
- `TimingSystem.OpenWindow(duration, perfectThresholdMs, goodThresholdMs)` bắt đầu đếm ngược.
- `TimingWindowUI` hiển thị thanh progress bar thu hẹp dần.
- `TimingInputHandler` lắng nghe input (Space / tap) → `TimingSystem.RegisterInput(Time.time)`.
- Hệ thống tính `EvaluateInput()`:
  - Nhấn trong khoảng `perfectThresholdMs` (mặc định 500ms) → **Perfect**
  - Nhấn trong khoảng `goodThresholdMs` (mặc định 800ms) → **Good**
  - Không nhấn / nhấn ngoài → **Miss**
- Có **input buffer 60ms**: nhấn trước khi window mở vẫn được tính.
- `TimingFeedbackUI` hiển thị chữ "PERFECT!" / "GOOD!" / "MISS" với hiệu ứng.

---

### 3.9 Hệ thống Status Effect

**Scripts:** `StatusEffect.cs`, `BleedEffect.cs`, `BurnEffect.cs`, `StunEffect.cs`, `ShieldEffect.cs`, `HealEffect.cs`, `CritRateBuffEffect.cs`

**Nghiệp vụ:**  
Các kỹ năng có thể áp dụng **hiệu ứng trạng thái** kéo dài nhiều lượt:

| Status Effect | Mô tả |
|---|---|
| **Burn (Cháy)** | Gây sát thương theo % mỗi đầu lượt |
| **Bleed (Chảy máu)** | Gây sát thương cố định mỗi đầu lượt |
| **Stun (Choáng)** | Bỏ qua lượt của entity bị choáng |
| **Shield (Khiên)** | Hấp thụ một lượng sát thương nhất định trước khi trừ HP |
| **Heal over Time** | Hồi phục HP mỗi đầu lượt |
| **CritRate Buff** | Tăng tỉ lệ Crit trong X lượt |

- Mỗi effect có `duration` (số lượt còn lại) và `stacks` (số lớp chồng chất).
- Tất cả effect được tick tại `OnTurnStart()` của entity.
- Effect hết duration tự xóa khỏi danh sách.

---

### 3.10 AI Enemy

**Scripts:** `AIController.cs`, `AIBehavior.cs`, `TargetSelector.cs`

**Nghiệp vụ:**  
Enemy được điều khiển bởi AI qua hai cơ chế:

**1. AIBehavior Asset (nếu được gán):**
- ScriptableObject định nghĩa bộ rule ưu tiên (priority rules).
- `AIController.DecideAction()` đọc snapshot toàn bộ entity → chọn kỹ năng tốt nhất theo rule.
- `TargetSelector` chọn mục tiêu dựa trên: `highest_threat`, `lowest_hp`, `self`, v.v.

**2. MoveSet Fallback (khi không có AIBehavior):**
- Duyệt tuần tự danh sách `skillIds` trong data enemy.
- Chọn kỹ năng đầu tiên hợp lệ (không cooldown, đủ mana, có target).

---

### 3.11 Save/Load

**Scripts:** `SaveManager.cs`, `SaveData.cs`, `SaveSlot.cs`

> 📎 *Xem sơ đồ cấu trúc dữ liệu: file `puml/05_save_data.puml`*

**Nghiệp vụ:**  
Game sử dụng **1 save slot duy nhất**:

- `SaveManager` là Singleton, quản lý toàn bộ vòng đời save.
- `EnsureCurrentSave(0)` đảm bảo luôn có save hợp lệ khi vào game.
- Dữ liệu được serialize thành JSON và ghi vào `PlayerPrefs` (hoặc file tùy cấu hình).
- Lưu tự động sau mỗi hành động quan trọng: kết thúc combat, gacha, trang bị phụ kiện, thay đổi lineup.
- Dữ liệu lưu bao gồm: tiến trình màn chơi, roster nhân vật, kho đồ, pity gacha, cài đặt âm thanh.

---

### 3.12 Event Bus & Core Services

**Scripts:** `EventBus.cs`, `CombatEvents.cs`, `FlowEvents.cs`, `SystemEvents.cs`, `RNGService.cs`, `DataManager.cs`

**Nghiệp vụ:**  
Hệ thống lõi hỗ trợ toàn bộ game:

**EventBus (Publish/Subscribe):**
- Các hệ thống giao tiếp qua event thay vì gọi trực tiếp nhau → giảm coupling.
- Các event chính: `CombatStartedEvent`, `SkillCastEvent`, `ActionExecutedEvent`, `EntityDiedEvent`, v.v.

**RNGService:**
- Random Number Generator có seed → kết quả tái lập được (dùng cho debug).
- Seed được đặt khi bắt đầu mỗi trận chiến.

**DataManager:**
- Load và cache tất cả JSON data khi khởi động.
- Cung cấp API: `LoadCharacter()`, `LoadSkill()`, `LoadItem()`, `LoadEnemy()`, v.v.
- Cache trong bộ nhớ (Dictionary) để tránh đọc file nhiều lần.

---

*Báo cáo được tổng hợp từ mã nguồn dự án – tháng 5/2026*

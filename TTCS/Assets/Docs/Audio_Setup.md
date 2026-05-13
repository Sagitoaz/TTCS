# Audio Setup

## Mục tiêu

Hệ thống audio hiện tại đã hỗ trợ:

- BGM theo scene
- BGM combat / victory / defeat
- Combat SFX qua `EventBus`
- UI click / hover cho toàn bộ `Button`
- Volume riêng cho `Master`, `SFX`, `UI`, `BGM`
- Lưu volume bằng `PlayerPrefs`

Code chính:

- [AudioController.cs](/d:/Download/TTCS/TTCS/Assets/Scripts/Audio/AudioController.cs)
- [UIButtonSoundRelay.cs](/d:/Download/TTCS/TTCS/Assets/Scripts/Audio/UIButtonSoundRelay.cs)

## 1. Tạo object audio global

Trong scene boot hoặc scene khởi tạo đầu tiên:

1. Tạo `GameObject` tên `AudioController`
2. Gắn component `TTCS.Audio.AudioController`
3. Object này sẽ tự `DontDestroyOnLoad`

## 2. Gắn AudioSource

Trong Inspector của `AudioController`, gắn:

- `_sfxSource`
- `_uiSource`
- `_bgmSource`

Nếu bạn không gắn, script vẫn tự tạo source con runtime. Nhưng nên gắn sẵn để dễ chỉnh mixer / spatial / output.

Thiết lập khuyến nghị:

- `SFXSource`
  - `Play On Awake = false`
  - `Loop = false`
- `UISource`
  - `Play On Awake = false`
  - `Loop = false`
- `BGMSource`
  - `Play On Awake = false`
  - `Loop = true`

## 3. Gắn clip combat

Trong `AudioController`, gắn các clip:

- `_hitSFX`
- `_critHitSFX`
- `_healSFX`
- `_deathSFX`
- `_skillCastSFX`
- `_perfectSFX`
- `_goodSFX`
- `_missSFX`
- `_combatBGM`
- `_victoryBGM`
- `_defeatBGM`

## 4. Gắn clip UI

Gắn các clip sau nếu có:

- `_uiClickSFX`
- `_uiConfirmSFX`
- `_uiCancelSFX`
- `_uiBackSFX`
- `_uiHoverSFX`
- `_uiPopupOpenSFX`
- `_uiPopupCloseSFX`

Fallback hiện tại:

- `Confirm` thiếu clip sẽ dùng `Click`
- `Cancel` thiếu clip sẽ dùng `Click`
- `Back` thiếu clip sẽ ưu tiên `Back`, rồi `Cancel`, rồi `Click`
- `PopupOpen` thiếu clip sẽ dùng `Click`
- `PopupClose` thiếu clip sẽ ưu tiên `PopupClose`, rồi `Cancel`, rồi `Click`

## 5. Gắn BGM theo scene

Trong list `_sceneBgmEntries`, thêm từng scene:

- `sceneName = MainMenuScene`, clip menu
- `sceneName = LevelSelectScene`, clip level select
- `sceneName = TeamFormationScene`, clip team
- `sceneName = GachaScene`, clip gacha
- `sceneName = InventoryScene`, clip inventory
- `sceneName = CharacterCollectionScene`, clip collection
- `sceneName = TutorialScene`, clip tutorial

Lưu ý:

- `CombatScene` không bắt buộc gắn ở đây nếu bạn muốn combat chỉ dùng `_combatBGM`
- Khi combat start, `CombatStartedEvent` sẽ override BGM scene bằng `_combatBGM`
- Khi combat end, `CombatEndedEvent` sẽ phát `_victoryBGM` hoặc `_defeatBGM`
- Khi load scene khác, BGM scene mới sẽ tự thay thế

## 6. UI button sound tự động

`AudioController` sẽ tự quét tất cả `Button` trong scene khi scene load và tự add `UIButtonSoundRelay` nếu button chưa có.

Mặc định:

- click button phát `AudioCueType.Click`
- hover chuột phát `AudioCueType.Hover`

Heuristic tự động theo tên button khi relay được add runtime:

- tên chứa `back` -> `Back`
- tên chứa `cancel` hoặc `close` -> `Cancel`
- tên chứa `confirm`, `fight`, `play`, `retry`, `continue`, `use`, `roll`, `start` -> `Confirm`

Bạn không cần tự gắn relay cho mọi button nếu chỉ cần sound mặc định.

## 7. Override sound cho button đặc biệt

Nếu một button cần cue riêng:

1. Chọn button đó trong scene
2. Add component `UIButtonSoundRelay`
3. Đổi `_clickCue`

Ví dụ:

- nút `Back` dùng `AudioCueType.Back`
- nút `Cancel` dùng `AudioCueType.Cancel`
- nút `Confirm` / `Fight` / `Use` dùng `AudioCueType.Confirm`

Bạn cũng có thể:

- tắt hover bằng `_playHover = false`
- đổi hover cue bằng `_hoverCue`

## 8. Gọi sound từ code khi mở / đóng panel

Nếu panel không mở bằng button click, gọi tay:

```csharp
AudioController.Instance?.PlayUICue(AudioCueType.PopupOpen);
AudioController.Instance?.PlayUICue(AudioCueType.PopupClose);
```

Ví dụ hợp lý:

- mở popup inventory detail
- đóng modal reward
- mở pause panel
- đóng pause panel

## 9. Chỉnh volume từ settings

API có sẵn:

```csharp
AudioController.Instance?.SetMasterVolume(value);
AudioController.Instance?.SetSFXVolume(value);
AudioController.Instance?.SetUIVolume(value);
AudioController.Instance?.SetBGMVolume(value);
```

Đọc volume hiện tại:

```csharp
var master = AudioController.Instance?.GetMasterVolume() ?? 1f;
var sfx = AudioController.Instance?.GetSFXVolume() ?? 1f;
var ui = AudioController.Instance?.GetUIVolume() ?? 1f;
var bgm = AudioController.Instance?.GetBGMVolume() ?? 0.7f;
```

Volume được lưu bằng `PlayerPrefs`.

## 10. Khuyến nghị tổ chức asset

Nên tách thư mục:

- `Assets/Audio/BGM`
- `Assets/Audio/SFX/Combat`
- `Assets/Audio/SFX/UI`

Tên file nên rõ nghĩa:

- `bgm_main_menu`
- `bgm_level_select`
- `bgm_combat`
- `sfx_ui_click`
- `sfx_ui_back`
- `sfx_hit`
- `sfx_skill_cast`

## 11. Những chỗ có thể nối thêm sau

Hiện tại hệ thống này chưa tự phát sound cho:

- popup open/close nếu bạn chỉ `SetActive(true/false)` mà không gọi `PlayUICue`
- slider drag sound trong settings
- toggle / checkbox / tab switch sound riêng
- ambient loop riêng cho map

Nếu cần, có thể mở rộng tiếp bằng:

- `UISliderSoundRelay`
- `UIToggleSoundRelay`
- `Scene ambient source`
- `AudioMixer`

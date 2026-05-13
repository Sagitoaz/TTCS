# Combat Result UI — Setup

`CombatUIController` dùng Result Screen hiện có:

- `_resultButton` là nút **Continue** (win) hoặc **Retry** (lose)
- `_backHomeButton` luôn hiển thị để về **Home**

## Pause Panel

Trong Combat scene, tạo 1 nút Pause + 1 panel overlay (set inactive mặc định) rồi gán vào các field sau trong `CombatUIController`:

- `_pauseButton` → mở/đóng pause panel
- `_pausePanel` → root của pause panel
- `_pauseContinueButton` → đóng pause panel (resume)
- `_pauseRetryButton` → retry combat (đi qua flow)
- `_pauseExitButton` → exit về Level Select

Pause dùng `Time.timeScale = 0` để dừng combat loop.

## Victory Rewards (icon + text)

Để hiện rewards khi thắng (giống Level Select):

- Tạo 1 Horizontal Scroll/Content (RectTransform) trong Result Panel → kéo vào `_victoryRewardContent`
- Kéo prefab `TTCS.Flow.LevelSelect.LevelRewardItemView` vào `_victoryRewardItemViewPrefab`

`CombatUIController` chỉ instantiate item vào content; mọi căn chỉnh layout/spacing/size bạn chỉnh trong Unity Editor.

Nếu không set 2 field trên thì combat vẫn chạy bình thường, chỉ là không hiện danh sách reward.

## Result Team Progress (Level + EXP)

Để sau khi end trận hiển thị level + exp bar cho từng nhân vật trong team:

- Tạo 1 container (RectTransform) trong Result Panel → kéo vào `_resultTeamProgressContent`
- Tạo 1 prefab item có component `CombatResultCharacterProgressView` và kéo vào `_resultTeamProgressItemPrefab`
	- Trong prefab item đó, gán:
		- `_characterSlot` = `TeamFormationPickerCellView` (characterSlot mà bạn đã dùng ở TeamView/Info)
		- `_expSlider` = Slider
		- `_expText` = TMP Text hiển thị dạng `400/800`

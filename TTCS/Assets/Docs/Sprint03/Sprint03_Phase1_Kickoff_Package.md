# Sprint 3 - Gói chốt Giai đoạn 1 (thiết kế và khóa interface)

> **Mục tiêu**: Hoàn tất phần thống nhất bắt buộc của Giai đoạn 1 để 2 dev vào xây dựng ngay.  
> **Ngày chốt**: 2026-03-31

---

## 1) Kết luận nhanh

**Giai đoạn 1 vẫn cần** vì đây là lớp khóa chuẩn chung (contract, schema, route, quy tắc tích hợp).  
Tài liệu này xem như đã thực thi phần “thiết kế và khóa interface” ở mức sprint planning.

---

## 2) Contract đã chốt (áp dụng cho code)

## 2.1 ProgressionService
```csharp
public interface IProgressionService
{
    ChapterState GetChapterState(string chapterId);
    LevelState GetLevelState(string levelId);
    bool CanEnterLevel(string levelId);
    void MarkLevelCompleted(string levelId, int stars, int score);
    UnlockResult TryUnlockNextContent();
}
```

## 2.2 TeamService
```csharp
public interface ITeamService
{
    IReadOnlyList<string> GetCurrentLineup();
    ValidationResult ValidateLineup(IReadOnlyList<string> lineup);
    void SaveLineup(IReadOnlyList<string> lineup);
}
```

## 2.3 GachaService
```csharp
public interface IGachaService
{
    GachaPoolInfo GetPoolInfo(string poolId);
    GachaRollResult Roll(string poolId, int count);
    void ApplyRollResult(GachaRollResult result);
}
```

## 2.4 InventoryService
```csharp
public interface IInventoryService
{
    IReadOnlyList<ItemStack> GetItems();
    bool CanUseItem(string itemId, string targetContext);
    UseItemResult UseItem(string itemId, int quantity, string targetContext);
}
```

## 2.5 FlowController
```csharp
public interface IFlowController
{
    bool TryEnterTutorial();
    void OpenMainMenu();
    void OpenLevelSelect(string chapterId);
    void EnterCombat(string levelId, IReadOnlyList<string> lineupSnapshot);
    void HandleCombatResult(CombatResult result);
}
```

---

## 3) Schema dữ liệu v1 đã chốt

## 3.1 Bắt buộc chung
- Tất cả JSON có `id` và `schemaVersion`.
- Không đổi tên field public nếu chưa có migration.
- Enum lưu bằng string.

## 3.2 Bộ file dữ liệu
- `Assets/Data/Chapters/*.json`
- `Assets/Data/Levels/*.json`
- `Assets/Data/Balance/*.json`
- `Assets/Data/Gacha/*.json`
- `Assets/Data/Items/*.json`
- `Assets/Data/Meta/skill_icon_map.json`

## 3.3 Save meta-state tối thiểu
```json
{
  "schemaVersion": 1,
  "tutorialCompleted": true,
  "unlockedChapters": ["chapter_01"],
  "levelProgress": {
    "level_01_01": { "cleared": true, "stars": 3, "bestScore": 1200 }
  },
  "lineup": ["char_warrior", "char_mage", "char_healer"],
  "inventory": [{ "itemId": "item_potion", "quantity": 5 }],
  "gachaState": { "pityCount": 4 }
}
```

---

## 4) Route scene/menu đã chốt

```text
Boot
 -> (new player && !tutorialCompleted) TutorialScene
 -> MainMenuScene
 -> TeamScene / GachaScene / InventoryScene / LevelSelectScene
 -> CombatScene
 -> ResultScene(or panel)
 -> MainMenuScene or LevelSelectScene
```

Rule:
- New player chạy tutorial đúng 1 lần.
- Returning player vào thẳng main menu.

---

## 5) Quyết định mở đã chốt để không block code

- **Gacha pity**: Có pity cơ bản ở Sprint 3, lưu `pityCount` trong save.
- **Item-use context**: Sprint 3 ưu tiên dùng item ngoài combat; trong combat để Sprint sau.
- **Lineup scope**: Lineup global theo profile (không per-level override trong Sprint 3).
- **Replay tutorial**: Có nút “Học lại” trong settings/menu, không bắt buộc khi login lại.

---

## 6) Bàn giao triển khai ngay cho 2 dev

## Dev A bắt đầu ngay
- Tạo interface/service skeleton cho Progression, Team, Gacha, Inventory.
- Làm migration save theo schema v1 ở trên.
- Hook reward bridge vào inventory/gacha.

## Dev B bắt đầu ngay
- Setup route scene theo luồng đã chốt.
- Làm MainMenu + Team + Gacha + Inventory UI skeleton.
- Hook Tutorial -> MainMenu -> LevelSelect -> Combat.

---

## 7) Tiêu chí “Phase 1 done”

- [x] Contract service đã khóa.
- [x] Schema dữ liệu v1 đã khóa.
- [x] Route scene/menu đã khóa.
- [x] Quyết định mở đã chốt để không block code.
- [x] Handover cụ thể cho Dev A/Dev B.

**Kết luận**: Có thể bắt đầu code ngay theo Sprint 3 WorkPlan.

# Sprint 3 - Biên bản thống nhất chuẩn hóa mã nguồn (2 Dev)

> **Mục đích**: Khóa các chuẩn chung trước khi vào code để giảm conflict và tránh refactor lớn về sau.  
> **Phạm vi**: Core level-based + meta-loop (Team, Gacha, Inventory, Main Menu).  
> **Hiệu lực**: Áp dụng toàn bộ Sprint 3.

---

## 1) Chuẩn namespace và cấu trúc module

## Namespace chuẩn
- `TTCS.Core.*` cho data/save/config/event/utils.
- `TTCS.Meta.*` cho Team, Gacha, Inventory, PlayerProfile.
- `TTCS.Flow.*` cho Tutorial/MainMenu/LevelSelect điều hướng.
- `TTCS.UI.*` cho các lớp hiển thị UI.
- `TTCS.Combat.*` giữ nguyên hệ hiện tại.

## Quy tắc thư mục
- `Assets/Scripts/Core/` -> service nền tảng, schema, save.
- `Assets/Scripts/Meta/` -> domain meta loop.
- `Assets/Scripts/Flow/` -> orchestration scene flow.
- `Assets/Scripts/UI/` -> presenter/view logic.
- Không đặt code runtime mới trong `aidlc-docs/`.

---

## 2) Contract cần khóa chung (bắt buộc)

## 2.1 Progression Contract
- `GetChapterState(chapterId)`
- `GetLevelState(levelId)`
- `CanEnterLevel(levelId)`
- `MarkLevelCompleted(levelId, stars, score)`
- `TryUnlockNextContent()`

## 2.2 Team Contract
- `GetCurrentLineup()`
- `ValidateLineup(lineup)`
- `SaveLineup(lineup)`

## 2.3 Gacha Contract
- `Roll(poolId, count)`
- `ApplyRollResult(result)`
- `GetPoolInfo(poolId)`

## 2.4 Inventory Contract
- `GetItems()`
- `CanUseItem(itemId, targetContext)`
- `UseItem(itemId, quantity, targetContext)`

## 2.5 Flow Contract
- `TryEnterTutorial()`
- `OpenMainMenu()`
- `OpenLevelSelect(chapterId)`
- `EnterCombat(levelId, lineupSnapshot)`
- `HandleCombatResult(result)`

---

## 3) Chuẩn dữ liệu JSON (để mở rộng content không sửa code)

## File dữ liệu tối thiểu
- `Assets/Data/Chapters/*.json`
- `Assets/Data/Levels/*.json`
- `Assets/Data/Balance/*.json`
- `Assets/Data/Gacha/*.json`
- `Assets/Data/Items/*.json`
- `Assets/Data/Meta/skill_icon_map.json`

## Quy tắc schema
- Mọi object phải có `id` duy nhất.
- Trường enum lưu dưới dạng string cố định (không dùng số mơ hồ).
- Version schema: thêm `schemaVersion` ở root.
- Không đổi tên field đã public nếu chưa có migration.

## Quy tắc migration
- Nếu thay đổi schema save: bắt buộc có adapter/migration function.
- Có file dữ liệu mẫu cho case cũ và case mới.

---

## 4) Chuẩn event và logging

## Naming event
- Pattern: `<Domain><Action>Event`  
  Ví dụ: `TutorialCompletedEvent`, `LineupSavedEvent`, `GachaRollResolvedEvent`.

## Event tối thiểu phải có
- Tutorial start/completed/skipped.
- Level selected/entered/completed.
- Reward computed/applied.
- Inventory item used.
- Gacha roll requested/resolved.

## Logging bắt buộc
- Log ở các điểm: tutorial, enter level, combat result, reward apply, save read/write.
- Không log secret/key.
- Mỗi log có: timestamp, context (scene/module), id chính (levelId/chapterId/userSlot).

---

## 5) Chuẩn coding và review

## Coding
- Class/Method/Property: PascalCase.
- Field private: `_camelCase`.
- Không magic number: đưa vào constants/config.
- Mỗi module public phải có XML summary ngắn (hoặc comment 1 dòng rõ ý).

## PR checklist bắt buộc
- [ ] Scope đúng ownership.
- [ ] Pass compile.
- [ ] Có test checklist đính kèm.
- [ ] Không phá flow cũ (combat/gacha/inventory).
- [ ] Có dữ liệu JSON mẫu nếu đụng schema/data.
- [ ] Dev còn lại approve.

---

## 6) Quy tắc làm việc chung để tránh conflict

## Không sửa chéo tùy tiện
- Dev A không sửa UI flow nếu không có ticket.
- Dev B không sửa service domain nếu không có ticket.

## Sync cố định
- Ngày 3: khóa interface.
- Ngày 7: integration v1.
- Ngày 10: full regression.
- Ngày 13: freeze và chỉ fix blocker.

## Quy tắc branch
- 1 nhánh cho 1 mục tiêu rõ.
- Không commit trộn nhiều module không liên quan.
- Rebase/merge main trước khi mở PR.

---

## 7) Danh sách “điểm cần thống nhất” phải chốt trong buổi kickoff

- [ ] Danh sách scene chính và route chuyển scene.
- [ ] Bộ service interface cuối cùng (Progression/Team/Gacha/Inventory/Flow).
- [ ] Schema JSON v1 + quy tắc migration.
- [ ] Quy tắc rarity/pity cơ bản của gacha (nếu có).
- [ ] Định nghĩa lineup hợp lệ (slot, role rule nếu có).
- [ ] Danh sách event chuẩn và nơi publish/subscribe.
- [ ] Test checklist chuẩn cho mỗi PR.

---

## 8) Quyết định mở (cần team xác nhận nếu chưa chốt)

- Có dùng cơ chế pity trong gacha Sprint 3 hay để Sprint 4?
- Inventory item-use có cho dùng ngoài combat không?
- Lineup cố định theo level hay global profile?
- Có cho “chơi lại tutorial” từ menu settings không?

---

## 9) Cam kết áp dụng

- Tài liệu này là chuẩn làm việc chung của 2 dev cho Sprint 3.
- Nếu thay đổi chuẩn, phải cập nhật file này trước rồi mới code theo chuẩn mới.

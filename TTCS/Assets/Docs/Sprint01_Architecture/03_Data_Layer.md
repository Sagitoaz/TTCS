# 03 — Data Layer

Tầng Data chịu trách nhiệm đọc, validate, cache và cung cấp toàn bộ game data. Không có logic game nào ở tầng này.

---

## 1. Sơ đồ quan hệ

```
JSON files (Assets/Data/)
        │
        ▼
  DataManager.LoadAllData()
        │
        ├──► LoadFolder<CharacterDataModel>("Characters") ──► DataValidator.ValidateCharacter()
        │                                                            │
        ├──► LoadFolder<SkillDataModel>("Skills")                    ▼
        │                                              DataCache<T>.Set(id, model)
        ├──► LoadFolder<EnemyDataModel>("Enemies")          │
        │                                                   ▼
        └──► LoadFolder<StageDataModel>("Stages")    In-memory dictionary

Runtime query:
  DataManager.LoadCharacter(id) ──► DataCache<CharacterDataModel>.Get(id)
```

---

## 2. DataManager

**File:** `Scripts/Core/Data/DataManager.cs`
**Namespace:** `TTCS.Core.Data`
**Pattern:** MonoBehaviour Singleton, `DontDestroyOnLoad`

### Trách nhiệm
- Load tất cả JSON files từ `Assets/Data/` trong `Awake()`.
- Validate từng record sau khi deserialize.
- Cache kết quả để tránh re-read disk.
- Cung cấp API tra cứu nhanh theo `id`.
- Publish `DataLoadedEvent` khi hoàn tất.

### Lifecycle
```
Awake()
  └── LoadAllData()
        ├── LoadFolder<CharacterDataModel>("Characters", _characterCache, ValidateCharacter)
        ├── LoadFolder<SkillDataModel>("Skills", _skillCache, ValidateSkill)
        ├── LoadFolder<EnemyDataModel>("Enemies", _enemyCache, ValidateEnemy)
        └── LoadFolder<StageDataModel>("Stages", _stageCache, ValidateStage)
              └── foreach file in folder:
                    ├── File.ReadAllText(path)
                    ├── JsonUtility.FromJson<T>(json)
                    ├── validator(model) == true ?
                    │     ├── YES: cache.Set(model.id, model)
                    │     └── NO: DebugLogger.LogWarning(...)
        └── Publish(DataLoadedEvent)
```

### Public API

| Method | Tham số | Trả về | Mô tả |
|--------|---------|--------|-------|
| `LoadAllData()` | — | void | Load toàn bộ data, gọi tự động trong Awake |
| `LoadCharacter(id)` | `string id` | `CharacterDataModel` | Tra cứu character theo id |
| `LoadSkill(id)` | `string id` | `SkillDataModel` | Tra cứu skill theo id |
| `LoadEnemy(id)` | `string id` | `EnemyDataModel` | Tra cứu enemy theo id |
| `LoadStage(id)` | `string id` | `StageDataModel` | Tra cứu stage theo id |
| `GetAllCharacters()` | — | `IReadOnlyCollection<CharacterDataModel>` | Lấy tất cả characters |
| `GetAllSkills()` | — | `IReadOnlyCollection<SkillDataModel>` | Lấy tất cả skills |
| `GetAllEnemies()` | — | `IReadOnlyCollection<EnemyDataModel>` | Lấy tất cả enemies |
| `GetAllStages()` | — | `IReadOnlyCollection<StageDataModel>` | Lấy tất cả stages |

### Properties

| Property | Type | Mô tả |
|----------|------|-------|
| `Instance` | `DataManager` | Singleton access |
| `IsLoaded` | `bool` | True sau khi LoadAllData() hoàn thành |

---

## 3. DataCache\<T\>

**File:** `Scripts/Core/Data/DataCache.cs`
**Namespace:** `TTCS.Core.Data`
**Pattern:** Generic class (không phải MonoBehaviour)

### Trách nhiệm
Lưu trữ in-memory các object đã load, tránh đọc file nhiều lần.

### API

| Method | Mô tả |
|--------|-------|
| `Set(id, data)` | Lưu vào cache, ghi đè nếu đã có |
| `Get(id)` | Lấy ra, null nếu không có |
| `Has(id)` | Kiểm tra key tồn tại |
| `Clear()` | Xóa toàn bộ |
| `GetAll()` | Trả về tất cả values |
| `Count` | Số lượng entries |

---

## 4. DataValidator

**File:** `Scripts/Core/Data/DataValidator.cs`
**Pattern:** Static class

### Trách nhiệm
Kiểm tra data model sau khi deserialize JSON. Log warning và trả về `false` nếu data thiếu field bắt buộc. Không throw exception.

### Validation rules

#### `ValidateCharacter(CharacterDataModel)`
| Field | Điều kiện |
|-------|-----------|
| `id` | Không rỗng |
| `nameKey` | Không rỗng |
| `baseStats.hp` | > 0 |
| `baseStats.atk` | > 0 |

#### `ValidateSkill(SkillDataModel)`
| Field | Điều kiện |
|-------|-----------|
| `id` | Không rỗng |
| `nameKey` | Không rỗng |
| `type` | Không rỗng ("attack", "heal", "buff", "debuff") |

#### `ValidateEnemy(EnemyDataModel)`
| Field | Điều kiện |
|-------|-----------|
| `id` | Không rỗng |
| `nameKey` | Không rỗng |
| `baseStats.hp` | > 0 |

#### `ValidateStage(StageDataModel)`
| Field | Điều kiện |
|-------|-----------|
| `id` | Không rỗng |
| `encounters` | Không null/rỗng |

---

## 5. Data Models (JSON Schema)

### 5.1 CharacterDataModel

```json
{
  "id":       "char_warrior",
  "nameKey":  "Warrior",
  "baseStats": {
    "hp": 3000, "atk": 280, "def": 180, "spd": 120,
    "crit": 0.05, "resist": 0.0
  },
  "growthCurve": { "hpPerLevel": 100, "atkPerLevel": 10, "defPerLevel": 5, "spdPerLevel": 1 },
  "skills": ["skill_warrior_slash", "skill_warrior_shield"],
  "passive": { "id": "passive_warrior", "nameKey": "Fortress" },
  "visual": { "spritePath": "Characters/warrior" }
}
```

**C# class `CharacterDataModel`:**

| Field | Type | Mô tả |
|-------|------|-------|
| `id` | `string` | Unique identifier |
| `nameKey` | `string` | Tên hiển thị |
| `baseStats` | `CharacterBaseStats` | HP, ATK, DEF, SPD, Crit, Resist |
| `growthCurve` | `CharacterGrowthCurve` | Tăng trưởng theo level |
| `skills` | `string[]` | Danh sách skill ID |
| `passive` | `CharacterPassive` | Passive skill |
| `visual` | `CharacterVisual` | Asset paths |
| `aiHints` | `CharacterAIHints` | Gợi ý cho AI auto |

---

### 5.2 SkillDataModel

```json
{
  "id": "skill_warrior_slash",
  "nameKey": "Slash",
  "type": "attack",
  "targetRule": { "type": "single_enemy", "count": 1 },
  "cost": { "mana": 0, "cooldown": 0, "limitPerFight": -1 },
  "damage": { "formula": "1.0", "element": "Physical" },
  "effects": [],
  "timing": { "windowDuration": 1.5, "perfectThresholdMs": 500, "goodThresholdMs": 800 },
  "actionCost": { "timelineUnits": 100 }
}
```

**C# class `SkillDataModel` — các field quan trọng:**

| Field | Type | Mô tả |
|-------|------|-------|
| `id` | `string` | Unique identifier |
| `type` | `string` | "attack" / "heal" / "buff" / "debuff" |
| `targetRule.type` | `string` | "single_enemy", "all_enemies", "single_ally", "self", ... |
| `targetRule.count` | `int` | Số target tối đa |
| `cost.mana` | `int` | Mana tiêu thụ |
| `cost.cooldown` | `int` | Cooldown sau dùng (số lượt) |
| `cost.limitPerFight` | `int` | -1 = không giới hạn |
| `damage.formula` | `string` | Multiplier (ví dụ: "1.2" → ATK×1.2) |
| `damage.element` | `string` | Fire / Ice / Physical / ... |
| `effects[]` | `SkillEffect[]` | Danh sách status effect kèm theo |
| `actionCost.timelineUnits` | `int` | Chi phí timeline (100 = Normal) |

---

### 5.3 EnemyDataModel

**C# class — các field quan trọng:**

| Field | Type | Mô tả |
|-------|------|-------|
| `id` | `string` | Unique identifier |
| `nameKey` | `string` | Tên hiển thị |
| `baseStats` | `EnemyBaseStats` | HP, ATK, DEF, SPD, Crit, Resist |
| `moveSet` | `EnemyMove[]` | Danh sách move (có skillId kèm theo) |
| `phases` | `EnemyPhase[]` | Phase thay đổi khi HP thấp |
| `rewards` | `EnemyRewards` | Gold, XP sau khi bị tiêu diệt |

---

### 5.4 StageDataModel

**C# class — các field quan trọng:**

| Field | Type | Mô tả |
|-------|------|-------|
| `id` | `string` | Unique identifier |
| `encounters` | `StageEncounter[]` | Danh sách wave |
| `encounters[i].enemies` | `StageEnemy[]` | Enemy trong wave i |
| `requirements` | `StageRequirements` | Điều kiện mở stage |
| `rewards` | `StageRewards` | Phần thưởng clear |

---

## 6. SaveManager

**File:** `Scripts/Core/Save/SaveManager.cs`
**Namespace:** `TTCS.Core.Save`
**Pattern:** MonoBehaviour Singleton, `DontDestroyOnLoad`

### Trách nhiệm
- Tạo game mới (`NewGame`) — khởi tạo `SaveData` mặc định trong memory.
- Serialize `SaveData` thành JSON và ghi file (`Save(slot)`).
- Đọc file JSON và deserialize thành `SaveData` (`Load(slot)`).
- Hỗ trợ 3 save slots (index 0–2).
- Publish `GameSavedEvent` / `GameLoadedEvent` sau mỗi thao tác.

### Save file location
```
Application.persistentDataPath/ttcs_save_slot_0.json
Application.persistentDataPath/ttcs_save_slot_1.json
Application.persistentDataPath/ttcs_save_slot_2.json
```

### Public API

| Method | Tham số | Trả về | Mô tả |
|--------|---------|--------|-------|
| `NewGame()` | — | void | Tạo SaveData mặc định, không ghi file |
| `Save(slotIndex)` | `int` (0–2) | void | Ghi CurrentSave ra file JSON |
| `Load(slotIndex)` | `int` (0–2) | `SaveData` | Đọc file, set CurrentSave |
| `DeleteSave(slotIndex)` | `int` | void | Xóa file slot |
| `HasSave(slotIndex)` | `int` | `bool` | Kiểm tra file tồn tại |
| `GetSlotMetadata(slotIndex)` | `int` | `SaveSlot` | Đọc metadata nhẹ |

### Properties

| Property | Type | Mô tả |
|----------|------|-------|
| `CurrentSave` | `SaveData` | SaveData đang active trong memory |
| `ActiveSlotIndex` | `int` | Slot đang load (-1 nếu NewGame) |

### SaveData structure

```csharp
public class SaveData
{
    public string lastSavedTimestamp;
    public bool   isNewGame;
    // Profile
    public string playerName;
    public int    totalPlayTime;
    // Progression (mở rộng Sprint 3)
    public List<string> unlockedCharacterIds;
    public int          gold;
    public int          totalBattlesWon;
}
```

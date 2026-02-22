# 📂 Data Directory - TTCS

> Thư mục chứa tất cả game data definitions dưới dạng JSON
> **Chỉnh sửa data này để modify characters, skills, enemies và stages**

---

## 📋 Folder Structure

```
Data/
├── Characters/         # Playable character definitions
│   ├── char_warrior.json
│   └── char_mage.json
├── Skills/            # Skill definitions
│   ├── skill_warrior_slash.json
│   └── skill_mage_fireball.json
├── Enemies/           # Enemy definitions
│   ├── enemy_goblin.json
│   └── enemy_dark_knight.json
└── Stages/            # Stage/Battle definitions
    └── stage_01_tutorial.json
```

---

## 🚀 Quick Start

### Tạo Character mới

1. **Copy template:**
   ```
   Duplicate: char_warrior.json → char_rogue.json
   ```

2. **Edit values:**
   - Change `id` to `"char_rogue"`
   - Update `nameKey`, stats, skills
   - Save file

3. **Reference in code:**
   ```csharp
   var data = DataManager.Instance.LoadCharacter("char_rogue");
   ```

### Tạo Skill mới

1. **Copy template:**
   ```
   Duplicate: skill_warrior_slash.json → skill_rogue_backstab.json
   ```

2. **Customize:**
   - Update damage formula
   - Add special effects
   - Set cooldown/cost

3. **Link to character:**
   ```json
   "skills": [
     "skill_rogue_backstab",
     ...
   ]
   ```

---

## 📊 Existing Data Files

### Characters (2)

| ID | Name | Role | Rarity | HP | ATK | DEF | SPD |
|----|------|------|--------|-----|-----|-----|-----|
| `char_warrior` | Warrior | Tank | SR | 3000 | 280 | 180 | 120 |
| `char_mage` | Mage | Support | SSR | 2000 | 400 | 100 | 140 |

### Skills (2 base skills)

| ID | Character | Type | Cooldown | Effect |
|----|-----------|------|----------|--------|
| `skill_warrior_slash` | Warrior | Attack | 0 | Physical damage |
| `skill_mage_fireball` | Mage | Attack | 2 | Fire damage + Burn |

### Enemies (2)

| ID | Type | HP | ATK | Special |
|----|------|-----|-----|---------|
| `enemy_goblin` | Common | 1500 | 180 | Flees at low HP |
| `enemy_dark_knight` | Elite | 5000 | 320 | Counter + Guard |

### Stages (1)

| ID | Name | Waves | Enemies | Difficulty |
|----|------|-------|---------|------------|
| `stage_01_tutorial` | Tutorial | 2 | 2 Goblins → 1 Knight | Easy |

---

## ✏️ Editing Guidelines

### 1. Maintain Valid JSON

**Always check syntax:**
- Use [JSONLint](https://jsonlint.com/) to validate
- VS Code with JSON extension shows errors
- Test load in Unity after editing

### 2. Use Unique IDs

**Naming convention:**
```
Characters:  char_<name>
Skills:      skill_<char>_<skillname>
Enemies:     enemy_<type>
Stages:      stage_<chapter>_<name>
```

**Examples:**
```json
"id": "char_rogue"          ✅
"id": "character001"        ❌ Not descriptive
"id": "CHAR_ROGUE"          ❌ Should be lowercase
```

### 3. Reference Existing IDs

**When linking skills to characters:**
```json
"skills": [
  "skill_warrior_slash"      ✅ Exists
  "skill_unknown_spell"      ❌ Will cause error
]
```

**System will log warning if ID not found**

### 4. Stat Ranges

**Recommended values (Level 1):**
```
HP:      1000 - 5000
ATK:     100 - 500
DEF:     50 - 300
SPD:     80 - 160
Crit:    0.0 - 0.5  (0% - 50%)
Resist:  0.0 - 0.5  (0% - 50%)
```

**Avoid:**
- Negative stats (except in modifiers)
- HP = 0 (use 1 minimum)
- Crit > 1.0 (100% is max)

---

## 🔧 Data Loading System

### How it works:

1. **On game start:**
   ```
   DataManager.Instance.LoadAllData()
   ```

2. **Loads all JSON:**
   ```
   Characters/*.json → CharacterData objects
   Skills/*.json     → SkillData objects
   Enemies/*.json    → EnemyData objects
   Stages/*.json     → StageData objects
   ```

3. **Cached in memory:**
   ```csharp
   var warrior = DataManager.Instance.GetCharacter("char_warrior");
   ```

### Error handling:

```
✓ Valid JSON → Loaded successfully
✗ Invalid JSON → Error logged, file skipped
✗ Missing reference → Warning logged, null returned
```

---

## 🧪 Testing Your Data

### Step 1: Validate JSON

**Online:**
- [JSONLint](https://jsonlint.com/)
- Copy-paste your JSON
- Fix any syntax errors

**VS Code:**
- Open JSON file
- Check bottom-right for errors
- Red underline = syntax error

### Step 2: Load in Unity

**Method 1 - Play Mode:**
```
1. Start game
2. Check Console for loading errors
3. Look for: "Loaded character: char_warrior"
```

**Method 2 - Debug Panel:**
```
1. Open Debug Panel (if implemented)
2. Click "Reload Data"
3. Verify no errors
```

### Step 3: Use in Combat

```
1. Enter test battle
2. Spawn character using your data
3. Test skills work correctly
4. Check stats display properly
```

---

## 📖 Documentation References

**See full specs:**
- [DataTemplates.md](../Docs/DataTemplates.md) - Detailed field explanations
- [TechnicalDescription.md](../Docs/TechnicalDescription.md) - Section 3: Data Models

**Code reference:**
```
Assets/Scripts/Data/
├── CharacterData.cs
├── SkillData.cs
├── EnemyData.cs
└── StageData.cs
```

---

## 🎯 Common Tasks

### Add new playable character

1. Copy `char_warrior.json`
2. Rename to `char_<name>.json`
3. Change all IDs
4. Create 3-4 skills for them
5. Test load

### Create boss enemy

1. Copy `enemy_dark_knight.json`
2. Set higher HP/stats
3. Add phase transitions
4. Configure loot table
5. Test in stage

### Design new stage

1. Copy `stage_01_tutorial.json`
2. Change chapter/order
3. Configure waves and enemies
4. Set rewards
5. Test playthrough

---

## ⚠️ Important Notes

### DO:
- ✅ Backup before major changes
- ✅ Test after every edit
- ✅ Keep IDs lowercase with underscores
- ✅ Document custom formulas
- ✅ Balance test with other content

### DON'T:
- ❌ Delete required fields
- ❌ Use duplicate IDs
- ❌ Reference non-existent IDs
- ❌ Set stats to impossible values
- ❌ Forget to save file

---

## 🔄 Version Control

**These files should be in Git:**
```bash
git add Assets/Data/**/*.json
git commit -m "data: add new character Rogue"
git push
```

**Why JSON in version control?**
- ✅ Text format - easy to diff
- ✅ Merge conflicts visible
- ✅ Track balance changes over time
- ✅ Revert if needed

---

## 🛠️ Tools & Utilities

### JSON Formatter (VS Code)

**Format document:**
```
Shift + Alt + F
```

**Settings:**
```json
{
  "editor.formatOnSave": true,
  "json.format.enable": true
}
```

### Batch Operations

**Find & replace IDs:**
```
Ctrl + Shift + H → Find in folder
```

**Validate all JSON:**
```bash
# Future utility script
python validate_all_json.py
```

---

## 📊 Data Statistics

**Current counts:**
- Characters: 2
- Skills: 2 (base)
- Enemies: 2
- Stages: 1

**Sprint 1 Goals:**
- Characters: 2 ✅
- Skills: 6+ (3 per char)
- Enemies: 2 ✅
- Stages: 1 ✅

---

## 🆘 Troubleshooting

### "Failed to load character data"

**Check:**
1. JSON syntax valid?
2. File in correct folder?
3. ID matches filename?

### "Skill ID not found"

**Fix:**
1. Verify skill JSON exists
2. Check ID spelling matches
3. Reload data in Unity

### "Invalid stat value"

**Solution:**
1. Check stat ranges
2. No negative HP/ATK/DEF/SPD
3. Crit/Resist between 0-1

---

**Happy editing! 🎮**

*For questions, see [DataTemplates.md](../Docs/DataTemplates.md) or ask the team.*

---

*Last Updated: February 22, 2026*

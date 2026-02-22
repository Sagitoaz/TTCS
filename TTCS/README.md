# 🎮 TTCS - Those at The Crossroads of Story

> Turn-Based Combat RPG với Timing Mechanics  
> **Unity 2021.3+ | C# | 2D Game**

---

## 🚀 QUICK START

### 📖 BẮT ĐẦU TỪ ĐÂY

1. **[Assets/Docs/SUMMARY.md](Assets/Docs/SUMMARY.md)** ← 📋 **ĐỌC ĐẦU TIÊN**
   - Tổng quan toàn bộ tài liệu
   - Cấu trúc mới: Common/ và Sprint01/

2. **[Assets/Docs/Sprint01/QuickSetupGuide.md](Assets/Docs/Sprint01/QuickSetupGuide.md)** ← 🛠️ **SETUP**
   - Hướng dẫn setup môi trường  
   - Workflow hàng ngày

3. **[Assets/Docs/Sprint01/FOLDER_OWNERSHIP.md](Assets/Docs/Sprint01/FOLDER_OWNERSHIP.md)** ← 📂 **AI LÀM GÌ**
   - Tránh conflict khi làm việc song song

4. **[Assets/Docs/Sprint01/WorkPlan_2Weeks.md](Assets/Docs/Sprint01/WorkPlan_2Weeks.md)** ← ⭐ **KẾ HOẠCH**
   - Chi tiết công việc 10 ngày

---

## 📂 Documentation Structure (MỚI!)

```
Assets/Docs/
├── SUMMARY.md                   ← Tổng quan tài liệu
│
├── Common/                      📚 Tài liệu chung (dùng xuyên suốt)
│   ├── projectdescription.md       - Mô tả dự án
│   ├── TechnicalDescription.md     - Kiến trúc chi tiết
│   ├── plot.md                     - Cốt truyện
│   ├── CodeConventions.md          - Quy chuẩn code
│   └── TestingChecklist.md         - Hướng dẫn testing
│
├── Sprint01/                    📅 Tài liệu Sprint 1 (tuần 1-2)
│   ├── WorkPlan_2Weeks.md          - Kế hoạch 10 ngày
│   ├── FOLDER_OWNERSHIP.md         - Phân chia file/folder
│   ├── QuickSetupGuide.md          - Setup & workflow
│   └── DataTemplates.md            - JSON templates
│
├── Sprint02/                    📅 Dự phòng tuần 3-4
└── Sprint03/                    📅 Dự phòng tuần 5-6
```

**Lợi ích:**
- ✅ Dễ tìm tài liệu chung vs specific sprint
- ✅ Dễ thêm tài liệu các tuần sau
- ✅ Tách biệt documentation lifecycle

---

## 👥 Team Structure

### 🔵 Developer A - Core Systems
**Owns:**
- Event Bus & RNG Service
- Data loading & Save/Load system
- Turn Manager & Skill Manager
- AI Controller
- Debug tools & logging

**Folders:**
- `Scripts/Core/Events/`
- `Scripts/Core/Data/`
- `Scripts/Core/Save/`
- `Scripts/Core/Utilities/` (RNG)
- `Scripts/Combat/Managers/` (Turn, Skill)
- `Scripts/Combat/AI/`
- `Scripts/Debug/`

---

### 🟢 Developer B - Combat & Gameplay
**Owns:**
- Combat entities & components
- Stats system & damage calculations
- Action pipeline
- Status effects system
- ScriptableObject definitions
- Game content (JSON data)

**Folders:**
- `Scripts/Combat/Entities/`
- `Scripts/Combat/Components/`
- `Scripts/Combat/Stats/`
- `Scripts/Combat/Actions/`
- `Scripts/Combat/Effects/`
- `Scripts/Data/`
- `Data/` (Characters, Skills, Enemies, Stages)

---

## 💻 Code Base (Đã tạo sẵn!)

### Core Systems
- ✅ **Constants.cs** - Tất cả game constants ([xem code](Assets/Scripts/Core/Constants.cs))
- ✅ **EventBus** - Event system với Subscribe/Publish ([xem code](Assets/Scripts/Core/Events/))
- ✅ **RNGService** - Seeded random generator ([xem code](Assets/Scripts/Core/Utilities/RNGService.cs))
- ✅ **GameUtils** - Helper utilities ([xem code](Assets/Scripts/Core/Utilities/GameUtils.cs))
- ✅ **DebugLogger** - Categorized logging ([xem code](Assets/Scripts/Debug/DebugLogger.cs))

### Combat Base
- ✅ **IEntityComponent** - Component interface ([xem code](Assets/Scripts/Combat/Components/))
- ✅ **EntityStats** - Stats container ([xem code](Assets/Scripts/Combat/Stats/EntityStats.cs))
- ✅ **CombatEvents** - All combat events ([xem code](Assets/Scripts/Core/Events/CombatEvents.cs))

### Utilities
- ✅ **Singleton<T>** - Generic singleton pattern ([xem code](Assets/Scripts/Utils/Singleton.cs))

---

## 📂 Project Structure
│   ├── Data/                    📊 JSON game data
│   │   ├── Characters/         [Sample data ready]
│   │   ├── Skills/
│   │   ├── Enemies/
│   │   └── Stages/
│   │
│   └── Tests/                   🧪 Unit tests
│
└── README.md                    👈 BẠN ĐANG Ở ĐÂY
```

---

## 🎯 Sprint 1 Goals (2 tuần)

### Week 1: Foundation
- ✅ Event Bus & RNG Service
- ✅ Data loading & Save system
- ✅ Combat entities & stats
- ✅ Turn manager & action pipeline

### Week 2: Integration
- ✅ Skills & status effects
- ✅ AI decision making
- ✅ Combat flow complete
- ✅ **1 battle playable end-to-end**

**Demo:** End of Day 10 (Mar 7)

---

## 📋 Daily Workflow

### Morning (9:00 AM)
```bash
# 1. Pull latest
git checkout main
git pull

# 2. Daily standup (15 min)
- Hôm qua làm gì?
- Hôm nay sẽ làm gì?
- Có blockers không?

# 3. Check WorkPlan for today's tasks
# 4. Check FOLDER_OWNERSHIP for your files
# 5. Start coding!
```

### During Work
```
✅ Work in YOUR owned folders
✅ Commit every 1-2 hours
✅ Test frequently
⚠️ Touch shared files? → Announce first!
```

### Evening (5:00 PM)
```bash
# 1. Commit & push
git add .
git commit -m "type: description"
git push

# 2. End-of-day sync (10 min)
# 3. Plan tomorrow
```

---

## 🚨 Important Rules

### 🔴 ALWAYS DO
- ✅ Check FOLDER_OWNERSHIP before editing ANY file
- ✅ Work in your designated folders
- ✅ Pull from main every morning
- ✅ Communicate before touching shared files
- ✅ Commit frequently (small commits)
- ✅ Run tests before pushing

### 🔴 NEVER DO
- ❌ Edit teammate's owned files without asking
- ❌ Work on same scene file simultaneously
- ❌ Commit directly to main (use feature branches)
- ❌ Push without testing
- ❌ Code in silence for days

---

## 💬 Communication

### Channels
- **Slack/Discord:** Daily sync, quick questions
- **GitHub PRs:** Code review
- **Pair programming:** Integration points (Day 5, 8, 10)

### Templates

**Before editing shared file:**
```
"@teammate I need to edit Constants.cs - OK now?"
```

**After merging to main:**
```
"Merged feature/event-bus to main. Please pull!"
```

**When stuck:**
```
"Stuck on [X] for >1 hour. Can we pair to debug?"
```

---

## 🆘 Need Help?

1. **Check documentation:**
   - [SUMMARY.md](Assets/Docs/SUMMARY.md) - Overview
   - [Sprint01/FOLDER_OWNERSHIP.md](Assets/Docs/Sprint01/FOLDER_OWNERSHIP.md) - File ownership
   - [Sprint01/WorkPlan_2Weeks.md](Assets/Docs/Sprint01/WorkPlan_2Weeks.md) - Daily tasks
   - [Common/CodeConventions.md](Assets/Docs/Common/CodeConventions.md) - Code style
   - [Common/TechnicalDescription.md](Assets/Docs/Common/TechnicalDescription.md) - Architecture

2. **Ask teammate** - Better than being stuck!

3. **External resources:**
   - Unity docs
   - C# reference
   - Stack Overflow (last resort)

---

## 🔧 Tools Required

- **Unity:** 2021.3+ LTS
- **IDE:** Visual Studio 2022 / JetBrains Rider
- **Git:** Latest version + GitLFS
- **OS:** Windows 10/11 (project tested on Windows)

---

## 📚 Documentation Index

| File | Purpose | Priority |
|------|---------|----------|
| [SUMMARY.md](Assets/Docs/SUMMARY.md) | Overview of all docs | ⭐⭐⭐⭐⭐ |
| **Sprint01/** | | |
| [FOLDER_OWNERSHIP.md](Assets/Docs/Sprint01/FOLDER_OWNERSHIP.md) | File ownership | ⭐⭐⭐⭐⭐ |
| [WorkPlan_2Weeks.md](Assets/Docs/Sprint01/WorkPlan_2Weeks.md) | 10-day tasks | ⭐⭐⭐⭐⭐ |
| [QuickSetupGuide.md](Assets/Docs/Sprint01/QuickSetupGuide.md) | Setup & workflow | ⭐⭐⭐⭐⭐ |
| [DataTemplates.md](Assets/Docs/Sprint01/DataTemplates.md) | Data format | ⭐⭐⭐ |
| **Common/** | | |
| [TechnicalDescription.md](Assets/Docs/Common/TechnicalDescription.md) | Architecture | ⭐⭐⭐⭐ |
| [CodeConventions.md](Assets/Docs/Common/CodeConventions.md) | Code style | ⭐⭐⭐⭐ |
| [TestingChecklist.md](Assets/Docs/Common/TestingChecklist.md) | Testing guide | ⭐⭐⭐ |
| [projectdescription.md](Assets/Docs/Common/projectdescription.md) | Project overview | ⭐⭐ |
| [plot.md](Assets/Docs/Common/plot.md) | Story & lore | ⭐ |

---

## 🎉 Ready to Start?

**Next step:** Open [Assets/Docs/SUMMARY.md](Assets/Docs/SUMMARY.md)

**Remember:**
- 📂 Check ownership before editing
- 💬 Communicate early and often
- 🧪 Test as you code
- 🤝 Pair when stuck

---

**Good luck! Let's build something awesome! 🚀**

---

*README v1.0 - Created February 22, 2026*  
*Sprint 1: Feb 24 - Mar 7, 2026*

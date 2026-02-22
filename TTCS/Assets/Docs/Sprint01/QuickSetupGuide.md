# 🚀 Quick Setup Guide - TTCS Sprint 1

> Hướng dẫn setup nhanh cho developers bắt đầu Sprint 1
> **Đọc file này TRƯỚC KHI BẮT ĐẦU coding**

---

## ✅ Pre-Sprint Checklist

### 📚 Đọc tài liệu (1-2 giờ)

- [ ] **projectdescription.md** - Hiểu game concept và vision
- [ ] **TechnicalDescription.md** - Nắm architecture và design
- [ ] **WorkPlan_2Weeks.md** - Xem task assignments
- [ ] **CodeConventions.md** - Học coding standards
- [ ] **DataTemplates.md** - Reference cho data structures

### 🛠️ Setup môi trường (30 phút)

- [ ] **Unity** 2021.3+ LTS installed
- [ ] **IDE** (Visual Studio / Rider) configured
- [ ] **Git** setup with GitLFS
- [ ] **Repo** cloned và đồng bộ
- [ ] **Project** mở được trong Unity
- [ ] **No errors** trong Console khi mở project

---

## 📁 Project Structure Overview

```
TTCS/
├── Assets/
│   ├── Docs/                    # 📖 Documentation
│   │   ├── WorkPlan_2Weeks.md  # ← KẾ HOẠCH CHÍNH
│   │   ├── CodeConventions.md  # ← QUY CHUẨN CODE
│   │   ├── DataTemplates.md    # ← TEMPLATE DATA
│   │   └── TestingChecklist.md # ← TESTING GUIDE
│   │
│   ├── Data/                    # 📊 Game data (JSON)
│   │   ├── Characters/         # Sample: warrior, mage
│   │   ├── Skills/             # Sample skills
│   │   ├── Enemies/            # Sample: goblin, knight
│   │   ├── Stages/             # Sample: tutorial stage
│   │   └── README.md           # ← Data editing guide
│   │
│   ├── Scripts/                 # 💻 Code (BẠN SẼ VIẾT Ở ĐÂY)
│   │   ├── Core/               # Framework code
│   │   ├── Combat/             # Combat logic
│   │   ├── Data/               # Data models
│   │   ├── UI/                 # UI scripts
│   │   └── Utilities/          # Helpers
│   │
│   ├── Scenes/                  # 🎬 Unity scenes
│   ├── Prefabs/                 # 🎨 Prefabs
│   └── Tests/                   # 🧪 Unit tests
│
└── [Unity generated files]
```

---

## 👥 Team Roles & Responsibilities

### 👨‍💻 Developer A - Core Systems

**Week 1 Focus:**
- Event Bus system
- RNG Service
- Data Loading pipeline
- Save/Load system
- Turn Manager

**Week 2 Focus:**
- Skill System
- AI Controller
- Debug Tools

**Key Files:**
```
Scripts/Core/EventBus.cs
Scripts/Core/RNGService.cs
Scripts/Core/DataManager.cs
Scripts/Core/SaveManager.cs
Scripts/Combat/Managers/TurnManager.cs
```

---

### 👩‍💻 Developer B - Combat & Gameplay

**Week 1 Focus:**
- Combat Entity system
- Stats & Components
- Action Pipeline
- Status Effects

**Week 2 Focus:**
- Effect implementation
- Combat Flow Controller
- Content creation (characters/enemies)

**Key Files:**
```
Scripts/Combat/Entities/CombatEntity.cs
Scripts/Combat/Components/HealthComponent.cs
Scripts/Combat/Components/StatsComponent.cs
Scripts/Combat/Actions/ActionResolver.cs
Scripts/Combat/Effects/StatusEffect.cs
```

---

## 🔧 Development Workflow

### Daily Routine

**Morning (Ngày làm việc bắt đầu):**
```
1. Git pull latest changes
2. Daily standup (15 min)
   - What did I do yesterday?
   - What will I do today?
   - Any blockers?
3. Review WorkPlan for today's tasks
4. Start coding
```

**During Development:**
```
1. Pick ONE task from WorkPlan
2. Create feature branch: feature/task-name
3. Code following CodeConventions.md
4. Test frequently (don't wait til end of day)
5. Commit often with clear messages
6. Push to remote regularly
```

**Evening (Trước khi kết thúc):**
```
1. Run all tests
2. Fix any errors/warnings
3. Commit and push work
4. Update WorkPlan (mark completed tasks)
5. Prepare notes for tomorrow's standup
```

---

## 💻 Coding Standards Quick Reference

### Naming
```csharp
// Classes & Methods: PascalCase
public class CombatEntity { }
public void ApplyDamage() { }

// Variables: camelCase
private int currentHealth;

// Constants: UPPER_SNAKE_CASE or PascalCase
public const int MAX_HEALTH = 100;

// Interfaces: I prefix + PascalCase
public interface IDamageable { }
```

### File Organization
```csharp
using System;
using UnityEngine;

public class Example : MonoBehaviour 
{
    // Public fields
    public int PublicValue;
    
    // Properties
    public int Health { get; private set; }
    
    // Private fields
    private int currentHealth;
    
    // Unity lifecycle
    void Awake() { }
    void Start() { }
    void Update() { }
    
    // Public methods
    public void DoSomething() { }
    
    // Private methods
    private void Helper() { }
}
```

### Git Commits
```bash
# Format: <type>: <subject>
✅ feat: implement turn manager
✅ fix: resolve damage overflow bug
✅ refactor: optimize event bus
✅ test: add entity tests
✅ docs: update API documentation

❌ update
❌ fixed stuff
❌ WIP
```

---

## 🧪 Testing Workflow

### Write Tests As You Go

**NOT after feature is done:**
```
❌ Code all week → Test on Friday
```

**But incrementally:**
```
✅ Write function → Write test → Run test → Commit
```

### Test Locations
```
Tests/
├── EditMode/          # Unit tests (doesn't need Play mode)
│   ├── DamageCalculatorTests.cs
│   └── EntityTests.cs
└── PlayMode/          # Integration tests (needs Unity runtime)
    └── CombatFlowTests.cs
```

### Running Tests
```
Unity → Window → General → Test Runner
→ Click "Run All"
```

**All tests should be GREEN before merge!**

---

## 🐛 Debugging Tips

### Unity Console
```
Debug.Log("Value: " + value);           // Info
Debug.LogWarning("Potential issue");    // Warning
Debug.LogError("This is bad!");         // Error
```

### Breakpoints
```
1. Set breakpoint in IDE (F9)
2. Unity → Attach to Unity
3. Enter Play mode
4. Code pauses at breakpoint
```

### Common Issues

**"NullReferenceException"**
```csharp
// ❌ BAD
healthBar.UpdateHealth(hp);

// ✅ GOOD
if (healthBar != null) 
{
    healthBar.UpdateHealth(hp);
}

// ✅ BETTER
healthBar?.UpdateHealth(hp);
```

**"Scene not loading"**
```
1. Check Build Settings → Scenes in Build
2. Add your scene if missing
```

**"Script not compiling"**
```
1. Check Console for errors
2. Fix syntax errors (red underlines)
3. Wait for Unity to recompile
```

---

## 🤝 Collaboration Guidelines

### Communication

**Slack/Discord:**
- Quick questions, daily updates
- Share screenshots/videos
- Coordinate pair programming

**GitHub:**
- Code reviews
- Issue tracking
- Pull requests

**In person/Call:**
- Complex discussions
- Architecture decisions
- Debugging together

### Pair Programming

**When to pair:**
- Complex features (AI, Combat Flow)
- Stuck on bug >1 hour
- Integration work (Day 10)

**How to pair:**
```
1. Screen share or sit together
2. Driver: types code
3. Navigator: reviews, suggests
4. Switch roles every 30 min
```

### Code Review Checklist

**Before requesting review:**
- [ ] Code compiles
- [ ] Tests pass
- [ ] Follows naming conventions
- [ ] Comments explain WHY
- [ ] No TODO left behind

**When reviewing:**
- [ ] Logic correct?
- [ ] Edge cases handled?
- [ ] Performance okay?
- [ ] Readable code?
- [ ] Tests included?

---

## 🎯 Sprint Goals Reminder

### Week 1: Foundation
```
✅ Event Bus working
✅ Data loading functional
✅ Combat entities defined
✅ Turn manager implemented
✅ Basic actions executable
```

### Week 2: Integration
```
✅ Skills system complete
✅ AI making decisions
✅ Full combat loop working
✅ 1 battle playable end-to-end
✅ Demo-ready build
```

### Definition of Done
```
Feature is done when:
- Code written
- Tests passing
- Reviewed by teammate
- Documented (comments + README)
- No critical bugs
- Demo-able
```

---

## 📅 Important Dates

| Date | Event |
|------|-------|
| **Feb 24 (Mon)** | Sprint 1 starts |
| **Feb 28 (Fri)** | Mid-sprint check |
| **Mar 7 (Fri)** | Sprint 1 ends |
| **Mar 7 (PM)** | Sprint Demo & Retro |

---

## 🆘 Getting Help

### Documentation
1. Check relevant .md file first
2. Search in TechnicalDescription.md
3. Look at code examples in Data/

### Teammate
```
1. Ask in chat (quick questions)
2. Schedule pair programming (complex issues)
3. Code review (get feedback)
```

### External Resources
- [Unity Docs](https://docs.unity3d.com/)
- [C# Reference](https://docs.microsoft.com/en-us/dotnet/csharp/)
- Stack Overflow (last resort)

---

## ⚡ Productivity Tips

### Focus Time
```
🍅 Pomodoro Technique:
- 25 min focused work
- 5 min break
- Repeat 4x
- 15 min long break
```

### Avoid Distractions
```
✅ Turn off notifications
✅ Close unnecessary tabs
✅ Use headphones (even without music)
✅ Block social media (Work time)
```

### When Stuck
```
1. Try for 15 min yourself
2. Search documentation/Google
3. Try another 15 min
4. Ask teammate for help

DON'T waste 2+ hours stuck alone!
```

---

## 🎉 Ready to Start!

### First Steps (Day 1 Morning)

**Developer A:**
```
1. Create Scripts/Core/ folder
2. Create EventBus.cs
3. Write basic singleton pattern
4. Test in empty scene
5. Commit: "feat: add event bus foundation"
```

**Developer B:**
```
1. Create Scripts/Combat/Entities/ folder
2. Create CombatEntity.cs
3. Define base properties (HP, ATK, DEF)
4. Test with placeholder GameObject
5. Commit: "feat: add combat entity base class"
```

### Daily Sync Point

**9:00 AM:** Standup (15 min)  
**12:00 PM:** Lunch + casual check-in  
**5:00 PM:** End of day sync (10 min)

---

## ✅ Final Checklist Before Starting

- [ ] All documentation read
- [ ] Unity project opens successfully
- [ ] Git configured and tested
- [ ] IDE setup complete
- [ ] Understand my role (Dev A or Dev B)
- [ ] Know what to work on Day 1
- [ ] Have teammate's contact info
- [ ] Ready to write some code! 🚀

---

**LET'S BUILD SOMETHING AWESOME! 💪**

Good luck với Sprint 1! Remember:
- **Communicate early and often**
- **Test as you go, not at the end**
- **Ask for help when stuck**
- **Focus on working software, not perfect code**

---

*Setup Guide v1.0*  
*Created: February 22, 2026*

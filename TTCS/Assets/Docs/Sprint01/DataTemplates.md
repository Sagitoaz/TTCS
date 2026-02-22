# 📊 Data Definition Templates - TTCS

> Template files để định nghĩa Characters, Skills, Enemies và Stages
> Sử dụng format JSON để dễ edit và version control

---

## 📋 Table of Contents

1. [Character Template](#character-template)
2. [Skill Template](#skill-template)
3. [Enemy Template](#enemy-template)
4. [Stage Template](#stage-template)
5. [Status Effect Template](#status-effect-template)

---

## 👤 Character Template

### File: `CharacterData_Template.json`

```json
{
  "id": "char_xxx",
  "nameKey": "Character_Name",
  "description": "Character backstory and role description",
  
  "metadata": {
    "rarity": "SSR",
    "factionTag": "Forgotten",
    "roleTag": "Attacker",
    "element": "Fire"
  },
  
  "baseStats": {
    "level": 1,
    "hp": 2500,
    "atk": 350,
    "def": 120,
    "spd": 145,
    "crit": 0.25,
    "critDmg": 1.5,
    "resist": 0.15
  },
  
  "growthCurve": {
    "hpPerLevel": 125,
    "atkPerLevel": 18,
    "defPerLevel": 6,
    "spdPerLevel": 2
  },
  
  "skills": [
    "skill_char_xxx_basic",
    "skill_char_xxx_special_1",
    "skill_char_xxx_special_2",
    "skill_char_xxx_ultimate"
  ],
  
  "passive": {
    "id": "passive_xxx",
    "nameKey": "Passive_Name",
    "description": "What the passive does"
  },
  
  "visual": {
    "spritePath": "Characters/char_xxx_sprite",
    "portraitPath": "Characters/char_xxx_portrait",
    "animatorController": "Characters/char_xxx_animator"
  },
  
  "aiHints": {
    "priority": "offense",
    "preferredTargets": ["lowest_hp", "marked"],
    "defensiveThreshold": 0.3
  }
}
```

---

## ⚡ Skill Template

### File: `SkillData_Template.json`

```json
{
  "id": "skill_xxx",
  "nameKey": "Skill_Name",
  "description": "What the skill does",
  "type": "attack",
  
  "targetRule": {
    "type": "single_enemy",
    "count": 1,
    "filter": [],
    "canTargetSelf": false
  },
  
  "cost": {
    "mana": 30,
    "cooldown": 2,
    "limitPerFight": -1
  },
  
  "damage": {
    "formula": "ATK * 1.5",
    "element": "physical",
    "canCrit": true,
    "ignoreDefense": 0
  },
  
  "effects": [
    {
      "type": "damage",
      "value": "ATK * 1.5",
      "element": "fire",
      "chance": 1.0
    },
    {
      "type": "status_effect",
      "effectId": "burn",
      "duration": 3,
      "chance": 0.6,
      "stacks": 1
    }
  ],
  
  "timing": {
    "hasTimingWindow": true,
    "windows": [
      {
        "startMs": 200,
        "endMs": 400,
        "type": "cast"
      }
    ],
    "grades": {
      "perfect": {
        "threshold": 50,
        "bonus": {
          "damageMultiplier": 1.2,
          "effectChanceBonus": 0.1
        }
      },
      "good": {
        "threshold": 150,
        "bonus": {
          "damageMultiplier": 1.0
        }
      },
      "miss": {
        "threshold": 999,
        "penalty": {
          "damageMultiplier": 1.0
        }
      }
    }
  },
  
  "actionCost": {
    "timelineUnits": 100,
    "description": "Standard action"
  },
  
  "visual": {
    "animation": "skill_xxx_anim",
    "vfxPrefab": "VFX/skill_xxx_vfx",
    "sfx": "SFX/skill_xxx_sound",
    "cameraShake": 0.3
  }
}
```

---

## 👹 Enemy Template

### File: `EnemyData_Template.json`

```json
{
  "id": "enemy_xxx",
  "nameKey": "Enemy_Name",
  "description": "Enemy lore and behavior",
  "type": "elite",
  
  "baseStats": {
    "hp": 5000,
    "atk": 280,
    "def": 150,
    "spd": 120,
    "crit": 0.1,
    "resist": 0.2
  },
  
  "resistances": {
    "physical": 0.0,
    "fire": -0.5,
    "ice": 0.3,
    "lightning": 0.0,
    "dark": 0.5
  },
  
  "moveSet": [
    {
      "skillId": "enemy_xxx_slash",
      "weight": 40,
      "conditions": []
    },
    {
      "skillId": "enemy_xxx_guard",
      "weight": 30,
      "conditions": [
        {
          "type": "hp_below",
          "value": 0.5
        }
      ]
    },
    {
      "skillId": "enemy_xxx_berserker",
      "weight": 30,
      "conditions": [
        {
          "type": "hp_below",
          "value": 0.3
        }
      ]
    }
  ],
  
  "phases": [
    {
      "phaseId": 1,
      "hpRange": [1.0, 0.6],
      "moves": ["slash", "guard"],
      "weights": [60, 40],
      "behavior": "balanced"
    },
    {
      "phaseId": 2,
      "hpRange": [0.6, 0.0],
      "moves": ["slash", "berserker", "rage"],
      "weights": [30, 40, 30],
      "behavior": "aggressive"
    }
  ],
  
  "telegraph": {
    "enabled": true,
    "defaultDuration": 1.0,
    "cueType": "visual_and_sfx",
    "overrideByCue": {}
  },
  
  "aiProfile": {
    "aggression": 0.7,
    "targetPriority": ["lowest_hp", "highest_threat"],
    "useSkillsWisely": true,
    "retreatThreshold": 0.1
  },
  
  "loot": {
    "guaranteed": [
      {
        "type": "currency",
        "id": "gold",
        "amount": [50, 100]
      }
    ],
    "possible": [
      {
        "type": "item",
        "id": "item_health_potion",
        "chance": 0.3,
        "amount": [1, 2]
      }
    ]
  },
  
  "visual": {
    "spritePath": "Enemies/enemy_xxx_sprite",
    "animatorController": "Enemies/enemy_xxx_animator",
    "scale": 1.2
  }
}
```

---

## 🗺️ Stage Template

### File: `StageData_Template.json`

```json
{
  "id": "stage_xxx",
  "nameKey": "Stage_Name",
  "description": "Stage narrative and context",
  "chapter": 1,
  "order": 1,
  
  "requirements": {
    "minLevel": 5,
    "prerequisiteStages": ["stage_001"],
    "unlockConditions": []
  },
  
  "encounters": [
    {
      "wave": 1,
      "enemies": [
        {
          "enemyId": "enemy_goblin",
          "level": 5,
          "position": 0
        },
        {
          "enemyId": "enemy_goblin",
          "level": 5,
          "position": 1
        }
      ],
      "spawnDelay": 0
    },
    {
      "wave": 2,
      "enemies": [
        {
          "enemyId": "enemy_dark_knight",
          "level": 6,
          "position": 1
        }
      ],
      "spawnDelay": 1.0
    }
  ],
  
  "environment": {
    "modifiers": [
      {
        "type": "stat_modifier",
        "stat": "spd",
        "value": -10,
        "target": "all_player"
      }
    ],
    "hazards": [],
    "weatherEffect": "none"
  },
  
  "rewards": {
    "firstClear": {
      "gold": 500,
      "exp": 300,
      "items": [
        {
          "id": "item_beginner_sword",
          "amount": 1
        }
      ]
    },
    "repeatClear": {
      "gold": 100,
      "exp": 50,
      "items": []
    },
    "stars": [
      {
        "condition": "clear_under_10_turns",
        "reward": {
          "gold": 100
        }
      },
      {
        "condition": "no_deaths",
        "reward": {
          "exp": 100
        }
      },
      {
        "condition": "use_no_items",
        "reward": {
          "items": [
            {
              "id": "item_rare_gem",
              "amount": 1
            }
          ]
        }
      }
    ]
  },
  
  "theme": {
    "backgroundImage": "Stages/stage_xxx_bg",
    "musicTrack": "Music/stage_xxx_bgm",
    "ambientSFX": "SFX/forest_ambient"
  },
  
  "difficulty": {
    "rating": "normal",
    "recommendedLevel": 5,
    "recommendedPower": 1200
  }
}
```

---

## 🌟 Status Effect Template

### File: `StatusEffectData_Template.json`

```json
{
  "id": "effect_xxx",
  "nameKey": "Effect_Name",
  "description": "What the effect does",
  "type": "debuff",
  
  "duration": {
    "turns": 3,
    "permanent": false,
    "canExtend": true,
    "maxDuration": 5
  },
  
  "stacking": {
    "stackable": true,
    "maxStacks": 3,
    "stackBehavior": "refresh_duration"
  },
  
  "effects": {
    "onApply": [
      {
        "type": "log",
        "message": "Applied {effectName} to {target}"
      },
      {
        "type": "stat_modifier",
        "stat": "atk",
        "modifier": -0.2,
        "duration": "effect"
      }
    ],
    "onTick": [
      {
        "type": "damage",
        "formula": "0.05 * maxHP",
        "element": "true_damage",
        "canCrit": false
      }
    ],
    "onRemove": [
      {
        "type": "log",
        "message": "{effectName} expired on {target}"
      }
    ]
  },
  
  "triggers": {
    "onTurnStart": true,
    "onTurnEnd": false,
    "onActionExecute": false,
    "onDamageTaken": false,
    "onDamageDealt": false
  },
  
  "immunity": {
    "grantsImmunityTo": [],
    "canBeCleansed": true,
    "resistedBy": ["effect_immunity_shield"]
  },
  
  "visual": {
    "iconPath": "Icons/effect_xxx_icon",
    "vfxPrefab": "VFX/effect_xxx_vfx",
    "color": "#FF0000",
    "floatingText": true
  }
}
```

---

## 🎯 How to Use These Templates

### Step 1: Copy Template
```bash
# Copy template file
cp DataTemplates/CharacterData_Template.json Data/Characters/char_warrior.json
```

### Step 2: Fill In Values
- Replace `xxx` với ID duy nhất
- Điền thông tin theo game design
- Giữ structure JSON hợp lệ

### Step 3: Validate
- Check JSON syntax với online validator
- Đảm bảo tất cả referenced IDs tồn tại
- Test load trong Unity

### Step 4: Test
- Load data vào game
- Verify stats hiển thị đúng
- Test combat với data mới

---

## 📝 Naming Conventions

### IDs
```
Characters:  char_<name>_<variant>
Skills:      skill_<character>_<skillname>
Enemies:     enemy_<type>_<variant>
Stages:      stage_<chapter>_<order>
Effects:     effect_<name>
```

**Examples:**
```
char_warrior_basic
skill_warrior_slash
enemy_goblin_common
stage_01_intro
effect_burn
```

### Name Keys (for localization)
```
<Type>_<Name>
```

**Examples:**
```
Character_Warrior
Skill_FireSlash
Enemy_DarkKnight
Stage_ForestBattle
```

---

## ✅ Validation Checklist

Khi tạo data mới, check:

- [ ] **ID unique** - Không trùng với data khác
- [ ] **All references valid** - Skills, effects exist
- [ ] **Stats in valid range** - No negatives (except resist)
- [ ] **Formulas correct** - Use valid stat names
- [ ] **Probabilities 0-1** - Chance values
- [ ] **JSON valid** - No syntax errors
- [ ] **File paths correct** - Assets exist
- [ ] **Tested in game** - Actually works

---

## 🔧 Optional: ScriptableObject Alternative

Nếu muốn dùng ScriptableObject thay vì JSON, cấu trúc C# tương ứng:

### CharacterData.cs
```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "TTCS/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Metadata")]
    public string id;
    public string nameKey;
    public string description;
    public Rarity rarity;
    public FactionTag factionTag;
    public RoleTag roleTag;
    public Element element;
    
    [Header("Base Stats")]
    public int baseHP;
    public int baseATK;
    public int baseDEF;
    public int baseSPD;
    public float baseCrit;
    public float baseCritDmg;
    public float baseResist;
    
    [Header("Growth")]
    public int hpPerLevel;
    public int atkPerLevel;
    public int defPerLevel;
    public int spdPerLevel;
    
    [Header("Skills")]
    public SkillData[] skills;
    public PassiveData passive;
    
    [Header("Visual")]
    public Sprite sprite;
    public Sprite portrait;
    public RuntimeAnimatorController animator;
    
    [Header("AI Hints")]
    public AIBehaviorProfile aiHints;
}
```

**Pros:** 
- ✅ Unity Inspector editing
- ✅ Type safety
- ✅ Asset references easier

**Cons:**
- ❌ Harder to version control
- ❌ Merge conflicts
- ❌ No external editing

> 💡 **Recommendation:** Use JSON for data, ScriptableObject for config

---

## 📚 Further Reading

- `TechnicalDescription.md` - Section 3: Data Models
- `CodeConventions.md` - Naming & structure rules
- Unity JSON Serialization docs

---

*Template Version: 1.0*  
*Last Updated: February 22, 2026*

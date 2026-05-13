# Skill Cast Portrait Panel - Unity Editor Setup

File code da duoc tao san:

- `Assets/Scripts/UI/Combat/SkillCastPortraitPanel.cs`
- `Assets/Scripts/UI/Combat/CombatUIController.cs` (da duoc noi them reference)

Feature:

- Moi khi co `SkillCastEvent`, panel se hien portrait caster.
- Character (player) truot tu trai vao giua, dung mot luc roi an.
- Enemy truot tu phai vao giua, dung mot luc roi an.
- Portrait load tu JSON (`visual.portraitPath`, fallback `visual.spritePath`).

---

## 1) Tao UI panel trong Combat Canvas

1. Mo scene combat, chon `Canvas` dang chua `CombatUIController`.
2. Tao object moi:
   - `UI > Panel`
   - Dat ten: `SkillCastPortraitPanel`
3. Trên `SkillCastPortraitPanel`:
   - Anchor: Middle Center
   - Pivot: `(0.5, 0.5)`
   - Kich thuoc goi y: `Width 700`, `Height 260` (tuy UI cua ban)
4. Tao child:
   - `UI > Image`
   - Dat ten: `PortraitImage`
   - Anchor stretch hoac center tuy layout, nhung phai nam trong panel.

> Luu y: Script di chuyen tren `RectTransform` cua `SkillCastPortraitPanel` (panel root).

---

## 2) Add component va gan references

1. Chon `SkillCastPortraitPanel` object.
2. Add component: `SkillCastPortraitPanel`.
3. Add component: `CanvasGroup` (bat buoc de fade out).
4. Gan fields trong `SkillCastPortraitPanel`:
   - `Panel Root` -> RectTransform cua `SkillCastPortraitPanel`
   - `Portrait Image` -> `PortraitImage` (Image)
   - `Canvas Group` -> CanvasGroup tren panel root

Gia tri motion goi y:

- `Enter Duration`: `0.28`
- `Hold Duration`: `0.45`
- `Fade Out Duration`: `0.12`
- `Offscreen Padding`: `120`

---

## 3) Noi vao CombatUIController

1. Chon object dang gan script `CombatUIController`.
2. Field moi trong inspector:
   - `Skill Cast Portrait Panel`
3. Keo-tha object `SkillCastPortraitPanel` vao field nay.

Sau buoc nay feature duoc kich hoat trong runtime.

---

## 4) Setup portrait cho tung character/enemy

Script tu dong resolve portrait theo caster:

- Character: `DataManager.LoadCharacter(character.CharacterId)`
- Enemy: `DataManager.LoadEnemy(enemy.EnemyTemplateId)`
- Lay path:
  1. `visual.portraitPath`
  2. Neu rong -> `visual.spritePath`

Vi vay moi JSON character/enemy can co mot trong hai path tren.

Vi du:

```json
"visual": {
  "portraitPath": "Characters/baldr_portrait",
  "spritePath": "Characters/baldr_sprite"
}
```

---

## 5) Sprite import settings (quan trong)

Voi portrait sprite su dung cho UI:

- Texture Type: `Sprite (2D and UI)`
- Sprite Mode: `Single`
- Read/Write: khong bat buoc
- Compression: tuy chon, uu tien giu chat luong

Neu portrait khong hien:

1. Kiem tra path trong JSON dung chua.
2. Kiem tra sprite co nam trong duong dan ma `DataManager.LoadCharacterPortraitSprite` load duoc.
3. Kiem tra `PortraitImage` da gan dung.
4. Kiem tra panel co dang bi object khac che (sorting/order).

---

## 6) Runtime behavior de verify nhanh

1. Vao combat.
2. Cho mot player cast skill:
   - Panel vao tu ben trai -> giua -> dung -> an.
3. Cho enemy cast skill:
   - Panel vao tu ben phai -> giua -> dung -> an.
4. Kiem tra portrait dung voi caster.

Neu skill cast lien tiep, script se dung sequence cu va phat sequence moi theo event moi.


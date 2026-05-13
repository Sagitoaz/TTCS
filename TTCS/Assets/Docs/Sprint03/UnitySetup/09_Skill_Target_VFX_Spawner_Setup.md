# Character Skill Target VFX Setup

Tai lieu nay huong dan cach gan VFX spawn len target theo tung prefab nhan vat/enemy.

Muc tieu cua cach lam moi:

- Khong sua file JSON skill moi lan cau hinh VFX.
- Khong tao profile global can nho tat ca skill cua moi nhan vat.
- Moi prefab nhan vat/enemy tu giu cau hinh VFX cua rieng no.
- Prefab VFX dung Animator cua Unity Editor truc tiep. Script khong can setup animation cho VFX.

## 1. Script gan vao dau?

Gan script `CharacterSkillTargetVFXController` vao prefab nhan vat hoac enemy.

Vi du voi prefab Hodr:

1. Mo prefab `char_hodr` trong Prefab Mode.
2. Chon GameObject root cua prefab, cung object dang co `CharacterView` hoac `EnemyView`.
3. Bam `Add Component`.
4. Tim `CharacterSkillTargetVFXController`.
5. Add component nay vao root.

Vi du hierarchy:

```text
char_hodr
- CharacterView
- CharacterAnimator
- Animator
- CharacterSkillTargetVFXController
- Body
- Head
- Weapon
- HitAnchor
- HeadAnchor
```

Voi enemy cung tuong tu:

```text
enemy_fomortiis
- EnemyView
- CharacterAnimator
- Animator
- CharacterSkillTargetVFXController
- Body
- HitAnchor
- HeadAnchor
```

Khong gan script nay vao prefab VFX.
Khong gan script nay vao `CombatManagers`.
Khong can tao object spawner trong scene.

## 2. Vi sao cach nay de quan ly hon?

Moi prefab chi can biet skill cua chinh no.

Vi du:

- Prefab Hodr chi khai bao `skill_hodr_quick_shot`, `skill_hodr_piercing_round`, ...
- Prefab Baldr chi khai bao `skill_baldr_sun_slash`, `skill_baldr_ragnarok_dawn`, ...
- Prefab Fomortiis chi khai bao `skill_fomortiis_dragon_roar`, `skill_fomortiis_void_summons`, ...

Khi vao battle, chi prefab nao duoc spawn thi component tren prefab do moi ton tai va moi lang nghe skill cast. Nhan vat khong duoc load vao man choi thi khong co component nao chay.

## 3. Prefab VFX can chuan bi nhu nao?

Ban da dung Animator trong Unity Editor de lam anim VFX, nen script khong can dieu khien animation cua VFX.

Prefab VFX chi can:

1. La prefab binh thuong trong Project.
2. Co `Animator` neu ban muon no tu play animation.
3. Animator co default state tu chay khi prefab duoc spawn.
4. Neu VFX la sprite, SpriteRenderer nen co sorting layer/order dung de hien tren nhan vat.

Goi y thu muc:

```text
Assets/Prefabs/VFX/SkillTarget/
```

Vi du prefab:

- `Hodr_Bullet.prefab`
- `Hodr_Impact.prefab`
- `Baldr_LightExplosion.prefab`
- `Fomortiis_DarkRoar.prefab`

## 4. Them skill VFX vao prefab nhan vat

Sau khi da add `CharacterSkillTargetVFXController` vao prefab:

1. Chon root prefab.
2. Trong Inspector, tim component `CharacterSkillTargetVFXController`.
3. Field `Owner View` co the de trong. Script se tu tim `CharacterView` hoac `EnemyView`.
4. Mo list `This Character's Skill Target VFX`.
5. Tang `Size` len theo so skill cua nhan vat can VFX.

Moi element trong list la mot skill cua nhan vat do.

Voi moi skill:

1. Nhap `Skill Id`.
2. Mo list `Layers`.
3. Tang `Size` theo so prefab VFX muon spawn.
4. Voi moi layer, keo prefab VFX vao field `Prefab`.
5. Chon `Spawn Mode`.

Can nhap `Skill Id` dung voi id trong JSON, nhung chi nhap mot lan tren prefab cua nhan vat do. Khong sua JSON.

## 5. Hai kieu spawn VFX

### SpawnAtTarget

Dung khi VFX xuat hien truc tiep tren target.

Dung cho:

- No impact.
- Set danh xuong target.
- Vong phep quanh target.
- Dau an tren dau target.
- Lua/chay/doc xuat hien tren nguoi target.

Thong so goi y:

| Field | Gia tri goi y |
|---|---|
| `Spawn Mode` | `SpawnAtTarget` |
| `Target Anchor` | `HitAnchor` |
| `Delay` | `0.1` den `0.4` |
| `Lifetime` | Bang thoi gian anim prefab VFX |
| `Parent To Target` | Bat neu muon VFX di theo target |

### FlyFromCasterToTarget

Dung khi VFX bay tu caster sang target.

Dung cho:

- Dan sung.
- Mui ten.
- Cau phep.
- Tia nang luong.
- Vat the phong ra.

Thong so goi y:

| Field | Gia tri goi y |
|---|---|
| `Spawn Mode` | `FlyFromCasterToTarget` |
| `Caster Anchor` | `HitAnchor` |
| `Target Anchor` | `HitAnchor` |
| `Travel Duration` | `0.12` den `0.45` |
| `Lifetime` | Bang thoi gian prefab con ton tai sau khi bay den |
| `Rotate Toward Travel Direction` | Bat neu sprite can quay theo huong bay |

## 6. Vi du setup Hodr

Mo prefab Hodr va add `CharacterSkillTargetVFXController`.

### Skill 1: skill_hodr_quick_shot

Trong `This Character's Skill Target VFX`, tao element:

- `Skill Id`: `skill_hodr_quick_shot`

Layer 0:

- `Label`: `Bullet`
- `Prefab`: keo `Hodr_Bullet.prefab`
- `Spawn Mode`: `FlyFromCasterToTarget`
- `Delay`: `0.05`
- `Lifetime`: `0.2`
- `Caster Anchor`: `HitAnchor`
- `Target Anchor`: `HitAnchor`
- `Start Offset`: `(0.2, 0.05, 0)`
- `Target Offset`: `(0, 0.05, 0)`
- `Travel Duration`: `0.18`
- `Rotate Toward Travel Direction`: bat neu can

Layer 1:

- `Label`: `Impact`
- `Prefab`: keo `Hodr_Impact.prefab`
- `Spawn Mode`: `SpawnAtTarget`
- `Delay`: `0.22`
- `Lifetime`: `0.6`
- `Target Anchor`: `HitAnchor`
- `Parent To Target`: bat neu impact can bam target

### Skill 2: skill_hodr_piercing_round

Tao element moi:

- `Skill Id`: `skill_hodr_piercing_round`

Layer 0:

- `Label`: `Charged Bullet`
- `Prefab`: keo prefab dan manh
- `Spawn Mode`: `FlyFromCasterToTarget`
- `Delay`: `0.1`
- `Travel Duration`: `0.25`
- `Lifetime`: `0.35`

Layer 1:

- `Label`: `Pierce Burst`
- `Prefab`: keo prefab no manh tren target
- `Spawn Mode`: `SpawnAtTarget`
- `Delay`: `0.35`
- `Lifetime`: `1.0`

## 7. Vi du setup Fomortiis

Mo prefab Fomortiis va add `CharacterSkillTargetVFXController`.

### Skill: skill_fomortiis_dragon_roar

- `Skill Id`: `skill_fomortiis_dragon_roar`

Layer 0:

- `Label`: `Dark Roar Wave`
- `Prefab`: keo prefab song am bong toi
- `Spawn Mode`: `SpawnAtTarget`
- `Delay`: `0.25`
- `Lifetime`: `1.0`
- `Target Anchor`: `HitAnchor`

### Skill: skill_fomortiis_void_summons

- `Skill Id`: `skill_fomortiis_void_summons`

Layer 0:

- `Label`: `Void Portal`
- `Prefab`: keo prefab cong bong toi
- `Spawn Mode`: `SpawnAtTarget`
- `Delay`: `0.2`
- `Lifetime`: `1.2`
- `Target Anchor`: `Root`

Layer 1:

- `Label`: `Shadow Claw`
- `Prefab`: keo prefab vuot bong toi
- `Spawn Mode`: `SpawnAtTarget`
- `Delay`: `0.45`
- `Lifetime`: `0.8`
- `Target Anchor`: `HitAnchor`

## 8. Cach test

1. Save prefab sau khi cau hinh xong.
2. Mo scene combat.
3. Dam bao character/enemy prefab do dang duoc scene spawn ra.
4. Enter Play Mode.
5. Cho nhan vat dung skill da cau hinh.
6. Kiem tra VFX:
   - Neu mode la `SpawnAtTarget`, prefab hien tai target.
   - Neu mode la `FlyFromCasterToTarget`, prefab bay tu caster sang target.
   - Neu skill co nhieu target, moi target nhan mot bo VFX.
   - Neu skill co nhieu layer, cac layer chay theo delay rieng.

## 9. Neu khong thay VFX

Kiem tra theo thu tu:

1. Script `CharacterSkillTargetVFXController` da gan vao root prefab nhan vat/enemy chua.
2. Prefab trong battle co dung la prefab ban vua sua khong.
3. `Skill Id` trong component co dung tuyet doi voi id skill JSON khong.
4. Skill do co that su duoc cast trong battle khong.
5. Prefab VFX da keo vao field `Prefab` chua.
6. `Lifetime` co qua ngan khong.
7. Sorting layer/order cua VFX co bi nam sau nhan vat khong.
8. `CharacterView` cua target co `HitAnchor`/`HeadAnchor` dung vi tri chua.

## 10. Cong thuc nhanh cho skill moi

Moi lan mot nhan vat co skill can VFX len target:

1. Mo prefab nhan vat do.
2. Chon root prefab.
3. Neu chua co, add `CharacterSkillTargetVFXController`.
4. Them element moi trong `This Character's Skill Target VFX`.
5. Nhap `Skill Id`.
6. Them layer.
7. Keo prefab VFX vao.
8. Chon `SpawnAtTarget` hoac `FlyFromCasterToTarget`.
9. Chinh delay/lifetime/anchor.
10. Save prefab va Play test.

Tat ca cau hinh nam tren prefab cua nhan vat do, nen de quan ly theo tung character va khong phai nho mot profile global.

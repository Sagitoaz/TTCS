# 05 - Data, Save, EventBus

## 1. Luồng DataManager

- `DataManager.Awake -> LoadAllData`.
- Load theo thư mục JSON và validate.
- Cache theo `id` để truy xuất nhanh.

API chính:
- `LoadCharacter`
- `LoadSkill`
- `LoadEnemy`
- `LoadStage`

## 2. Luồng SaveManager

- `NewGame` tạo save mặc định trong memory.
- `Save(slot)` serialize ra file JSON.
- `Load(slot)` deserialize vào `CurrentSave`.
- `DeleteSave(slot)` xóa file slot.

## 3. Luồng EventBus

- `Subscribe<T>` đăng ký listener.
- `Publish<T>` gửi event cho tất cả listener.
- `Unsubscribe<T>` gỡ listener.

## 4. Bảng producer-consumer chính

| Event | Producer | Consumer |
|---|---|---|
| CombatStartedEvent | CombatFlowController | Audio/UI/Logger |
| TurnStartedEvent | TurnManager | UI turn routing/animation highlight |
| DamageTakenEvent | HealthComponent | HUD/FloatingText/VFX/Audio |
| SkillCastEvent | SkillManager/ActionResolver | Animation/Audio |
| CombatEndedEvent | CombatFlowController | Result UI/BGM |

## 5. Nguyên tắc khi thêm event mới

- Event name rõ ý nghĩa nghiệp vụ.
- Payload vừa đủ để consumer xử lý.
- Publish tại đúng điểm lifecycle.
- Subscribe/Unsubscribe cân bằng theo vòng đời MonoBehaviour.

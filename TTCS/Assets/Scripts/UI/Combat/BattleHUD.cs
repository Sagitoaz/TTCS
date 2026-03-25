using System;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TTCS.Combat.Entities;
using TTCS.Core.Data;
using TTCS.Core.Events;
using TTCS.Data;
using TTCS.Debugging;
using static TTCS.Debugging.DebugLogger;

namespace TTCS.UI.Combat
{
    /// <summary>
    /// 🔵 Dev A - Battle HUD
    /// Hiển thị HP/MP bars cho tối đa 3 ally và 3 enemy.
    /// Lắng nghe DamageTakenEvent và HealingReceivedEvent để animate thanh HP.
    /// </summary>
    public class BattleHUD : MonoBehaviour
    {
        // ─── Inner Type ───────────────────────────────────────────────────
        [System.Serializable]
        private class HUDSlot
        {
            public string EntityId;
            public Slider          HPSlider;
            public Slider          MPSlider;
            public Image           PortraitImage;
            public TextMeshProUGUI HPText;
            public CanvasGroup    SlotGroup;

            private float _maxHP = 1f;
            private float _maxMP = 1f;

            public void Initialize(CombatEntity entity, int maxMP, Sprite portraitSprite)
            {
                EntityId  = entity.ID;
                _maxHP    = entity.Health.MaxHP;
                _maxMP    = Mathf.Max(1, maxMP);

                LockSliderInput(HPSlider);
                LockSliderInput(MPSlider);

                HPSlider.value = entity.HPPercent;
                MPSlider.value = TTCS.Combat.Managers.SkillManager.Instance != null
                    ? (float)TTCS.Combat.Managers.SkillManager.Instance.GetMana(entity.ID) / _maxMP
                    : 1f;

                if (PortraitImage != null)
                {
                    PortraitImage.sprite = portraitSprite;
                    PortraitImage.color = portraitSprite != null ? Color.white : new Color(1f, 1f, 1f, 0f);
                    PortraitImage.preserveAspect = true;
                }

                SetHPText(entity.HPPercent);
                SlotGroup.alpha = 1f;
                gameObject.SetActive(true);
            }

            private static void LockSliderInput(Slider slider)
            {
                if (slider == null) return;

                slider.interactable = false;
                var nav = slider.navigation;
                nav.mode = Navigation.Mode.None;
                slider.navigation = nav;
            }

            public void AnimateHP(float newPercent)
            {
                HPSlider.DOValue(newPercent, 0.4f).SetEase(Ease.OutCubic);
                // BUG-1 FIX: truyền newPercent vào SetHPText thay vì đọc HPSlider.value cũ
                SetHPText(newPercent);
            }

            public void AnimateMP(int currentMana, int maxMana)
            {
                if (MPSlider == null) return;
                float ratio = maxMana > 0 ? (float)currentMana / maxMana : 0f;
                MPSlider.DOValue(ratio, 0.3f).SetEase(Ease.OutCubic);
            }

            

            public void SetDead()
            {
                // BUG-3 FIX: animate HP bar về 0 và cập nhật text trước khi fade
                HPSlider.DOValue(0f, 0.3f).SetEase(Ease.OutCubic);
                SetHPText(0f);
                SlotGroup.DOFade(0.4f, 0.5f).SetDelay(0.2f);
            }

            private void SetHPText(float percent)
            {
                if (HPText != null)
                    HPText.text = $"{Mathf.RoundToInt(percent * _maxHP)}/{Mathf.RoundToInt(_maxHP)}";
            }

            private GameObject gameObject => HPSlider.gameObject.transform.parent.gameObject;
        }

        // ─── Inspector ────────────────────────────────────────────────────
        [Header("Ally Slots (max 3)")]
        [SerializeField] private HUDSlot[] _allySlots  = new HUDSlot[3];

        [Header("Enemy Slots (max 3)")]
        [SerializeField] private HUDSlot[] _enemySlots = new HUDSlot[3];

        // ─── Slot Lookup ──────────────────────────────────────────────────
        private readonly Dictionary<string, HUDSlot> _slotMap = new();
        private readonly Dictionary<string, Sprite> _portraitCache = new();

        // ──────────────────────────────────────────────────────────────────
        #region Initialization

        /// <summary>
        /// Gán entity vào các slot và bắt đầu lắng nghe events.
        /// </summary>
        public void InitializeSlots(List<CombatEntity> allies, List<CombatEntity> enemies)
        {
            _slotMap.Clear();

            InitGroup(allies,  _allySlots);
            InitGroup(enemies, _enemySlots);

            SubscribeEvents();
        }

        private void InitGroup(List<CombatEntity> entities, HUDSlot[] slots)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (i < entities.Count && entities[i] != null)
                {
                    Sprite portrait = ResolvePortraitSprite(entities[i]);
                    int maxMP = TTCS.Combat.Managers.SkillManager.Instance != null
                        ? TTCS.Combat.Managers.SkillManager.Instance.GetMaxMana(entities[i].ID)
                        : 100;
                    slots[i].Initialize(entities[i], maxMP, portrait);
                    _slotMap[entities[i].ID] = slots[i];
                }
                else
                {
                    // Ẩn slot thừa
                    if (slots[i].HPSlider != null)
                        slots[i].HPSlider.gameObject.transform.parent.gameObject.SetActive(false);
                }
            }
        }

        private Sprite ResolvePortraitSprite(CombatEntity entity)
        {
            if (entity == null || DataManager.Instance == null) return null;

            string portraitPath = null;

            if (entity is Character character)
            {
                var data = DataManager.Instance.LoadCharacter(character.CharacterId);
                portraitPath = data?.visual?.portraitPath;

                if (string.IsNullOrWhiteSpace(portraitPath))
                    portraitPath = data?.visual?.spritePath;
            }
            else if (entity is Enemy)
            {
                // Theo yêu cầu hiện tại: enemy chưa hiển thị portrait trên BattleHUD.
                return null;
            }

            if (string.IsNullOrWhiteSpace(portraitPath))
            {
                LogWarning($"BattleHUD: Missing portrait path for entity '{entity.ID}'.", LogCategory.UI);
                return null;
            }

            string cacheKey = portraitPath.Trim();
            if (_portraitCache.TryGetValue(cacheKey, out var cachedSprite))
                return cachedSprite;

            var sprite = LoadCharacterPortraitFromProjectPath(portraitPath);
            if (sprite == null)
                sprite = LoadSpriteFromResourcesPath(portraitPath);

            _portraitCache[cacheKey] = sprite;

            if (sprite == null)
                LogWarning($"BattleHUD: Could not load portrait sprite from 'Assets/Sprites/Characters' or Resources path '{portraitPath}' for entity '{entity.ID}'.", LogCategory.UI);

            return sprite;
        }

        private Sprite LoadSpriteFromResourcesPath(string rawPath)
        {
            string normalizedPath = NormalizeResourcesPath(rawPath);
            if (string.IsNullOrWhiteSpace(normalizedPath))
                return null;

            if (_portraitCache.TryGetValue(normalizedPath, out var cachedSprite))
                return cachedSprite;

            var sprite = Resources.Load<Sprite>(normalizedPath);
            _portraitCache[normalizedPath] = sprite;
            return sprite;
        }

        private static Sprite LoadCharacterPortraitFromProjectPath(string rawPath)
        {
#if UNITY_EDITOR
            string normalized = NormalizeAssetLikePath(rawPath);
            if (string.IsNullOrWhiteSpace(normalized))
                return null;

            foreach (var candidate in BuildAssetCandidates(normalized))
            {
                var sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(candidate);
                if (sprite != null)
                    return sprite;
            }
#endif
            return null;
        }

        private static List<string> BuildAssetCandidates(string normalized)
        {
            var candidates = new List<string>();

            if (normalized.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                candidates.AddRange(WithCommonImageExtensions(normalized));
                return candidates;
            }

            if (normalized.StartsWith("Characters/", StringComparison.OrdinalIgnoreCase))
            {
                candidates.AddRange(WithCommonImageExtensions($"Assets/Sprites/{normalized}"));
                return candidates;
            }

            candidates.AddRange(WithCommonImageExtensions($"Assets/Sprites/Characters/{normalized}"));
            return candidates;
        }

        private static IEnumerable<string> WithCommonImageExtensions(string path)
        {
            if (HasImageExtension(path))
            {
                yield return path;
                yield break;
            }

            yield return path + ".png";
            yield return path + ".jpg";
            yield return path + ".jpeg";
        }

        private static bool HasImageExtension(string path)
        {
            string ext = Path.GetExtension(path);
            return ext.Equals(".png", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeAssetLikePath(string rawPath)
        {
            if (string.IsNullOrWhiteSpace(rawPath)) return null;

            string path = rawPath.Trim().Replace('\\', '/');
            if (path.StartsWith("Resources/", StringComparison.OrdinalIgnoreCase))
                path = path.Substring("Resources/".Length);

            return path;
        }

        private static string NormalizeResourcesPath(string rawPath)
        {
            if (string.IsNullOrWhiteSpace(rawPath)) return null;

            string path = rawPath.Trim().Replace('\\', '/');
            if (path.StartsWith("Resources/", StringComparison.OrdinalIgnoreCase))
                path = path.Substring("Resources/".Length);

            int extensionIndex = path.LastIndexOf('.');
            if (extensionIndex > 0)
                path = path.Substring(0, extensionIndex);

            return path;
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Event Handling

        private void SubscribeEvents()
        {
            var bus = EventBus.Instance;
            bus.Subscribe<DamageTakenEvent>(OnDamageTaken);
            bus.Subscribe<HealingReceivedEvent>(OnHealingReceived);
            bus.Subscribe<EntityDeathEvent>(OnEntityDeath);
            bus.Subscribe<ManaChangedEvent>(OnManaChanged);
            
        }

        private void UnsubscribeEvents()
        {
            var bus = EventBus.Instance;
            bus.Unsubscribe<DamageTakenEvent>(OnDamageTaken);
            bus.Unsubscribe<HealingReceivedEvent>(OnHealingReceived);
            bus.Unsubscribe<EntityDeathEvent>(OnEntityDeath);
            bus.Unsubscribe<ManaChangedEvent>(OnManaChanged);
            
        }

        private void OnDamageTaken(DamageTakenEvent e)
        {
            
            if (!_slotMap.TryGetValue(e.TargetId, out var slot)) return;
            

            // Tính percent mới dựa trên entity thực — cần lấy từ CombatUIController
            float newPercent = CombatUIController.Instance != null
                ? CombatUIController.Instance.GetEntityHPPercent(e.TargetId)
                : Mathf.Max(0f, slot.HPSlider.value - 0.05f);
            

             

            slot.AnimateHP(newPercent);
        }

        private void OnHealingReceived(HealingReceivedEvent e)
        {
            if (!_slotMap.TryGetValue(e.TargetId, out var slot)) return;

            float newPercent = CombatUIController.Instance != null
                ? CombatUIController.Instance.GetEntityHPPercent(e.TargetId)
                : Mathf.Min(1f, slot.HPSlider.value + 0.1f);

            // BUG-2 FIX: dùng AnimateHP để cập nhật cả bar lẫn text
            slot.AnimateHP(newPercent);
        }

        private void OnEntityDeath(EntityDeathEvent e)
        {
            if (_slotMap.TryGetValue(e.EntityId, out var slot))
                slot.SetDead();
        }

        private void OnManaChanged(ManaChangedEvent e)
        {
            if (_slotMap.TryGetValue(e.EntityId, out var slot))
                slot.AnimateMP(e.CurrentMana, e.MaxMana);
        }

        

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        #endregion
    }
}

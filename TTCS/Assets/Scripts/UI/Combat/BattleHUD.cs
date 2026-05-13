using System;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TTCS.Combat.Entities;
using TTCS.Combat.Effects;
using TTCS.Core.Data;
using TTCS.Core.Events;
using TTCS.Core.Save;
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
            public Slider          HPDelayedSlider;
            public Slider          MPSlider;
            public Image           PortraitImage;
            public TextMeshProUGUI LevelText;
            public Image           RoleIconImage;
            public Image           ElementIconImage;
            public Image           RarityBorderImage;
            public Image           RarityBackgroundImage;
            public TextMeshProUGUI HPText;
            public CanvasGroup    SlotGroup;
            public RectTransform   EffectRoot;

            private float _maxHP = 1f;
            private float _maxMP = 1f;
            private float _lastHPPercent = 1f;
            private readonly List<GameObject> _spawnedEffectIcons = new List<GameObject>();

            public void Initialize(CombatEntity entity, int maxMP, Sprite portraitSprite)
            {
                EntityId  = entity.ID;
                _maxHP    = entity.Health.MaxHP;
                _maxMP    = Mathf.Max(1, maxMP);

                EnsureDelayedHpSlider();

                LockSliderInput(HPSlider);
                LockSliderInput(HPDelayedSlider);
                LockSliderInput(MPSlider);

                _lastHPPercent = Mathf.Clamp01(entity.HPPercent);
                HPSlider.value = _lastHPPercent;
                if (HPDelayedSlider != null)
                    HPDelayedSlider.value = _lastHPPercent;
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
                if (SlotRectTransform != null)
                    SlotRectTransform.localScale = Vector3.one;
                var root = SlotRootObject;
                if (root != null)
                    root.SetActive(true);

                ClearEffectIcons();
            }

            public void SetAllyMetadata(int level, string rarity, Sprite roleIcon, Sprite elementIcon, Sprite borderSprite, Sprite backgroundSprite, Color borderColor, Color backgroundColor)
            {
                if (LevelText != null)
                    LevelText.text = $"Lv.{Mathf.Max(1, level)}";

                SetOptionalIcon(RoleIconImage, roleIcon);
                SetOptionalIcon(ElementIconImage, elementIcon);


                if (RarityBorderImage != null)
                {
                    if (borderSprite != null)
                    {
                        RarityBorderImage.sprite = borderSprite;
                        RarityBorderImage.color = Color.white;
                    }
                    else
                    {
                        RarityBorderImage.color = borderColor;
                    }
                }

                if (RarityBackgroundImage != null)
                {
                    if (backgroundSprite != null)
                    {
                        RarityBackgroundImage.sprite = backgroundSprite;
                        RarityBackgroundImage.color = Color.white;
                    }
                    else
                    {
                        RarityBackgroundImage.color = backgroundColor;
                    }
                }
            }

            public void ClearAllyMetadata()
            {
                if (LevelText != null)
                    LevelText.text = string.Empty;

                SetOptionalIcon(RoleIconImage, null);
                SetOptionalIcon(ElementIconImage, null);

                

                if (RarityBorderImage != null)
                {
                    RarityBorderImage.sprite = null;
                    RarityBorderImage.color = Color.white;
                }

                if (RarityBackgroundImage != null)
                {
                    RarityBackgroundImage.sprite = null;
                    RarityBackgroundImage.color = new Color(1f, 1f, 1f, 0.25f);
                }
            }

            private void EnsureDelayedHpSlider()
            {
                if (HPDelayedSlider != null || HPSlider == null)
                    return;

                var parent = HPSlider.transform.parent;
                if (parent == null)
                    return;

                var delayedObj = UnityEngine.Object.Instantiate(HPSlider.gameObject, parent);
                delayedObj.name = HPSlider.gameObject.name + "_DelayedAuto";

                HPDelayedSlider = delayedObj.GetComponent<Slider>();
                if (HPDelayedSlider == null)
                    return;

                var delayedFill = HPDelayedSlider.fillRect != null ? HPDelayedSlider.fillRect.GetComponent<Image>() : null;
                if (delayedFill != null)
                    delayedFill.color = new Color(1f, 0.85f, 0.55f, 0.95f);

                int baseIndex = HPSlider.transform.GetSiblingIndex();
                delayedObj.transform.SetSiblingIndex(baseIndex);
                HPSlider.transform.SetSiblingIndex(baseIndex + 1);
            }

            private static void LockSliderInput(Slider slider)
            {
                if (slider == null) return;

                slider.interactable = false;
                var nav = slider.navigation;
                nav.mode = Navigation.Mode.None;
                slider.navigation = nav;
            }

            private static void SetOptionalIcon(Image target, Sprite sprite)
            {
                if (target == null)
                    return;

                target.sprite = sprite;
                target.color = sprite == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
            }

            public void AnimateHP(float newPercent)
            {
                if (HPSlider == null) return;

                newPercent = Mathf.Clamp01(newPercent);
                float oldPercent = _lastHPPercent;
                _lastHPPercent = newPercent;

                HPSlider.DOKill();
                HPSlider.value = newPercent;

                if (HPDelayedSlider != null)
                {
                    HPDelayedSlider.DOKill();

                    if (newPercent < oldPercent)
                    {
                        HPDelayedSlider.value = oldPercent;
                        HPDelayedSlider.DOValue(newPercent, 0.5f).SetEase(Ease.OutCubic);
                    }
                    else
                    {
                        HPDelayedSlider.value = newPercent;
                    }
                }

                SetHPText(newPercent);
            }

            public void AnimateMP(int currentMana, int maxMana)
            {
                if (MPSlider == null) return;
                float ratio = maxMana > 0 ? (float)currentMana / maxMana : 0f;
                MPSlider.DOValue(ratio, 0.3f).SetEase(Ease.OutCubic);
            }

            public void SetVisible(bool visible)
            {
                var root = SlotRootObject;
                if (root != null)
                    root.SetActive(visible);
            }

            public void SetHighlight(bool isCurrentTarget)
            {
                if (SlotGroup != null)
                    SlotGroup.alpha = 1f;

                if (SlotRectTransform != null)
                    SlotRectTransform.localScale = isCurrentTarget ? Vector3.one * 1.08f : Vector3.one;
            }

            public void SetScreenPosition(Vector3 screenPosition)
            {
                if (SlotRectTransform != null)
                    SlotRectTransform.position = screenPosition;
            }

            public void SetDead()
            {
                _lastHPPercent = 0f;

                if (HPSlider != null)
                {
                    HPSlider.DOKill();
                    HPSlider.value = 0f;
                }

                if (HPDelayedSlider != null)
                {
                    HPDelayedSlider.DOKill();
                    HPDelayedSlider.DOValue(0f, 0.5f).SetEase(Ease.OutCubic);
                }

                SetHPText(0f);
                SlotGroup.DOFade(0.4f, 0.5f).SetDelay(0.2f);
            }

            public void SetEffectIcons(IReadOnlyList<string> effectIds, Func<string, Sprite> spriteResolver)
            {
                EnsureEffectRoot();
                ClearEffectIcons();

                if (EffectRoot == null || effectIds == null || effectIds.Count == 0)
                {
                    return;
                }

                var displayed = new HashSet<string>();
                for (var i = 0; i < effectIds.Count; i++)
                {
                    var effectId = effectIds[i];
                    if (string.IsNullOrWhiteSpace(effectId) || !displayed.Add(effectId))
                    {
                        continue;
                    }

                    var sprite = spriteResolver != null ? spriteResolver(effectId) : null;
                    if (sprite == null)
                    {
                        continue;
                    }

                    var iconObject = new GameObject($"Effect_{effectId}", typeof(RectTransform), typeof(Image));
                    iconObject.transform.SetParent(EffectRoot, false);

                    var rect = iconObject.GetComponent<RectTransform>();
                    rect.sizeDelta = new Vector2(28f, 28f);

                    var image = iconObject.GetComponent<Image>();
                    image.sprite = sprite;
                    image.color = Color.white;
                    image.preserveAspect = true;

                    _spawnedEffectIcons.Add(iconObject);
                }
            }

            private void EnsureEffectRoot()
            {
                if (EffectRoot != null)
                {
                    return;
                }

                var root = SlotRootObject;
                if (root == null)
                {
                    return;
                }

                var effectRootObject = new GameObject("EffectRoot", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                effectRootObject.transform.SetParent(root.transform, false);

                EffectRoot = effectRootObject.GetComponent<RectTransform>();
                EffectRoot.anchorMin = new Vector2(1f, 1f);
                EffectRoot.anchorMax = new Vector2(1f, 1f);
                EffectRoot.pivot = new Vector2(1f, 1f);
                EffectRoot.anchoredPosition = new Vector2(-8f, -8f);
                EffectRoot.sizeDelta = new Vector2(140f, 32f);

                var layout = effectRootObject.GetComponent<HorizontalLayoutGroup>();
                layout.spacing = 4f;
                layout.childAlignment = TextAnchor.MiddleRight;
                layout.childControlWidth = false;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
            }

            private void ClearEffectIcons()
            {
                for (var i = 0; i < _spawnedEffectIcons.Count; i++)
                {
                    if (_spawnedEffectIcons[i] != null)
                    {
                        UnityEngine.Object.Destroy(_spawnedEffectIcons[i]);
                    }
                }

                _spawnedEffectIcons.Clear();
            }

            private void SetHPText(float percent)
            {
                if (HPText != null)
                    HPText.text = $"{Mathf.RoundToInt(percent * _maxHP)}/{Mathf.RoundToInt(_maxHP)}";
            }

            private RectTransform SlotRectTransform => HPSlider != null
                ? HPSlider.gameObject.transform.parent as RectTransform
                : null;

            private GameObject SlotRootObject => HPSlider != null
                ? HPSlider.gameObject.transform.parent.gameObject
                : null;
        }

        // ─── Inspector ────────────────────────────────────────────────────
        [Header("Ally Slots (max 3)")]
        [SerializeField] private HUDSlot[] _allySlots  = new HUDSlot[3];

        [Header("Enemy Slots (max 3)")]
        [SerializeField] private HUDSlot[] _enemySlots = new HUDSlot[3];

        [Header("Enemy Floating HUD")]
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private float _enemyWorldOffsetY = 1.2f;
        [SerializeField] private RectTransform _enemyHudCanvasRoot;
        [SerializeField] private float _enemyDamageHudDuration = 1.6f;

        // ─── Slot Lookup ──────────────────────────────────────────────────
        private readonly Dictionary<string, HUDSlot> _slotMap = new();
        private readonly Dictionary<string, Sprite> _portraitCache = new();
        private readonly HashSet<string> _enemyEntityIds = new();
        private readonly HashSet<string> _targetingEnemyIds = new();
        private readonly Dictionary<string, float> _enemyVisibleUntil = new();
        private readonly HashSet<string> _missingEffectIconWarnings = new();
        private bool _isEnemyTargetingActive;
        private string _currentTargetEnemyId;
        private bool _isEnemyTurnActive;
        private string _enemyTurnActorId;
        private Canvas _rootCanvas;

        private const string EffectIconResourceFolder = "UI/Effects";

        // ──────────────────────────────────────────────────────────────────
        #region Initialization

        private void Awake()
        {
            _rootCanvas = GetComponentInParent<Canvas>();
            if (_enemyHudCanvasRoot == null)
                _enemyHudCanvasRoot = _rootCanvas != null ? _rootCanvas.transform as RectTransform : null;
        }

        /// <summary>
        /// Gán entity vào các slot và bắt đầu lắng nghe events.
        /// </summary>
        public void InitializeSlots(List<CombatEntity> allies, List<CombatEntity> enemies)
        {
            _slotMap.Clear();
            _enemyEntityIds.Clear();
            _targetingEnemyIds.Clear();
            _enemyVisibleUntil.Clear();
            _isEnemyTargetingActive = false;
            _currentTargetEnemyId = null;
            _isEnemyTurnActive = false;
            _enemyTurnActorId = null;

            InitGroup(allies,  _allySlots, isEnemyGroup: false);
            InitGroup(enemies, _enemySlots, isEnemyGroup: true);
            RefreshAllEffectIcons();

            SubscribeEvents();
        }

        /// <summary>
        /// Update enemy slots khi wave progression xảy ra (gọi từ CombatUIController).
        /// </summary>
        public void UpdateEnemySlots(List<CombatEntity> newEnemies)
        {
            // Remove old enemy IDs from slot map
            foreach (var oldEnemyId in _enemyEntityIds)
            {
                _slotMap.Remove(oldEnemyId);
            }

            // Clear old enemy data
            _enemyEntityIds.Clear();
            _targetingEnemyIds.Clear();
            _enemyVisibleUntil.Clear();

            // Reset all enemy slots
            for (int i = 0; i < _enemySlots.Length; i++)
            {
                var slot = _enemySlots[i];
                
                if (i < newEnemies.Count && newEnemies[i] != null)
                {
                    Sprite portrait = ResolvePortraitSprite(newEnemies[i]);
                    int maxMP = TTCS.Combat.Managers.SkillManager.Instance != null
                        ? TTCS.Combat.Managers.SkillManager.Instance.GetMaxMana(newEnemies[i].ID)
                        : 100;
                    
                    // Kill any running animations on the sliders
                    if (slot.HPSlider != null)
                        slot.HPSlider.DOKill();
                    if (slot.HPDelayedSlider != null)
                        slot.HPDelayedSlider.DOKill();
                    
                    // Reinitialize this slot with new enemy
                    slot.Initialize(newEnemies[i], maxMP, portrait);
                    slot.ClearAllyMetadata();
                    _slotMap[newEnemies[i].ID] = slot;
                    _enemyEntityIds.Add(newEnemies[i].ID);
                    RefreshEffectIcons(newEnemies[i].ID);
                    
                    slot.SetVisible(false);
                    if (slot.MPSlider != null)
                        slot.MPSlider.gameObject.SetActive(false);
                }
                else
                {
                    // Hide unused slots
                    if (slot.HPSlider != null)
                        slot.HPSlider.gameObject.transform.parent.gameObject.SetActive(false);
                }
            }

            Log($"BattleHUD: Updated enemy slots to {newEnemies.Count} enemies.", LogCategory.UI);
        }

        private void InitGroup(List<CombatEntity> entities, HUDSlot[] slots, bool isEnemyGroup)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (i < entities.Count && entities[i] != null)
                {
                    var entity = entities[i];
                    Sprite portrait = ResolvePortraitSprite(entity);
                    int maxMP = TTCS.Combat.Managers.SkillManager.Instance != null
                        ? TTCS.Combat.Managers.SkillManager.Instance.GetMaxMana(entity.ID)
                        : 100;
                    slots[i].Initialize(entity, maxMP, portrait);
                    _slotMap[entity.ID] = slots[i];

                    if (!isEnemyGroup)
                    {
                        ApplyAllySlotMetadata(slots[i], entity);
                    }
                    else
                    {
                        slots[i].ClearAllyMetadata();
                    }

                    RefreshEffectIcons(entity.ID);

                    if (isEnemyGroup)
                    {
                        _enemyEntityIds.Add(entity.ID);
                        slots[i].SetVisible(false);
                        // Bỏ hiển thị thanh mana của enemy
                        if (slots[i].MPSlider != null)
                            slots[i].MPSlider.gameObject.SetActive(false);
                    }
                }
                else
                {
                    // Ẩn slot thừa
                    if (slots[i].HPSlider != null)
                        slots[i].HPSlider.gameObject.transform.parent.gameObject.SetActive(false);
                }
            }
        }

        public void BeginEnemyTargeting(List<string> candidateEnemyIds, string currentTargetId)
        {
            _isEnemyTargetingActive = true;
            _targetingEnemyIds.Clear();

            if (candidateEnemyIds != null)
            {
                foreach (var id in candidateEnemyIds)
                {
                    if (!string.IsNullOrWhiteSpace(id))
                        _targetingEnemyIds.Add(id);
                }
            }

            _currentTargetEnemyId = currentTargetId;
            RefreshEnemyVisibility();
            UpdateEnemyFloatingHUDPosition();
        }

        public void UpdateEnemyTargeting(string currentTargetId)
        {
            if (!_isEnemyTargetingActive) return;

            _currentTargetEnemyId = currentTargetId;
            RefreshEnemyVisibility();
            UpdateEnemyFloatingHUDPosition();
        }

        public void EndEnemyTargeting()
        {
            _isEnemyTargetingActive = false;
            _currentTargetEnemyId = null;
            _targetingEnemyIds.Clear();

            RefreshEnemyVisibility();
        }

        private void OnTurnStarted(TurnStartedEvent e)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.EntityId)) return;
            if (!_enemyEntityIds.Contains(e.EntityId)) return;

            _isEnemyTurnActive = true;
            _enemyTurnActorId = e.EntityId;
            RefreshEnemyVisibility();
        }

        private void OnTurnEnded(TurnEndedEvent e)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.EntityId)) return;
            if (!_enemyEntityIds.Contains(e.EntityId)) return;

            _isEnemyTurnActive = false;
            _enemyTurnActorId = null;
            RefreshEnemyVisibility();
        }

        private void MarkEnemyVisibleByDamage(string enemyId)
        {
            if (string.IsNullOrWhiteSpace(enemyId) || !_enemyEntityIds.Contains(enemyId))
                return;

            float until = Time.time + Mathf.Max(0.1f, _enemyDamageHudDuration);
            if (_enemyVisibleUntil.TryGetValue(enemyId, out var oldUntil))
                _enemyVisibleUntil[enemyId] = Mathf.Max(oldUntil, until);
            else
                _enemyVisibleUntil[enemyId] = until;

            RefreshEnemyVisibility();
        }

        private bool IsEnemyVisibleByDamage(string enemyId)
        {
            return _enemyVisibleUntil.TryGetValue(enemyId, out var until) && Time.time <= until;
        }

        private bool ShouldShowEnemyHud(string enemyId)
        {
            if (_isEnemyTargetingActive && _targetingEnemyIds.Contains(enemyId))
                return true;

            if (_isEnemyTurnActive && enemyId == _enemyTurnActorId)
                return true;

            if (IsEnemyVisibleByDamage(enemyId))
                return true;

            return false;
        }

        private void RefreshEnemyVisibility()
        {
            foreach (var id in _enemyEntityIds)
            {
                if (_slotMap.TryGetValue(id, out var slot))
                {
                    bool shouldShow = ShouldShowEnemyHud(id);
                    slot.SetVisible(shouldShow);

                    if (shouldShow)
                    {
                        bool isCurrentTarget = _isEnemyTargetingActive && id == _currentTargetEnemyId;
                        slot.SetHighlight(isCurrentTarget);
                    }
                }
            }
        }

        private void LateUpdate()
        {
            RefreshEnemyVisibility();
            UpdateEnemyFloatingHUDPosition();
        }

        private void UpdateEnemyFloatingHUDPosition()
        {
            if (_enemyEntityIds.Count == 0)
                return;

            var cam = _worldCamera != null ? _worldCamera : Camera.main;
            if (cam == null || _enemyHudCanvasRoot == null)
                return;

            Camera canvasCamera = null;
            if (_rootCanvas != null && _rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                canvasCamera = _rootCanvas.worldCamera;

            foreach (var enemyId in _enemyEntityIds)
            {
                if (!_slotMap.TryGetValue(enemyId, out var slot))
                    continue;

                if (!ShouldShowEnemyHud(enemyId))
                    continue;

                Vector3 worldPos = CombatUIController.Instance?.GetEntityWorldPos(enemyId) ?? Vector3.zero;
                worldPos.y += _enemyWorldOffsetY;

                Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
                if (screenPos.z <= 0f)
                    continue;

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        _enemyHudCanvasRoot,
                        screenPos,
                        canvasCamera,
                        out var localPoint))
                {
                    slot.SetScreenPosition(_enemyHudCanvasRoot.TransformPoint(localPoint));
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

        private void ApplyAllySlotMetadata(HUDSlot slot, CombatEntity entity)
        {
            if (slot == null || entity == null)
            {
                return;
            }

            if (!(entity is Character character) || DataManager.Instance == null)
            {
                slot.ClearAllyMetadata();
                return;
            }

            var data = DataManager.Instance.LoadCharacter(character.CharacterId);
            if (data == null)
            {
                slot.ClearAllyMetadata();
                return;
            }

            var level = ResolveCharacterLevel(character, data);
            var rarity = data.metadata?.rarity ?? "R";
            var roleIcon = LoadRoleIconSprite(data.metadata?.roleTag);
            var elementIcon = LoadElementIconSprite(data.metadata?.element);
            var borderSprite = LoadRarityBorderSprite(rarity);
            var backgroundSprite = LoadRarityBackgroundSprite(rarity);

            slot.SetAllyMetadata(
                level,
                rarity,
                roleIcon,
                elementIcon,
                borderSprite,
                backgroundSprite,
                GetRarityBorderColor(rarity),
                GetRarityBackgroundColor(rarity));
        }

        private static int ResolveCharacterLevel(Character character, CharacterDataModel data)
        {
            var baseLevel = data?.baseStats?.level ?? 1;
            var saveLevel = SaveManager.Instance?.CurrentSave?.GetCharacterLevel(character.CharacterId) ?? 0;
            return Mathf.Max(1, saveLevel > 0 ? saveLevel : baseLevel);
        }

        private static Sprite LoadRoleIconSprite(string roleTag)
        {
            var key = NormalizeTag(roleTag);
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            var candidates = new[]
            {
                $"UI/Role/icon_role_{key}",
                $"Icons/Role/icon_role_{key}",
                $"Role/{key}",
                $"UI/Role/{key}",
                $"Icons/{key}"
            };

            for (var i = 0; i < candidates.Length; i++)
            {
                var sprite = Resources.Load<Sprite>(candidates[i]);
                if (sprite != null)
                {
                    return sprite;
                }
            }

            return null;
        }

        private static Sprite LoadElementIconSprite(string elementTag)
        {
            var key = NormalizeTag(elementTag);
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            var candidates = new[]
            {
                $"UI/Element/icon_element_{key}",
                $"Icons/Element/icon_element_{key}",
                $"Element/{key}",
                $"UI/Element/{key}",
                $"Icons/{key}"
            };

            for (var i = 0; i < candidates.Length; i++)
            {
                var sprite = Resources.Load<Sprite>(candidates[i]);
                if (sprite != null)
                {
                    return sprite;
                }
            }

            return null;
        }

        private static Sprite LoadRarityBorderSprite(string rarity)
        {
            var key = NormalizeTag(rarity);
            var candidates = string.IsNullOrWhiteSpace(key)
                ? new[] { "UI/Border/common_border" }
                : new[]
                {
                    $"UI/Border/{key}_border",
                    "UI/Border/common_border"
                };

            for (var i = 0; i < candidates.Length; i++)
            {
                var sprite = Resources.Load<Sprite>(candidates[i]);
                if (sprite != null)
                {
                    return sprite;
                }
            }

            return null;
        }

        private static Sprite LoadRarityBackgroundSprite(string rarity)
        {
            var key = NormalizeTag(rarity);
            var candidates = string.IsNullOrWhiteSpace(key)
                ? new[] { "UI/Border/common_bg" }
                : new[]
                {
                    $"UI/Border/{key}_bg",
                    "UI/Border/common_bg"
                };

            for (var i = 0; i < candidates.Length; i++)
            {
                var sprite = Resources.Load<Sprite>(candidates[i]);
                if (sprite != null)
                {
                    return sprite;
                }
            }

            return null;
        }

        private static Color GetRarityBorderColor(string rarity)
        {
            if (string.Equals(rarity, "UR", StringComparison.OrdinalIgnoreCase))
            {
                return HexToColor("#E61919");
            }

            if (string.Equals(rarity, "SSR", StringComparison.OrdinalIgnoreCase))
            {
                return HexToColor("#FFD700");
            }

            if (string.Equals(rarity, "SR", StringComparison.OrdinalIgnoreCase))
            {
                return HexToColor("#FF007F");
            }

            if (string.Equals(rarity, "R", StringComparison.OrdinalIgnoreCase))
            {
                return HexToColor("#00F0FF");
            }

            return Color.white;
        }

        private static Color GetRarityBackgroundColor(string rarity)
        {
            if (string.Equals(rarity, "UR", StringComparison.OrdinalIgnoreCase))
            {
                return HexToColor("#3D0000AA");
            }

            if (string.Equals(rarity, "SSR", StringComparison.OrdinalIgnoreCase))
            {
                return HexToColor("#FFB30066");
            }

            if (string.Equals(rarity, "SR", StringComparison.OrdinalIgnoreCase))
            {
                return HexToColor("#FF007F66");
            }

            if (string.Equals(rarity, "R", StringComparison.OrdinalIgnoreCase))
            {
                return HexToColor("#00F0FF66");
            }

            return new Color(1f, 1f, 1f, 0.25f);
        }

        private static string NormalizeTag(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().ToLowerInvariant();
        }

        private static Color HexToColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out var color))
            {
                return color;
            }

            return Color.white;
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

        private void RefreshAllEffectIcons()
        {
            foreach (var entry in _slotMap)
            {
                RefreshEffectIcons(entry.Key);
            }
        }

        private void RefreshEffectIcons(string entityId)
        {
            if (string.IsNullOrWhiteSpace(entityId) || !_slotMap.TryGetValue(entityId, out var slot))
            {
                return;
            }

            var effectIds = CombatUIController.Instance?.GetEntityActiveEffectIds(entityId);
            slot.SetEffectIcons(effectIds, LoadEffectIconSprite);
        }

        private Sprite LoadEffectIconSprite(string effectId)
        {
            if (string.IsNullOrWhiteSpace(effectId))
            {
                return null;
            }

            var normalized = NormalizeTag(effectId);
            var candidates = new[]
            {
                $"{EffectIconResourceFolder}/{normalized}",
                $"{EffectIconResourceFolder}/effect_{normalized}"
            };

            for (var i = 0; i < candidates.Length; i++)
            {
                var sprite = Resources.Load<Sprite>(candidates[i]);
                if (sprite != null)
                {
                    return sprite;
                }
            }

            if (_missingEffectIconWarnings.Add(normalized))
            {
                LogWarning($"BattleHUD: Missing effect icon for '{effectId}'. Put sprite at Resources/{EffectIconResourceFolder}/{normalized}.png", LogCategory.UI);
            }

            return null;
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
            bus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            bus.Subscribe<TurnEndedEvent>(OnTurnEnded);
            bus.Subscribe<StatusEffectAppliedEvent>(OnStatusEffectApplied);
            bus.Subscribe<StatusEffectRemovedEvent>(OnStatusEffectRemoved);
            
        }

        private void UnsubscribeEvents()
        {
            var bus = EventBus.Instance;
            bus.Unsubscribe<DamageTakenEvent>(OnDamageTaken);
            bus.Unsubscribe<HealingReceivedEvent>(OnHealingReceived);
            bus.Unsubscribe<EntityDeathEvent>(OnEntityDeath);
            bus.Unsubscribe<ManaChangedEvent>(OnManaChanged);
            bus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            bus.Unsubscribe<TurnEndedEvent>(OnTurnEnded);
            bus.Unsubscribe<StatusEffectAppliedEvent>(OnStatusEffectApplied);
            bus.Unsubscribe<StatusEffectRemovedEvent>(OnStatusEffectRemoved);
            
        }

        private void OnDamageTaken(DamageTakenEvent e)
        {
            
            if (!_slotMap.TryGetValue(e.TargetId, out var slot)) return;
            

            // Tính percent mới dựa trên entity thực — cần lấy từ CombatUIController
            float newPercent = CombatUIController.Instance != null
                ? CombatUIController.Instance.GetEntityHPPercent(e.TargetId)
                : Mathf.Max(0f, slot.HPSlider.value - 0.05f);
            

             

            slot.AnimateHP(newPercent);

            // Enemy nào bị dính damage sẽ hiện HUD trong một khoảng thời gian ngắn.
            MarkEnemyVisibleByDamage(e.TargetId);
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
            {
                slot.SetDead();
                slot.SetEffectIcons(null, LoadEffectIconSprite);
            }

            _enemyVisibleUntil.Remove(e.EntityId);
            if (_enemyTurnActorId == e.EntityId)
            {
                _enemyTurnActorId = null;
                _isEnemyTurnActive = false;
            }
            RefreshEnemyVisibility();
        }

        private void OnManaChanged(ManaChangedEvent e)
        {
            // Bỏ qua cập nhật mana cho enemy
            if (_enemyEntityIds.Contains(e.EntityId))
                return;

            if (_slotMap.TryGetValue(e.EntityId, out var slot))
                slot.AnimateMP(e.CurrentMana, e.MaxMana);
        }

        private void OnStatusEffectApplied(StatusEffectAppliedEvent e)
        {
            RefreshEffectIcons(e != null ? e.TargetId : null);
        }

        private void OnStatusEffectRemoved(StatusEffectRemovedEvent e)
        {
            RefreshEffectIcons(e != null ? e.TargetId : null);
        }

        

        private void OnDestroy()
        {
            UnsubscribeEvents();
            EndEnemyTargeting();
        }

        #endregion
    }
}

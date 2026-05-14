using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using TTCS.Combat.Entities;
using TTCS.Combat.Managers;
using TTCS.Core.Data;
using TTCS.Core.Events;
using TTCS.Debugging;
using static TTCS.Debugging.DebugLogger;

namespace TTCS.UI.Combat
{
    /// <summary>
    /// 🔵 Dev A - Turn Order Display
    /// Hiển thị hàng đợi lượt (tối đa 8 slot) dựa trên TurnManager timeline preview.
    ///
    /// Subscribe: TimelineUpdatedEvent → rebuild slot list.
    /// </summary>
    public class TurnOrderDisplay : MonoBehaviour
    {
        // ─── Inspector ────────────────────────────────────────────────────
        [SerializeField] private TurnOrderSlot _slotPrefab;
        [SerializeField] private Transform     _slotContainer;
        [SerializeField] private int           _previewCount = 5;
        [SerializeField] private Sprite        _playerFrameSprite;
        [SerializeField] private Sprite        _enemyFrameSprite;

        // ─── Pool ─────────────────────────────────────────────────────────
        private readonly List<TurnOrderSlot> _pool = new();

        // ─── Entity Cache ─────────────────────────────────────────────────
        private readonly Dictionary<string, Sprite> _entityPortraitCache = new();
        private readonly Dictionary<string, bool> _entityIsPlayerCache = new();

        // ──────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            BuildPool();
        }

        private void OnEnable()
        {
            EventBus.Instance.Subscribe<TimelineUpdatedEvent>(OnTimelineUpdated);
        }

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<TimelineUpdatedEvent>(OnTimelineUpdated);
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Initialization

        /// <summary>
        /// Đăng ký entity để TurnOrderDisplay biết portrait.
        /// Gọi từ CombatUIController.Initialize() trước khi battle bắt đầu.
        /// </summary>
        public void RegisterEntity(CombatEntity entity)
        {
            if (entity == null || string.IsNullOrWhiteSpace(entity.ID)) return;
            _entityPortraitCache[entity.ID] = ResolvePortraitSprite(entity);
            _entityIsPlayerCache[entity.ID] = entity.IsPlayer;
        }

        private void BuildPool()
        {
            if (_slotPrefab == null)
            {
                LogWarning("TurnOrderDisplay: SlotPrefab chưa được gán!", LogCategory.UI);
                return;
            }

            for (int i = 0; i < _previewCount; i++)
            {
                var slot = Instantiate(_slotPrefab, _slotContainer);
                slot.gameObject.SetActive(false);
                _pool.Add(slot);
            }
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Refresh

        private void OnTimelineUpdated(TimelineUpdatedEvent _)
        {
            Refresh();
        }

        /// <summary>Rebuild toàn bộ turn order display từ TurnManager preview.</summary>
        public void Refresh()
        {
            if (TurnManager.Instance == null) return;

            var preview      = TurnManager.Instance.GetTimelinePreview(_previewCount);
            string currentId = TurnManager.Instance.CurrentActor;

            // Ẩn tất cả trước
            foreach (var slot in _pool)
                slot.gameObject.SetActive(false);

            for (int i = 0; i < preview.Count && i < _pool.Count; i++)
            {
                string entityId = preview[i];
                _entityPortraitCache.TryGetValue(entityId, out var portrait);
                _entityIsPlayerCache.TryGetValue(entityId, out var isPlayer);

                bool isCurrentActor =  i == 0;
                Sprite frameSprite = isPlayer ? _playerFrameSprite : _enemyFrameSprite;
                _pool[i].SetData(portrait, frameSprite, isCurrentActor);
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
            else if (entity is Enemy enemy)
            {
                var data = DataManager.Instance.LoadEnemy(enemy.EnemyTemplateId);
                portraitPath = data?.visual?.portraitPath;
                if (string.IsNullOrWhiteSpace(portraitPath))
                    portraitPath = data?.visual?.spritePath;
            }

            if (string.IsNullOrWhiteSpace(portraitPath))
                return null;

            var sprite = DataManager.Instance.LoadCharacterPortraitSprite(portraitPath);
            if (sprite != null)
                return sprite;

            sprite = LoadCharacterPortraitFromProjectPath(portraitPath);
            if (sprite != null)
                return sprite;

            string resourcesPath = NormalizeResourcesPath(portraitPath);
            if (string.IsNullOrWhiteSpace(resourcesPath))
                return null;

            return Resources.Load<Sprite>(resourcesPath);
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

            if (normalized.StartsWith("Characters/", StringComparison.OrdinalIgnoreCase)
                || normalized.StartsWith("Enemies/", StringComparison.OrdinalIgnoreCase))
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
    }
}

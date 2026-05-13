using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TTCS.Combat.Managers;
using TTCS.Core.Events;
using TTCS.Visual;

namespace TTCS.Visual.VFX
{
    /// <summary>
    /// Attach this component to a character/enemy prefab root.
    /// It owns only that prefab's skill target VFX configuration.
    /// </summary>
    [DisallowMultipleComponent]
    public class CharacterSkillTargetVFXController : MonoBehaviour
    {
        public enum SpawnMode
        {
            SpawnAtTarget,
            FlyFromCasterToTarget
        }

        public enum AnchorPoint
        {
            HitAnchor,
            HeadAnchor,
            Root
        }

        [Serializable]
        public class VFXLayer
        {
            [Header("Prefab")]
            public string label = "VFX Layer";
            public GameObject prefab;
            public SpawnMode spawnMode = SpawnMode.SpawnAtTarget;

            [Header("Timing")]
            [Min(0f)] public float delay = 0f;
            [Min(0.01f)] public float lifetime = 1f;

            [Header("Anchor And Offset")]
            public AnchorPoint casterAnchor = AnchorPoint.HitAnchor;
            public AnchorPoint targetAnchor = AnchorPoint.HitAnchor;
            public Vector3 startOffset = Vector3.zero;
            public Vector3 targetOffset = Vector3.zero;
            public bool parentToTarget = false;

            [Header("Fly Mode")]
            [Min(0.01f)] public float travelDuration = 0.35f;
            public AnimationCurve travelCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            public bool rotateTowardTravelDirection = false;
        }

        [Serializable]
        public class SkillVFXConfig
        {
            [Tooltip("Must match the skill JSON id used by this character, for example skill_hodr_piercing_round.")]
            public string skillId;
            public List<VFXLayer> layers = new List<VFXLayer>();
        }

        [Header("Owner")]
        [Tooltip("Leave empty to auto-find CharacterView/EnemyView on this prefab.")]
        [SerializeField] private CharacterView _ownerView;

        [Header("This Character's Skill Target VFX")]
        [Tooltip("Only configure skills owned by this prefab here.")]
        [SerializeField] private List<SkillVFXConfig> _skills = new List<SkillVFXConfig>();

        private void Awake()
        {
            if (_ownerView == null)
                _ownerView = GetComponent<CharacterView>();

            if (_ownerView == null)
                _ownerView = GetComponentInParent<CharacterView>();
        }

        private void OnEnable()
        {
            EventBus.Instance.Subscribe<SkillCastEvent>(OnSkillCast);
        }

        private void OnDisable()
        {
            if (EventBus.Instance == null)
                return;

            EventBus.Instance.Unsubscribe<SkillCastEvent>(OnSkillCast);
        }

        private void OnSkillCast(SkillCastEvent e)
        {
            if (e == null || _ownerView == null)
                return;

            if (!string.Equals(e.CasterId, _ownerView.EntityId, StringComparison.Ordinal))
                return;

            SkillVFXConfig config = FindConfig(e.SkillId);
            if (config == null || config.layers == null || config.layers.Count == 0)
                return;

            if (e.TargetIds == null || e.TargetIds.Length == 0)
                return;

            for (int targetIndex = 0; targetIndex < e.TargetIds.Length; targetIndex++)
            {
                CharacterView targetView = GetRegisteredView(e.TargetIds[targetIndex]);
                if (targetView == null)
                    continue;

                for (int layerIndex = 0; layerIndex < config.layers.Count; layerIndex++)
                {
                    VFXLayer layer = config.layers[layerIndex];
                    if (layer == null || layer.prefab == null)
                        continue;

                    StartCoroutine(SpawnLayerRoutine(targetView, layer));
                }
            }
        }

        private SkillVFXConfig FindConfig(string skillId)
        {
            if (string.IsNullOrWhiteSpace(skillId) || _skills == null)
                return null;

            for (int i = 0; i < _skills.Count; i++)
            {
                SkillVFXConfig config = _skills[i];
                if (config == null)
                    continue;

                if (string.Equals(config.skillId, skillId, StringComparison.Ordinal))
                    return config;
            }

            return null;
        }

        private IEnumerator SpawnLayerRoutine(CharacterView targetView, VFXLayer layer)
        {
            if (layer.delay > 0f)
                yield return new WaitForSeconds(layer.delay);

            Vector3 targetPosition = ResolveAnchor(targetView, layer.targetAnchor) + layer.targetOffset;
            Vector3 startPosition = layer.spawnMode == SpawnMode.FlyFromCasterToTarget
                ? ResolveAnchor(_ownerView, layer.casterAnchor) + layer.startOffset
                : targetPosition;

            Transform parent = layer.parentToTarget && targetView != null ? targetView.transform : transform;
            GameObject instance = Instantiate(layer.prefab, startPosition, Quaternion.identity, parent);
            instance.name = $"{layer.prefab.name}_{layer.spawnMode}";

            if (layer.parentToTarget)
                instance.transform.position = startPosition;

            if (layer.spawnMode == SpawnMode.FlyFromCasterToTarget)
                yield return MoveToTarget(instance.transform, startPosition, targetPosition, layer);

            Destroy(instance, Mathf.Max(0.01f, layer.lifetime));
        }

        private IEnumerator MoveToTarget(
            Transform instance,
            Vector3 startPosition,
            Vector3 targetPosition,
            VFXLayer layer)
        {
            if (instance == null)
                yield break;

            float duration = Mathf.Max(0.01f, layer.travelDuration);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (instance == null)
                    yield break;

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curved = layer.travelCurve != null ? layer.travelCurve.Evaluate(t) : t;
                instance.position = Vector3.LerpUnclamped(startPosition, targetPosition, curved);

                if (layer.rotateTowardTravelDirection)
                    RotateToward(instance, targetPosition - startPosition);

                yield return null;
            }

            if (instance != null)
                instance.position = targetPosition;
        }

        private static CharacterView GetRegisteredView(string entityId)
        {
            if (ActionAnimationController.Instance == null || string.IsNullOrEmpty(entityId))
                return null;

            return ActionAnimationController.Instance.GetViewForEntity(entityId);
        }

        private static Vector3 ResolveAnchor(CharacterView view, AnchorPoint anchor)
        {
            if (view == null)
                return Vector3.zero;

            switch (anchor)
            {
                case AnchorPoint.HeadAnchor:
                    return view.HeadAnchorPosition;
                case AnchorPoint.Root:
                    return view.WorldPosition;
                default:
                    return view.HitAnchorPosition;
            }
        }

        private static void RotateToward(Transform instance, Vector3 direction)
        {
            if (instance == null || direction.sqrMagnitude < 0.0001f)
                return;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            instance.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}

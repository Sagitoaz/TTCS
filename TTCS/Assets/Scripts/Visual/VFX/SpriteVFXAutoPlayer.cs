using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TTCS.Visual.VFX
{
    /// <summary>
    /// Sprite VFX player for "toggle Active in Animation and auto-play".
    /// Use this on each VFX prefab (slash arc, cast glow, rune ring, etc.).
    /// </summary>
    [DisallowMultipleComponent]
    public class SpriteVFXAutoPlayer : MonoBehaviour
    {
        [Serializable]
        private class ShaderFloatTrack
        {
            public bool enabled = true;
            public string propertyName = "_EmissionScale";
            public float from = 1f;
            public float to = 1f;
            public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        }

        [Serializable]
        private class ShaderColorTrack
        {
            public bool enabled = false;
            public string propertyName = "_Color";
            public Gradient gradient = new Gradient();
        }

        [Serializable]
        private class TimedChildActivation
        {
            public GameObject target;
            [Range(0f, 1f)] public float normalizedTime = 0f;
            public bool autoDisableWhenFinished = true;
        }

        [Header("Playback")]
        [SerializeField] private bool _playOnEnable = true;
        [SerializeField] private bool _autoDeactivateWhenFinished = true;
        [SerializeField] private bool _loop = false;
        [SerializeField] private bool _useUnscaledTime = false;
        [SerializeField] private float _startDelay = 0f;
        [SerializeField] private float _duration = 0.3f;

        [Header("Targets")]
        [SerializeField] private Transform _animatedRoot;
        [SerializeField] private Transform _fillTargetX;
        [SerializeField] private SpriteRenderer[] _renderers;
        [SerializeField] private bool _autoCollectRenderers = true;

        [Header("Transform - Position")]
        [SerializeField] private bool _animateLocalPosition = false;
        [SerializeField] private Vector3 _localPositionFrom = Vector3.zero;
        [SerializeField] private Vector3 _localPositionTo = Vector3.zero;
        [SerializeField] private AnimationCurve _localPositionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Transform - Scale")]
        [SerializeField] private bool _animateLocalScale = true;
        [SerializeField] private Vector3 _localScaleFrom = Vector3.one;
        [SerializeField] private Vector3 _localScaleTo = Vector3.one;
        [SerializeField] private AnimationCurve _localScaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Transform - Rotation Z")]
        [SerializeField] private bool _animateRotationZ = false;
        [SerializeField] private float _rotationZFrom = 0f;
        [SerializeField] private float _rotationZTo = 0f;
        [SerializeField] private AnimationCurve _rotationZCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Fill X (left-right reveal via scale.x)")]
        [SerializeField] private bool _animateFillX = false;
        [SerializeField] private float _fillXFrom = 0f;
        [SerializeField] private float _fillXTo = 1f;
        [SerializeField] private AnimationCurve _fillXCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private bool _preserveFillTargetScaleSign = true;

        [Header("Sprite Color / Alpha")]
        [SerializeField] private bool _animateColor = false;
        [SerializeField] private Gradient _colorGradient = null;
        [SerializeField] private bool _animateAlpha = true;
        [SerializeField] private float _alphaFrom = 1f;
        [SerializeField] private float _alphaTo = 0f;
        [SerializeField] private AnimationCurve _alphaCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        [Header("Sorting")]
        [SerializeField] private bool _animateSortingOrder = false;
        [SerializeField] private int _sortingOrderFrom = 0;
        [SerializeField] private int _sortingOrderTo = 0;
        [SerializeField] private AnimationCurve _sortingOrderCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header("Shader Tracks (optional, per-renderer via MPB)")]
        [SerializeField] private ShaderFloatTrack[] _shaderFloatTracks;
        [SerializeField] private ShaderColorTrack[] _shaderColorTracks;

        [Header("Layered Child Trigger (optional)")]
        [SerializeField] private TimedChildActivation[] _timedChildActivations;

        private struct RendererState
        {
            public Color color;
            public int sortingOrder;
        }

        private Transform _root;
        private Transform _fillRoot;
        private Vector3 _initialLocalPosition;
        private Vector3 _initialLocalScale;
        private Quaternion _initialLocalRotation;
        private Vector3 _fillInitialLocalScale;
        private Coroutine _playRoutine;
        private MaterialPropertyBlock _mpb;
        private RendererState[] _rendererStates;
        private bool[] _childActivationTriggered;

        private float SafeDuration => Mathf.Max(0.01f, _duration);

        private void Awake()
        {
            CacheReferences();
            CacheInitialState();
            EnsureDefaultGradient();
            DeactivateTimedChildrenAtStart();
        }

        private void OnEnable()
        {
            if (!_playOnEnable || !Application.isPlaying)
                return;

            Play();
        }

        private void OnDisable()
        {
            if (_playRoutine != null)
            {
                StopCoroutine(_playRoutine);
                _playRoutine = null;
            }

            RestoreInitialState();
        }

        [ContextMenu("Play VFX")]
        public void Play()
        {
            CacheReferences();
            CacheInitialState();
            StopCurrentPlayback();
            _playRoutine = StartCoroutine(PlayRoutine());
        }

        [ContextMenu("Stop And Reset VFX")]
        public void StopAndReset()
        {
            StopCurrentPlayback();
            RestoreInitialState();
            DeactivateTimedChildrenAtStart();
        }

        private IEnumerator PlayRoutine()
        {
            DeactivateTimedChildrenAtStart();
            Apply(0f);

            if (_startDelay > 0f)
                yield return WaitFor(_startDelay);

            do
            {
                float elapsed = 0f;
                while (elapsed < SafeDuration)
                {
                    elapsed += DeltaTime();
                    float t = Mathf.Clamp01(elapsed / SafeDuration);
                    Apply(t);
                    TriggerTimedChildren(t);
                    yield return null;
                }

                Apply(1f);
                TriggerTimedChildren(1f);
            }
            while (_loop);

            _playRoutine = null;

            if (_autoDeactivateWhenFinished)
                gameObject.SetActive(false);
        }

        private void Apply(float normalized)
        {
            if (_root == null)
                return;

            float posT = Evaluate(_localPositionCurve, normalized);
            float scaleT = Evaluate(_localScaleCurve, normalized);
            float rotT = Evaluate(_rotationZCurve, normalized);
            float fillT = Evaluate(_fillXCurve, normalized);
            float alphaT = Evaluate(_alphaCurve, normalized);
            float sortingT = Evaluate(_sortingOrderCurve, normalized);

            if (_animateLocalPosition)
                _root.localPosition = Vector3.LerpUnclamped(_localPositionFrom, _localPositionTo, posT);

            if (_animateLocalScale)
                _root.localScale = Vector3.LerpUnclamped(_localScaleFrom, _localScaleTo, scaleT);

            if (_animateRotationZ)
            {
                float rz = Mathf.LerpUnclamped(_rotationZFrom, _rotationZTo, rotT);
                _root.localRotation = Quaternion.Euler(0f, 0f, rz);
            }

            if (_animateFillX && _fillRoot != null)
            {
                float fill = Mathf.LerpUnclamped(_fillXFrom, _fillXTo, fillT);
                Vector3 s = _fillInitialLocalScale;
                float sign = _preserveFillTargetScaleSign ? Mathf.Sign(s.x == 0f ? 1f : s.x) : 1f;
                s.x = Mathf.Abs(s.x) * fill * sign;
                _fillRoot.localScale = s;
            }

            if (_renderers == null || _renderers.Length == 0)
                return;

            for (int i = 0; i < _renderers.Length; i++)
            {
                var sr = _renderers[i];
                if (sr == null)
                    continue;

                Color baseColor = _rendererStates[i].color;
                Color color = baseColor;

                if (_animateColor && _colorGradient != null)
                    color = _colorGradient.Evaluate(normalized);

                if (_animateAlpha)
                {
                    float a = Mathf.LerpUnclamped(_alphaFrom, _alphaTo, alphaT);
                    color.a *= a;
                }

                sr.color = color;

                if (_animateSortingOrder)
                {
                    float order = Mathf.LerpUnclamped(_sortingOrderFrom, _sortingOrderTo, sortingT);
                    sr.sortingOrder = Mathf.RoundToInt(order);
                }

                ApplyShaderTracks(sr, normalized);
            }
        }

        private void ApplyShaderTracks(SpriteRenderer sr, float normalized)
        {
            if ((_shaderFloatTracks == null || _shaderFloatTracks.Length == 0) &&
                (_shaderColorTracks == null || _shaderColorTracks.Length == 0))
            {
                return;
            }

            if (_mpb == null)
                _mpb = new MaterialPropertyBlock();

            sr.GetPropertyBlock(_mpb);

            if (_shaderFloatTracks != null)
            {
                for (int i = 0; i < _shaderFloatTracks.Length; i++)
                {
                    var track = _shaderFloatTracks[i];
                    if (track == null || !track.enabled || string.IsNullOrWhiteSpace(track.propertyName))
                        continue;

                    float t = Evaluate(track.curve, normalized);
                    float v = Mathf.LerpUnclamped(track.from, track.to, t);
                    _mpb.SetFloat(track.propertyName, v);
                }
            }

            if (_shaderColorTracks != null)
            {
                for (int i = 0; i < _shaderColorTracks.Length; i++)
                {
                    var track = _shaderColorTracks[i];
                    if (track == null || !track.enabled || string.IsNullOrWhiteSpace(track.propertyName) || track.gradient == null)
                        continue;

                    Color c = track.gradient.Evaluate(normalized);
                    _mpb.SetColor(track.propertyName, c);
                }
            }

            sr.SetPropertyBlock(_mpb);
        }

        private void TriggerTimedChildren(float normalized)
        {
            if (_timedChildActivations == null || _timedChildActivations.Length == 0)
                return;

            if (_childActivationTriggered == null || _childActivationTriggered.Length != _timedChildActivations.Length)
                _childActivationTriggered = new bool[_timedChildActivations.Length];

            for (int i = 0; i < _timedChildActivations.Length; i++)
            {
                if (_childActivationTriggered[i])
                    continue;

                var timed = _timedChildActivations[i];
                if (timed == null || timed.target == null)
                    continue;

                if (normalized >= timed.normalizedTime)
                {
                    timed.target.SetActive(true);
                    _childActivationTriggered[i] = true;
                }
            }
        }

        private void DeactivateTimedChildrenAtStart()
        {
            if (_timedChildActivations == null || _timedChildActivations.Length == 0)
                return;

            _childActivationTriggered = new bool[_timedChildActivations.Length];
            for (int i = 0; i < _timedChildActivations.Length; i++)
            {
                var timed = _timedChildActivations[i];
                if (timed?.target != null && timed.autoDisableWhenFinished)
                    timed.target.SetActive(false);
            }
        }

        private void RestoreInitialState()
        {
            if (_root != null)
            {
                _root.localPosition = _initialLocalPosition;
                _root.localScale = _initialLocalScale;
                _root.localRotation = _initialLocalRotation;
            }

            if (_fillRoot != null)
                _fillRoot.localScale = _fillInitialLocalScale;

            if (_renderers != null)
            {
                for (int i = 0; i < _renderers.Length; i++)
                {
                    var sr = _renderers[i];
                    if (sr == null)
                        continue;

                    sr.color = _rendererStates[i].color;
                    sr.sortingOrder = _rendererStates[i].sortingOrder;

                    if (_mpb == null)
                        _mpb = new MaterialPropertyBlock();
                    _mpb.Clear();
                    sr.SetPropertyBlock(_mpb);
                }
            }
        }

        private void CacheReferences()
        {
            _root = _animatedRoot != null ? _animatedRoot : transform;
            _fillRoot = _fillTargetX != null ? _fillTargetX : _root;

            if (_autoCollectRenderers || _renderers == null || _renderers.Length == 0)
                _renderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        private void CacheInitialState()
        {
            if (_root != null)
            {
                _initialLocalPosition = _root.localPosition;
                _initialLocalScale = _root.localScale;
                _initialLocalRotation = _root.localRotation;
            }

            if (_fillRoot != null)
                _fillInitialLocalScale = _fillRoot.localScale;

            if (_renderers == null)
                return;

            _rendererStates = new RendererState[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
            {
                var sr = _renderers[i];
                if (sr == null)
                    continue;

                _rendererStates[i] = new RendererState
                {
                    color = sr.color,
                    sortingOrder = sr.sortingOrder
                };
            }
        }

        private void EnsureDefaultGradient()
        {
            if (_colorGradient != null)
                return;

            _colorGradient = new Gradient();
            _colorGradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                });
        }

        private void StopCurrentPlayback()
        {
            if (_playRoutine != null)
            {
                StopCoroutine(_playRoutine);
                _playRoutine = null;
            }
        }

        private IEnumerator WaitFor(float seconds)
        {
            if (_useUnscaledTime)
            {
                float e = 0f;
                while (e < seconds)
                {
                    e += Time.unscaledDeltaTime;
                    yield return null;
                }
                yield break;
            }

            yield return new WaitForSeconds(seconds);
        }

        private float DeltaTime()
        {
            return _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        }

        private static float Evaluate(AnimationCurve curve, float t)
        {
            if (curve == null || curve.length == 0)
                return t;
            return curve.Evaluate(t);
        }
    }
}

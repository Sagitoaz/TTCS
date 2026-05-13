using System.Collections;
using DG.Tweening;
using TTCS.Combat.Entities;
using TTCS.Combat.Managers;
using TTCS.Core.Data;
using TTCS.Core.Events;
using UnityEngine;
using UnityEngine.UI;

namespace TTCS.UI.Combat
{
    /// <summary>
    /// Hiển thị portrait panel khi có SkillCastEvent.
    /// Character: trượt từ trái vào giữa.
    /// Enemy: trượt từ phải vào giữa.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class SkillCastPortraitPanel : MonoBehaviour
    {
        [Header("UI Refs")]
        [SerializeField] private RectTransform _panelRoot;
        [SerializeField] private Image _portraitImage;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Motion")]
        [SerializeField] private float _enterDuration = 0.28f;
        [SerializeField] private float _holdDuration = 0.45f;
        [SerializeField] private float _fadeOutDuration = 0.12f;
        [SerializeField] private float _offscreenPadding = 120f;

        private Vector2 _centerAnchoredPos;
        private Coroutine _sequenceRoutine;

        private void Awake()
        {
            if (_panelRoot == null)
                _panelRoot = transform as RectTransform;

            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();

            _centerAnchoredPos = _panelRoot != null ? _panelRoot.anchoredPosition : Vector2.zero;
            HideImmediate();
        }

        private void OnEnable()
        {
            EventBus.Instance?.Subscribe<SkillCastEvent>(OnSkillCast);
        }

        private void OnDisable()
        {
            EventBus.Instance?.Unsubscribe<SkillCastEvent>(OnSkillCast);
            StopRunningSequence();
            HideImmediate();
        }

        private void OnSkillCast(SkillCastEvent e)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.CasterId))
                return;

            if (!TryResolveCasterPortrait(e.CasterId, out var portraitSprite, out var isPlayer))
                return;

            if (portraitSprite == null || _panelRoot == null || _portraitImage == null)
                return;

            _portraitImage.sprite = portraitSprite;
            _portraitImage.enabled = true;

            StopRunningSequence();
            _sequenceRoutine = StartCoroutine(PlaySequence(isPlayer));
        }

        private IEnumerator PlaySequence(bool isPlayer)
        {
            gameObject.SetActive(true);

            if (_canvasGroup != null)
                _canvasGroup.alpha = 1f;

            var startPos = new Vector2(GetOffscreenX(isPlayer), _centerAnchoredPos.y);
            _panelRoot.anchoredPosition = startPos;

            Tween enterTween = _panelRoot.DOAnchorPos(_centerAnchoredPos, _enterDuration).SetEase(Ease.OutCubic);
            yield return enterTween.WaitForCompletion();

            if (_holdDuration > 0f)
                yield return new WaitForSecondsRealtime(_holdDuration);

            if (_canvasGroup != null && _fadeOutDuration > 0f)
            {
                Tween fadeTween = _canvasGroup.DOFade(0f, _fadeOutDuration).SetEase(Ease.InQuad);
                yield return fadeTween.WaitForCompletion();
            }

            HideImmediate();
            _sequenceRoutine = null;
        }

        private float GetOffscreenX(bool fromLeft)
        {
            var parentRect = _panelRoot.parent as RectTransform;
            float parentHalfWidth = parentRect != null ? parentRect.rect.width * 0.5f : Screen.width * 0.5f;
            float panelHalfWidth = _panelRoot.rect.width * 0.5f;
            float distance = parentHalfWidth + panelHalfWidth + Mathf.Max(0f, _offscreenPadding);
            return fromLeft ? -distance : distance;
        }

        private bool TryResolveCasterPortrait(string casterId, out Sprite sprite, out bool isPlayer)
        {
            sprite = null;
            isPlayer = true;

            var flow = CombatFlowController.Instance;
            var dataManager = DataManager.Instance;
            if (flow == null || dataManager == null)
                return false;

            var entities = flow.GetAllEntities();
            if (entities == null || entities.Count == 0)
                return false;

            for (int i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                if (entity == null || !string.Equals(entity.ID, casterId, System.StringComparison.Ordinal))
                    continue;

                isPlayer = entity.IsPlayer;
                string portraitPath = null;

                if (entity is Character character)
                {
                    var model = dataManager.LoadCharacter(character.CharacterId);
                    portraitPath = model?.visual?.portraitPath;
                    if (string.IsNullOrWhiteSpace(portraitPath))
                        portraitPath = model?.visual?.spritePath;
                }
                else if (entity is Enemy enemy)
                {
                    var model = dataManager.LoadEnemy(enemy.EnemyTemplateId);
                    portraitPath = model?.visual?.portraitPath;
                    if (string.IsNullOrWhiteSpace(portraitPath))
                        portraitPath = model?.visual?.spritePath;
                }

                if (string.IsNullOrWhiteSpace(portraitPath))
                    return false;

                sprite = dataManager.LoadCharacterPortraitSprite(portraitPath);
                return sprite != null;
            }

            return false;
        }

        private void StopRunningSequence()
        {
            if (_sequenceRoutine != null)
            {
                StopCoroutine(_sequenceRoutine);
                _sequenceRoutine = null;
            }

            if (_panelRoot != null)
                _panelRoot.DOKill();

            _canvasGroup?.DOKill();
        }

        private void HideImmediate()
        {
            if (_canvasGroup != null)
                _canvasGroup.alpha = 0f;
        }
    }
}

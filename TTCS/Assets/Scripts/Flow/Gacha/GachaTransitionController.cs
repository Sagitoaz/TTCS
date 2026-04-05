using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace TTCS.Flow.Gacha
{
    public class GachaTransitionController : MonoBehaviour
    {
        [Serializable]
        public class RarityTransitionPreset
        {
            public float shakePositionStrength = 12f;
            public float shakeRotationStrength = 5f;
            public float zoomScale = 15f;
            public float flashPeakAlpha = 1f;
            public float itemFlashPeakAlpha = 0.55f;
            public float anticipationMultiplier = 1f;
            public float zoomMultiplier = 1f;
            public float revealMultiplier = 1f;
        }

        [Header("Required References")]
        public GameObject bannerScreen;
        public RectTransform bannerImage;
        public GameObject resultScreen;
        public Image flashImage;

        [Header("Main Transition Timing")]
        public float anticipationDuration = 1.0f;
        public float zoomDuration = 0.5f;
        public float revealDuration = 1.0f;

        [Header("Result Item Transition")]
        public float resultFlashInDuration = 0.12f;
        public float resultFlashOutDuration = 0.18f;
        public float resultFlashPeakAlpha = 0.55f;

        [Header("SSR Special Effects")]
        public bool enableSsrSpecialEffects = true;
        public bool enableSsrDoubleFlash = true;
        public float ssrDoubleFlashMidAlpha = 0.35f;
        public float ssrDoubleFlashDownDuration = 0.08f;
        public float ssrDoubleFlashUpDuration = 0.12f;
        public RectTransform ssrSealTransform;
        public CanvasGroup ssrSealCanvasGroup;
        public float ssrSealDuration = 0.55f;
        public AudioSource sfxAudioSource;
        public AudioClip ssrStingerClip;

        [Header("Result Card Entrance")]
        public float resultCardPopDuration = 0.24f;
        public float resultCardSettleDuration = 0.14f;
        public float resultCardSsrStartScale = 0.6f;
        public float resultCardSsrPeakScale = 1.08f;
        public float resultCardDefaultStartScale = 0.92f;
        public float resultCardDefaultPeakScale = 1.02f;

        [Header("Rarity Presets")]
        public RarityTransitionPreset ssrPreset = new RarityTransitionPreset
        {
            shakePositionStrength = 18f,
            shakeRotationStrength = 8f,
            zoomScale = 1.18f,
            flashPeakAlpha = 1f,
            itemFlashPeakAlpha = 0.8f,
            anticipationMultiplier = 1.1f,
            zoomMultiplier = 1f,
            revealMultiplier = 1.1f
        };
        public RarityTransitionPreset srPreset = new RarityTransitionPreset
        {
            shakePositionStrength = 14f,
            shakeRotationStrength = 6f,
            zoomScale = 1.14f,
            flashPeakAlpha = 0.9f,
            itemFlashPeakAlpha = 0.65f,
            anticipationMultiplier = 1f,
            zoomMultiplier = 1f,
            revealMultiplier = 1f
        };
        public RarityTransitionPreset rPreset = new RarityTransitionPreset
        {
            shakePositionStrength = 10f,
            shakeRotationStrength = 4f,
            zoomScale = 1.1f,
            flashPeakAlpha = 0.75f,
            itemFlashPeakAlpha = 0.5f,
            anticipationMultiplier = 0.9f,
            zoomMultiplier = 1f,
            revealMultiplier = 0.9f
        };

        private Sequence _mainSequence;
        private Sequence _itemSequence;
        private Sequence _sealSequence;
        private Tween _resultCardTween;
        private Vector2 _initialAnchorPos;
        private Vector3 _initialScale = Vector3.one;
        private Vector3 _initialEuler;
        private bool _hasCachedInitialTransform;

        public event Action OnSwapToResult;
        public event Action OnResultItemSwap;

        private void Awake()
        {
            CacheInitialBannerTransform();
            SetFlashAlpha(0f);

            if (bannerScreen == null)
            {
                Debug.LogWarning("[GachaTransition] bannerScreen is not assigned.");
            }

            if (bannerImage == null)
            {
                Debug.LogWarning("[GachaTransition] bannerImage is not assigned. Shake/zoom effect will be skipped.");
            }

            if (resultScreen == null)
            {
                Debug.LogWarning("[GachaTransition] resultScreen is not assigned. Swap-screen effect may not be visible.");
            }

            if (flashImage == null)
            {
                Debug.LogWarning("[GachaTransition] flashImage is not assigned. Flash effect will be skipped.");
            }

            if (ssrSealCanvasGroup != null)
            {
                ssrSealCanvasGroup.alpha = 0f;
                ssrSealCanvasGroup.gameObject.SetActive(false);
            }
        }

        public void PlayGachaAnimation(Color rarityColor)
        {
            PlayGachaAnimation(rarityColor, "R");
        }

        public void PlayGachaAnimation(Color rarityColor, string rarityTag)
        {
            KillTweens();
            CacheInitialBannerTransform();
            var preset = GetPreset(rarityTag);
            var isSsr = string.Equals(rarityTag, "SSR", StringComparison.OrdinalIgnoreCase);

            Debug.Log($"[GachaTransition] PlayGachaAnimation called with rarityTag='{rarityTag}', isSsr={isSsr}, enableSsrSpecialEffects={enableSsrSpecialEffects}");

            if (resultScreen != null)
            {
                resultScreen.SetActive(false);
            }

            if (bannerScreen != null)
            {
                bannerScreen.SetActive(true);
            }

            SetFlashColor(rarityColor, 0f);

            _mainSequence = DOTween.Sequence();
            var anticipation = Mathf.Max(0f, anticipationDuration * Mathf.Max(0.1f, preset.anticipationMultiplier));
            var zoom = Mathf.Max(0f, zoomDuration * Mathf.Max(0.1f, preset.zoomMultiplier));
            var reveal = Mathf.Max(0f, revealDuration * Mathf.Max(0.1f, preset.revealMultiplier));

            if (bannerImage != null)
            {
                _mainSequence.Append(bannerImage.DOShakeAnchorPos(anticipation, new Vector2(preset.shakePositionStrength, preset.shakePositionStrength), 16, 90f, false, true));
                _mainSequence.Join(bannerImage.DOShakeRotation(anticipation, new Vector3(0f, 0f, preset.shakeRotationStrength), 16, 90f, false));
            }
            else
            {
                _mainSequence.AppendInterval(anticipation);
            }

            if (bannerImage != null)
            {
                _mainSequence.Append(bannerImage.DOScale(new Vector3(preset.zoomScale, preset.zoomScale, preset.zoomScale), zoom).SetEase(Ease.InExpo));
            }
            else
            {
                _mainSequence.AppendInterval(zoom);
            }

            if (flashImage != null)
            {
                var peakAlpha = Mathf.Clamp01(preset.flashPeakAlpha);
                _mainSequence.Join(flashImage.DOFade(peakAlpha, zoom));
            }

            if (isSsr && enableSsrSpecialEffects)
            {
                _mainSequence.AppendCallback(PlaySsrStinger);

                if (enableSsrDoubleFlash && flashImage != null)
                {
                    _mainSequence.Append(flashImage.DOFade(Mathf.Clamp01(ssrDoubleFlashMidAlpha), ssrDoubleFlashDownDuration));
                    _mainSequence.Append(flashImage.DOFade(Mathf.Clamp01(preset.flashPeakAlpha), ssrDoubleFlashUpDuration));
                }

                _mainSequence.AppendCallback(PlaySsrSealEffect);
            }

            _mainSequence.AppendCallback(() =>
            {
                if (bannerScreen != null)
                {
                    bannerScreen.SetActive(false);
                }

                if (resultScreen != null)
                {
                    resultScreen.SetActive(true);
                }

                OnSwapToResult?.Invoke();
            });

            if (flashImage != null)
            {
                _mainSequence.Append(flashImage.DOFade(0f, reveal));
            }
            else
            {
                _mainSequence.AppendInterval(reveal);
            }

            _mainSequence.OnComplete(ResetBannerTransform);
            _mainSequence.OnKill(ResetBannerTransform);
        }

        public void PlayResultItemTransition(Color rarityColor)
        {
            PlayResultItemTransition(rarityColor, "R");
        }

        public void PlayResultItemTransition(Color rarityColor, string rarityTag)
        {
            if (flashImage == null)
            {
                OnResultItemSwap?.Invoke();
                return;
            }

            var preset = GetPreset(rarityTag);

            if (_itemSequence != null && _itemSequence.IsActive())
            {
                _itemSequence.Kill();
            }

            SetFlashColor(rarityColor, 0f);

            _itemSequence = DOTween.Sequence();
            var peakAlpha = Mathf.Clamp01(Mathf.Max(resultFlashPeakAlpha, preset.itemFlashPeakAlpha));
            _itemSequence.Append(flashImage.DOFade(peakAlpha, resultFlashInDuration));
            _itemSequence.AppendCallback(() => OnResultItemSwap?.Invoke());
            _itemSequence.Append(flashImage.DOFade(0f, resultFlashOutDuration));
        }

        public void PlayResultCardEntrance(RectTransform cardRoot, string rarityTag)
        {
            if (cardRoot == null)
            {
                return;
            }

            if (_resultCardTween != null && _resultCardTween.IsActive())
            {
                _resultCardTween.Kill();
            }

            var isSsr = string.Equals(rarityTag, "SSR", StringComparison.OrdinalIgnoreCase);
            var startScale = isSsr ? resultCardSsrStartScale : resultCardDefaultStartScale;
            var peakScale = isSsr ? resultCardSsrPeakScale : resultCardDefaultPeakScale;

            cardRoot.localScale = Vector3.one * Mathf.Max(0.05f, startScale);

            var seq = DOTween.Sequence();
            seq.Append(cardRoot.DOScale(peakScale, resultCardPopDuration).SetEase(Ease.OutBack));
            seq.Append(cardRoot.DOScale(1f, resultCardSettleDuration).SetEase(Ease.OutQuad));
            _resultCardTween = seq;
        }

        private RarityTransitionPreset GetPreset(string rarityTag)
        {
            if (string.Equals(rarityTag, "SSR", StringComparison.OrdinalIgnoreCase))
            {
                return ssrPreset ?? new RarityTransitionPreset();
            }

            if (string.Equals(rarityTag, "SR", StringComparison.OrdinalIgnoreCase))
            {
                return srPreset ?? new RarityTransitionPreset();
            }

            return rPreset ?? new RarityTransitionPreset();
        }

        public void ReturnToBannerScreen()
        {
            KillTweens();
            SetFlashAlpha(0f);

            if (resultScreen != null)
            {
                resultScreen.SetActive(false);
            }

            if (bannerScreen != null)
            {
                bannerScreen.SetActive(true);
            }

            ResetBannerTransform();
        }

        private void OnDestroy()
        {
            KillTweens();
        }

        private void KillTweens()
        {
            if (_mainSequence != null && _mainSequence.IsActive())
            {
                _mainSequence.Kill();
            }

            if (_itemSequence != null && _itemSequence.IsActive())
            {
                _itemSequence.Kill();
            }

            if (_sealSequence != null && _sealSequence.IsActive())
            {
                _sealSequence.Kill();
            }

            if (_resultCardTween != null && _resultCardTween.IsActive())
            {
                _resultCardTween.Kill();
            }
        }

        private void PlaySsrSealEffect()
        {
            Debug.Log($"[GachaTransition] PlaySsrSealEffect called. enableSsrSpecialEffects={enableSsrSpecialEffects}, sealTransform={ssrSealTransform}, sealCanvasGroup={ssrSealCanvasGroup}");

            if (!enableSsrSpecialEffects)
            {
                Debug.LogWarning("[GachaTransition] SSR special effects disabled. Check 'enableSsrSpecialEffects' in Inspector.");
                return;
            }

            if (ssrSealTransform == null)
            {
                Debug.LogWarning("[GachaTransition] ssrSealTransform is not assigned. Seal effect will not play.");
                return;
            }

            if (ssrSealCanvasGroup == null)
            {
                Debug.LogWarning("[GachaTransition] ssrSealCanvasGroup is not assigned. Seal effect will not play.");
                return;
            }

            if (_sealSequence != null && _sealSequence.IsActive())
            {
                _sealSequence.Kill();
            }

            ssrSealCanvasGroup.gameObject.SetActive(true);
            ssrSealCanvasGroup.alpha = 0f;
            ssrSealTransform.localScale = Vector3.one * 0.5f;
            ssrSealTransform.localEulerAngles = Vector3.zero;

            Debug.Log($"[GachaTransition] Seal effect starting with duration {ssrSealDuration}s");

            _sealSequence = DOTween.Sequence();
            _sealSequence.Append(ssrSealCanvasGroup.DOFade(1f, ssrSealDuration * 0.25f));
            _sealSequence.Join(ssrSealTransform.DOScale(1.2f, ssrSealDuration * 0.65f).SetEase(Ease.OutBack));
            _sealSequence.Join(ssrSealTransform.DORotate(new Vector3(0f, 0f, 180f), ssrSealDuration, RotateMode.FastBeyond360).SetEase(Ease.OutCubic));
            _sealSequence.Append(ssrSealCanvasGroup.DOFade(0f, ssrSealDuration * 0.35f));
            _sealSequence.OnComplete(() =>
            {
                if (ssrSealCanvasGroup != null)
                {
                    ssrSealCanvasGroup.gameObject.SetActive(false);
                }
            });
        }

        public void PlaySsrSealEffectDirect()
        {
            if (!enableSsrSpecialEffects || ssrSealTransform == null || ssrSealCanvasGroup == null)
            {
                return;
            }

            if (_sealSequence != null && _sealSequence.IsActive())
            {
                _sealSequence.Kill();
            }

            ssrSealCanvasGroup.gameObject.SetActive(true);
            ssrSealCanvasGroup.alpha = 0f;
            ssrSealTransform.localScale = Vector3.one * 0.5f;
            ssrSealTransform.localEulerAngles = Vector3.zero;

            Debug.Log($"[GachaTransition] SSR seal effect triggered for result item swap");

            _sealSequence = DOTween.Sequence();
            _sealSequence.Append(ssrSealCanvasGroup.DOFade(1f, ssrSealDuration * 0.25f));
            _sealSequence.Join(ssrSealTransform.DOScale(1.2f, ssrSealDuration * 0.65f).SetEase(Ease.OutBack));
            _sealSequence.Join(ssrSealTransform.DORotate(new Vector3(0f, 0f, 180f), ssrSealDuration, RotateMode.FastBeyond360).SetEase(Ease.OutCubic));
            _sealSequence.Append(ssrSealCanvasGroup.DOFade(0f, ssrSealDuration * 0.35f));
            _sealSequence.OnComplete(() =>
            {
                if (ssrSealCanvasGroup != null)
                {
                    ssrSealCanvasGroup.gameObject.SetActive(false);
                }
            });
        }

        private void PlaySsrStinger()
        {
            if (!enableSsrSpecialEffects || sfxAudioSource == null || ssrStingerClip == null)
            {
                return;
            }

            sfxAudioSource.PlayOneShot(ssrStingerClip);
        }

        private void CacheInitialBannerTransform()
        {
            if (bannerImage == null || _hasCachedInitialTransform)
            {
                return;
            }

            _initialAnchorPos = bannerImage.anchoredPosition;
            _initialScale = bannerImage.localScale;
            _initialEuler = bannerImage.localEulerAngles;
            _hasCachedInitialTransform = true;
        }

        private void ResetBannerTransform()
        {
            if (bannerImage == null || !_hasCachedInitialTransform)
            {
                return;
            }

            bannerImage.anchoredPosition = _initialAnchorPos;
            bannerImage.localScale = _initialScale;
            bannerImage.localEulerAngles = _initialEuler;
        }

        private void SetFlashColor(Color color, float alpha)
        {
            if (flashImage == null)
            {
                return;
            }

            color.a = alpha;
            flashImage.color = color;
        }

        private void SetFlashAlpha(float alpha)
        {
            if (flashImage == null)
            {
                return;
            }

            var c = flashImage.color;
            c.a = alpha;
            flashImage.color = c;
        }
    }
}

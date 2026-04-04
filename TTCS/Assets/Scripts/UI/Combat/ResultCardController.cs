using DG.Tweening;
using UnityEngine;

namespace TTCS.UI.Combat
{
    /// <summary>
    /// Điều khiển hiển thị card kết quả theo kiểu WinCard/LoseCard.
    /// Tách riêng khỏi CombatUIController để dễ chỉnh UI mà không động logic combat.
    /// </summary>
    public class ResultCardController : MonoBehaviour
    {
        [Header("Cards")]
        [SerializeField] private GameObject _winCard;
        [SerializeField] private GameObject _loseCard;

        [Header("Card Animation")]
        [SerializeField] private float _enterScaleFrom = 0.78f;
        [SerializeField] private float _enterDuration = 0.28f;
        [SerializeField] private bool _enableLoopPulse = true;
        [SerializeField] private float _loopScale = 1.06f;
        [SerializeField] private float _loopHalfDuration = 0.6f;

        private Tween _activeLoopTween;

        public void ShowResult(bool victory)
        {
            if (_winCard != null) _winCard.SetActive(victory);
            if (_loseCard != null) _loseCard.SetActive(!victory);

            if (victory)
            {
                StopActiveAnimation();
                PlayCardAnimation(_winCard);
            }
            else
            {
                StopActiveAnimation();
                PlayCardAnimation(_loseCard);
            }
        }

        public void HideAll()
        {
            if (_winCard != null) _winCard.SetActive(false);
            if (_loseCard != null) _loseCard.SetActive(false);
            StopActiveAnimation();
        }

        private void OnDisable()
        {
            StopActiveAnimation();
        }

        private void PlayCardAnimation(GameObject card)
        {
            if (card == null)
                return;

            var rt = card.transform as RectTransform;
            if (rt == null)
                return;

            rt.localScale = Vector3.one * _enterScaleFrom;

            rt.DOScale(Vector3.one, _enterDuration)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    if (_enableLoopPulse)
                    {
                        _activeLoopTween = rt.DOScale(Vector3.one * _loopScale, _loopHalfDuration)
                            .SetEase(Ease.InOutSine)
                            .SetLoops(-1, LoopType.Yoyo);
                    }
                });
        }

        private void StopActiveAnimation()
        {
            if (_activeLoopTween != null && _activeLoopTween.IsActive())
                _activeLoopTween.Kill();
        }
    }
}

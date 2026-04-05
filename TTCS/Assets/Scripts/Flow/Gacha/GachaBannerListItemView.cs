using System;
using UnityEngine;
using UnityEngine.UI;

namespace TTCS.Flow.Gacha
{
    public class GachaBannerListItemView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _bannerImage;
        [SerializeField] private GameObject _normalStateObject;

        private Vector3 _baseScale;

        private string _poolId;
        private Action<string> _onClick;

        private void Awake()
        {
            _baseScale = transform.localScale;

            if (_button != null)
            {
                _button.onClick.AddListener(OnClicked);
            }
        }

        public void Bind(string poolId, Sprite bannerSprite, Action<string> onClick)
        {
            _poolId = poolId;
            _onClick = onClick;

            if (_bannerImage != null)
            {
                _bannerImage.sprite = bannerSprite;
                _bannerImage.color = bannerSprite == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
            }
        }

        public void SetSelected(bool selected)
        {
            if (_normalStateObject != null)
            {
                _normalStateObject.SetActive(!selected);
            }

            transform.localScale = selected ? _baseScale * 1.2f : _baseScale;
        }

        private void OnClicked()
        {
            if (!string.IsNullOrWhiteSpace(_poolId))
            {
                _onClick?.Invoke(_poolId);
            }
        }
    }
}

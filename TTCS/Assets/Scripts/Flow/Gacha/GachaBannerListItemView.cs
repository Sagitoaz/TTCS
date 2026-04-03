using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TTCS.Flow.Gacha
{
    public class GachaBannerListItemView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Image _selectedBg;

        private string _poolId;
        private Action<string> _onClick;

        private void Awake()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(OnClicked);
            }
        }

        public void Bind(string poolId, string title, Action<string> onClick)
        {
            _poolId = poolId;
            _onClick = onClick;

            if (_titleText != null)
            {
                _titleText.text = string.IsNullOrWhiteSpace(title) ? poolId : title;
            }
        }

        public void SetSelected(bool selected)
        {
            if (_selectedBg != null)
            {
                _selectedBg.gameObject.SetActive(selected);
            }
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

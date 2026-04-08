using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TTCS.Flow.Inventory
{
    public sealed class InventoryAccessoryStatLineView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _statNameText;
        [SerializeField] private Image _statIcon;
        [SerializeField] private TMP_Text _valueText;

        public void Bind(string statName, Sprite icon, int amount)
        {
            if (_statNameText != null)
            {
                _statNameText.text = string.IsNullOrWhiteSpace(statName) ? string.Empty : statName;
            }

            if (_statIcon != null)
            {
                _statIcon.sprite = icon;
                _statIcon.color = icon == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
                _statIcon.preserveAspect = true;
            }

            if (_valueText != null)
            {
                _valueText.text = amount >= 0 ? $"+{amount}" : amount.ToString();
            }
        }
    }
}

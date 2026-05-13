using System;
using TMPro;
using TTCS.Core.Data;
using TTCS.Core.Progression;
using TTCS.Core.Save;
using TTCS.Flow.TeamFormation;
using UnityEngine;
using UnityEngine.UI;

namespace TTCS.UI.Combat
{
    public sealed class CombatResultCharacterProgressView : MonoBehaviour
    {
        [SerializeField] private TeamFormationPickerCellView _characterSlot;
        [SerializeField] private Slider _expSlider;
        [SerializeField] private TextMeshProUGUI _expText;

        public void Bind(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return;
            }

            var save = SaveManager.Instance?.CurrentSave;
            var level = Math.Max(1, save?.GetCharacterLevel(characterId) ?? 1);
            var exp = Math.Max(0, save?.GetCharacterExp(characterId) ?? 0);
            var expToNext = Math.Max(1, CharacterProgression.GetExpToNextLevel(level));
            var clamped = Math.Min(exp, expToNext);

            var data = DataManager.Instance?.LoadCharacter(characterId);
            var rarity = data?.metadata?.rarity ?? "R";
            var portraitPath = data?.visual?.portraitPath;
            var portrait = DataManager.Instance?.LoadCharacterPortraitSprite(portraitPath);

            _characterSlot?.Bind(data?.nameKey ?? characterId, level, rarity, portrait);

            if (_expSlider != null)
            {
                _expSlider.minValue = 0;
                _expSlider.maxValue = expToNext;
                _expSlider.value = clamped;
            }

            if (_expText != null)
            {
                _expText.text = $"{clamped}/{expToNext}";
            }
        }
    }
}

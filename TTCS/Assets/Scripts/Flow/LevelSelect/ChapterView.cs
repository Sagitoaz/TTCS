using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TTCS.Flow.LevelSelect
{
    public class ChapterView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _chapterIndexText;
        [SerializeField] private TMP_Text _chapterNameText;
        [SerializeField] private Image _backgroundImage;

        public void Bind(int chapterOrder, string chapterName, bool isUnlocked, string chapterId, Action onClick)
        {
            if (_chapterIndexText != null)
            {
                _chapterIndexText.text = $"Chapter {chapterOrder}";
            }

            if (_chapterNameText != null)
            {
                _chapterNameText.text = string.IsNullOrWhiteSpace(chapterName) ? chapterId : chapterName;
            }

            if (_backgroundImage != null)
            {
                var sprite = LoadChapterBackground(chapterId, isUnlocked);
                _backgroundImage.sprite = sprite;
                _backgroundImage.color = sprite == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
            }

            if (_button != null)
            {
                _button.interactable = isUnlocked;
                _button.onClick.RemoveAllListeners();
                if (onClick != null)
                {
                    _button.onClick.AddListener(() => onClick.Invoke());
                }
            }
        }

        private static Sprite LoadChapterBackground(string chapterId, bool isUnlocked)
        {
            if (string.IsNullOrWhiteSpace(chapterId))
            {
                return null;
            }

            var state = isUnlocked ? "unlocked" : "locked";
            return Resources.Load<Sprite>($"UI/LevelSelect/Chapter/{chapterId}_{state}");
        }
    }
}

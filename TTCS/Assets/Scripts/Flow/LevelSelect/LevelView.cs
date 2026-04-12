using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TTCS.Flow.LevelSelect
{
    public class LevelView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _orderText;
        [SerializeField] private TMP_Text _levelNameText;
        [SerializeField] private TMP_Text _recommendLevelText;
        [SerializeField] private Transform _enemyElementRoot;
        [SerializeField] private Image _enemyElementIconPrefab;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private GameObject _completedObject;

        private readonly List<GameObject> _spawnedElements = new List<GameObject>();

        public void Bind(string levelId, int chapterOrder, int levelOrder, string levelName, int recommendLevel, bool isUnlocked, bool isCompleted, IReadOnlyList<string> enemyElements, Action onClick)
        {
            if (_orderText != null)
            {
                _orderText.text = $"Chapter {chapterOrder} - {levelOrder}";
            }

            if (_levelNameText != null)
            {
                _levelNameText.text = string.IsNullOrWhiteSpace(levelName) ? levelId : levelName;
            }

            if (_recommendLevelText != null)
            {
                _recommendLevelText.text = recommendLevel > 0 ? $"Lv.{recommendLevel}+" : "Lv.-";
            }

            if (_backgroundImage != null)
            {
                var sprite = LoadLevelBackground(isUnlocked);
                _backgroundImage.sprite = sprite;
                _backgroundImage.color = sprite == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
            }

            if (_completedObject != null)
            {
                _completedObject.SetActive(isCompleted);
            }

            RebuildElementIcons(enemyElements);

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

        private void RebuildElementIcons(IReadOnlyList<string> enemyElements)
        {
            ClearSpawnedElements();
            if (_enemyElementRoot == null || _enemyElementIconPrefab == null || enemyElements == null)
            {
                return;
            }

            for (var i = 0; i < enemyElements.Count; i++)
            {
                var icon = Instantiate(_enemyElementIconPrefab, _enemyElementRoot);
                _spawnedElements.Add(icon.gameObject);

                var sprite = LoadElementIcon(enemyElements[i]);
                icon.sprite = sprite;
                icon.color = sprite == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
            }
        }

        private void ClearSpawnedElements()
        {
            for (var i = 0; i < _spawnedElements.Count; i++)
            {
                if (_spawnedElements[i] != null)
                {
                    Destroy(_spawnedElements[i]);
                }
            }

            _spawnedElements.Clear();
        }

        private static Sprite LoadLevelBackground(bool isUnlocked)
        {
            var state = isUnlocked ? "unlocked" : "locked";
            return Resources.Load<Sprite>($"UI/LevelSelect/Level/level_{state}");
        }

        private static Sprite LoadElementIcon(string element)
        {
            var normalized = NormalizeElement(element);
            return Resources.Load<Sprite>($"UI/Element/icon_element_{normalized}");
        }

        private static string NormalizeElement(string element)
        {
            if (string.IsNullOrWhiteSpace(element))
            {
                return "physical";
            }

            var normalized = element.Trim().ToLowerInvariant();
            if (normalized == "neutral")
            {
                return "physical";
            }

            return normalized;
        }
    }
}

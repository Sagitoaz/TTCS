using System;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using TTCS.Core.Data;
using TTCS.Core.Save;
using TTCS.Data;
using TTCS.Meta;
using TTCS.Meta.Gacha;
using UnityEngine;
using UnityEngine.UI;

namespace TTCS.Flow.Gacha
{
	/// <summary>
	/// Day 5+ - Gacha scene UI controller.
	/// Banner list (left), banner preview (right), roll and sequential result display.
	/// </summary>
	public class GachaUIController : MonoBehaviour
	{
		[Header("Top Bar")]
		[SerializeField] private Button _backButton;
		[SerializeField] private TextMeshProUGUI _poolNameText;
		[SerializeField] private TextMeshProUGUI _pityText;
		[SerializeField] private TextMeshProUGUI _goldText;
		[SerializeField] private Image _goldIcon;

		[Header("Banner List (Left)")]
		[SerializeField] private Transform _bannerListRoot;
		[SerializeField] private GachaBannerListItemView _bannerItemPrefab;

		[Header("Banner Preview (Right)")]
		[SerializeField] private Image _bannerBackgroundImage;

		[Header("Roll Actions")]
		[SerializeField] private Button _rollOneButton;
		[SerializeField] private Button _rollTenButton;
		[SerializeField] private TextMeshProUGUI _rollCostText;

		[Header("Roll Animation")]
		[SerializeField] private Animator _rollAnimator;
		[SerializeField] private string _rollTrigger = "Roll";
		[SerializeField] private float _rollRevealDelay = 1.0f;

		[Header("Result Panel")]
		[SerializeField] private GameObject _resultPanel;
		[SerializeField] private Image _resultPortrait;
		[SerializeField] private TextMeshProUGUI _resultNameText;
		[SerializeField] private TextMeshProUGUI _resultRarityText;
		[SerializeField] private TextMeshProUGUI _resultRoleText;
		[SerializeField] private TextMeshProUGUI _resultElementText;
		[SerializeField] private TextMeshProUGUI _resultExtraText;
		[SerializeField] private Button _resultCloseButton;
		[SerializeField] private Button _resultNextButton;

		private IGachaService _gachaService;
		private DataManager _dataManager;
		private SaveManager _saveManager;
		private readonly List<GachaPoolDataModel> _pools = new List<GachaPoolDataModel>();
		private readonly List<GachaBannerListItemView> _spawnedBannerItems = new List<GachaBannerListItemView>();
		private string _selectedPoolId = "pool_standard";
		private GachaRollResult _activeRollResult;
		private int _activeResultIndex;

		private void Start()
		{
			_gachaService = MetaServiceHub.Instance?.GachaService;
			_dataManager = DataManager.Instance;
			_saveManager = SaveManager.Instance;
			_saveManager?.EnsureCurrentSave(0);

			WireButtons();
			BuildBannerList();
			RefreshPoolInfo();
			SetResultPanelVisible(false);
		}

		private void WireButtons()
		{
			if (_backButton != null)
			{
				_backButton.onClick.AddListener(OnBackClicked);
			}

			if (_rollOneButton != null)
			{
				_rollOneButton.onClick.AddListener(() => OnRollClicked(1));
			}

			if (_rollTenButton != null)
			{
				_rollTenButton.onClick.AddListener(() => OnRollClicked(10));
			}

			if (_resultCloseButton != null)
			{
				_resultCloseButton.onClick.AddListener(OnResultCloseClicked);
			}

			if (_resultNextButton != null)
			{
				_resultNextButton.onClick.AddListener(OnResultNextClicked);
			}
		}

		private void BuildBannerList()
		{
			_pools.Clear();
			ClearBannerItems();

			var pools = _dataManager?.GetAllGachaPools();
			if (pools != null)
			{
				foreach (var pool in pools)
				{
					if (pool == null || string.IsNullOrWhiteSpace(pool.id))
					{
						continue;
					}

					_pools.Add(pool);
				}
			}

			if (_pools.Count == 0)
			{
				_pools.Add(new GachaPoolDataModel
				{
					id = "pool_standard",
					nameKey = "pool_standard",
					pityThreshold = 10,
					rollCostSingle = 160,
					rollCostTen = 1600
				});
			}

			_selectedPoolId = _pools[0].id;

			if (_bannerListRoot == null || _bannerItemPrefab == null)
			{
				return;
			}

			for (var i = 0; i < _pools.Count; i++)
			{
				var pool = _pools[i];
				var item = Instantiate(_bannerItemPrefab, _bannerListRoot);
				item.Bind(pool.id, string.IsNullOrWhiteSpace(pool.nameKey) ? pool.id : pool.nameKey, OnBannerSelected);
				item.SetSelected(i == 0);
				_spawnedBannerItems.Add(item);
			}
		}

		private void ClearBannerItems()
		{
			for (var i = 0; i < _spawnedBannerItems.Count; i++)
			{
				if (_spawnedBannerItems[i] != null)
				{
					Destroy(_spawnedBannerItems[i].gameObject);
				}
			}
			_spawnedBannerItems.Clear();
		}

		private void OnBannerSelected(string poolId)
		{
			_selectedPoolId = poolId;
			for (var i = 0; i < _spawnedBannerItems.Count; i++)
			{
				var current = _spawnedBannerItems[i];
				if (current != null)
				{
					current.SetSelected(string.Equals(_pools[i].id, poolId, StringComparison.Ordinal));
				}
			}

			RefreshPoolInfo();
		}

		private void OnRollClicked(int count)
		{
			if (!IsRollAllowed(count))
			{
				return;
			}

			StartCoroutine(PlayRollAndShowResult(count));
		}

		private bool IsRollAllowed(int count)
		{
			if (_gachaService == null)
			{
				_gachaService = MetaServiceHub.Instance?.GachaService;
			}

			if (_gachaService == null)
			{
				Debug.LogWarning("[GachaUI] GachaService is null. Ensure MetaServiceHub is initialized.");
				return false;
			}

			var save = _saveManager?.CurrentSave;
			if (save == null)
			{
				Debug.LogWarning("[GachaUI] Save is null.");
				return false;
			}

			var pool = GetSelectedPool();
			if (pool == null)
			{
				Debug.LogWarning("[GachaUI] Selected pool not found.");
				return false;
			}

			var cost = count >= 10 ? Math.Max(0, pool.rollCostTen) : Math.Max(0, pool.rollCostSingle) * count;
			if (save.gold < cost)
			{
				if (_resultExtraText != null)
				{
					_resultExtraText.text = $"Not enough gold. Need {cost:N0}, current {save.gold:N0}.";
				}
				return false;
			}

			return true;
		}

		private IEnumerator PlayRollAndShowResult(int count)
		{
			if (_rollAnimator != null && !string.IsNullOrWhiteSpace(_rollTrigger))
			{
				_rollAnimator.SetTrigger(_rollTrigger);
			}

			var wait = _rollRevealDelay < 0f ? 0f : _rollRevealDelay;
			if (wait > 0f)
			{
				yield return new WaitForSeconds(wait);
			}

			var result = _gachaService.Roll(_selectedPoolId, count);
			_gachaService.ApplyRollResult(result);

			ShowRollResult(result, count);
			RefreshPoolInfo();
		}

		private void ShowRollResult(GachaRollResult result, int rollCount)
		{
			_activeRollResult = result;
			_activeResultIndex = 0;

			SetResultPanelVisible(true);

			if (result == null || result.Rewards == null || result.Rewards.Count == 0)
			{
				if (_resultNameText != null) _resultNameText.text = "No Reward";
				if (_resultRarityText != null) _resultRarityText.text = "-";
				if (_resultRoleText != null) _resultRoleText.text = "Role: -";
				if (_resultElementText != null) _resultElementText.text = "Element: -";
				if (_resultExtraText != null) _resultExtraText.text = $"Roll x{rollCount} | Pity: -";
				if (_resultPortrait != null)
				{
					_resultPortrait.sprite = null;
					_resultPortrait.color = new Color(1f, 1f, 1f, 0f);
				}
				if (_resultNextButton != null)
				{
					_resultNextButton.gameObject.SetActive(false);
				}
				return;
			}

			UpdateResultView();
		}

		private void UpdateResultView()
		{
			if (_activeRollResult == null || _activeRollResult.Rewards == null || _activeRollResult.Rewards.Count == 0)
			{
				return;
			}

			if (_activeResultIndex < 0)
			{
				_activeResultIndex = 0;
			}

			if (_activeResultIndex >= _activeRollResult.Rewards.Count)
			{
				_activeResultIndex = _activeRollResult.Rewards.Count - 1;
			}

			var reward = _activeRollResult.Rewards[_activeResultIndex];
			var character = string.Equals(reward.RewardType, "character", StringComparison.OrdinalIgnoreCase)
				? _dataManager?.LoadCharacter(reward.RewardId)
				: null;

			if (_resultNameText != null)
			{
				_resultNameText.text = ResolveRewardName(reward);
			}

			if (_resultRarityText != null)
			{
				var rarity = character?.metadata?.rarity ?? (reward.IsRare ? "SSR" : "R");
				_resultRarityText.text = rarity;
				_resultRarityText.color = GetRarityColor(rarity);
			}

			if (_resultRoleText != null)
			{
				_resultRoleText.text = $"Role: {character?.metadata?.roleTag ?? "-"}";
			}

			if (_resultElementText != null)
			{
				_resultElementText.text = $"Element: {character?.metadata?.element ?? "-"}";
			}

			if (_resultExtraText != null)
			{
				var duplicateSuffix = reward.IsDuplicateConverted
					? $" | Duplicate -> +{reward.ConvertedCurrencyAmount} gold"
					: string.Empty;
				_resultExtraText.text = $"{_activeResultIndex + 1}/{_activeRollResult.Rewards.Count} | x{Math.Max(1, reward.Amount)}{duplicateSuffix}";
			}

			UpdateResultPortrait(reward);

			if (_resultNextButton != null)
			{
				_resultNextButton.gameObject.SetActive(_activeResultIndex < _activeRollResult.Rewards.Count - 1);
			}
		}

		private void UpdateResultPortrait(GachaRollReward reward)
		{
			if (_resultPortrait == null)
			{
				return;
			}

			_resultPortrait.sprite = null;
			_resultPortrait.color = new Color(1f, 1f, 1f, 0f);

			if (reward == null)
			{
				return;
			}

			// Character reward: try to show portrait.
			if (string.Equals(reward.RewardType, "character", StringComparison.OrdinalIgnoreCase))
			{
				var character = _dataManager?.LoadCharacter(reward.RewardId);
				var portraitPath = character?.visual?.portraitPath;
				if (!string.IsNullOrWhiteSpace(portraitPath))
				{
					var sprite = _dataManager?.LoadCharacterPortraitSprite(portraitPath);
					if (sprite != null)
					{
						_resultPortrait.sprite = sprite;
						_resultPortrait.color = Color.white;
					}
				}
			}
		}

		private void OnResultNextClicked()
		{
			if (_activeRollResult == null || _activeRollResult.Rewards == null)
			{
				return;
			}

			if (_activeResultIndex < _activeRollResult.Rewards.Count - 1)
			{
				_activeResultIndex++;
				UpdateResultView();
			}
		}

		private void OnResultCloseClicked()
		{
			_activeRollResult = null;
			_activeResultIndex = 0;
			SetResultPanelVisible(false);
		}

		private string ResolveRewardName(GachaRollReward reward)
		{
			if (reward == null || string.IsNullOrWhiteSpace(reward.RewardId))
			{
				return "Unknown Reward";
			}

			if (string.Equals(reward.RewardType, "character", StringComparison.OrdinalIgnoreCase))
			{
				var character = _dataManager?.LoadCharacter(reward.RewardId);
				if (!string.IsNullOrWhiteSpace(character?.nameKey))
				{
					return character.nameKey;
				}
			}
			else
			{
				var item = _dataManager?.LoadItem(reward.RewardId);
				if (!string.IsNullOrWhiteSpace(item?.nameKey))
				{
					return item.nameKey;
				}
			}

			return reward.RewardId;
		}

		private void RefreshPoolInfo()
		{
			var poolInfo = _gachaService?.GetPoolInfo(_selectedPoolId) ?? new GachaPoolInfo(_selectedPoolId, 0, 10);
			var pool = GetSelectedPool();

			if (_saveManager != null && _saveManager.CurrentSave == null)
			{
				_saveManager.EnsureCurrentSave(0);
			}

			if (_poolNameText != null)
			{
				_poolNameText.text = string.IsNullOrWhiteSpace(pool?.nameKey) ? _selectedPoolId : pool.nameKey;
			}

			if (_pityText != null)
			{
				_pityText.text = $"Pity: {poolInfo.PityCount}/{Math.Max(1, poolInfo.PityThreshold)}";
			}

			if (_goldText != null)
			{
				_goldText.text = $"Gold: {(_saveManager?.CurrentSave?.gold ?? 0):N0}";
			}

			if (_goldIcon != null)
			{
				var icon = LoadGoldIconSprite();
				_goldIcon.sprite = icon;
				_goldIcon.color = icon == null ? new Color(1f, 0.85f, 0f, 1f) : Color.white;
			}

			if (_rollCostText != null)
			{
				var oneCost = Math.Max(0, pool?.rollCostSingle ?? 160);
				var tenCost = Math.Max(0, pool?.rollCostTen ?? 1600);
				_rollCostText.text = $"Cost 1x: {oneCost:N0} | Cost 10x: {tenCost:N0}";
			}

			if (_bannerBackgroundImage != null)
			{
				var bannerSprite = LoadBannerSprite(pool?.bannerBackgroundPath);
				_bannerBackgroundImage.sprite = bannerSprite;
				_bannerBackgroundImage.color = bannerSprite == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
			}
		}

		private GachaPoolDataModel GetSelectedPool()
		{
			for (var i = 0; i < _pools.Count; i++)
			{
				if (string.Equals(_pools[i].id, _selectedPoolId, StringComparison.Ordinal))
				{
					return _pools[i];
				}
			}

			return null;
		}

		private static Sprite LoadBannerSprite(string bannerPath)
		{
			if (string.IsNullOrWhiteSpace(bannerPath))
			{
				return null;
			}

			return Resources.Load<Sprite>(bannerPath);
		}

		private static Sprite LoadGoldIconSprite()
		{
			var candidates = new[]
			{
				"UI/icon_gold",
				"Icons/icon_gold",
				"Icons/UI/icon_gold",
				"Sprites/UI/icon_gold",
				"Sprites/Items/item_gold",
				"Items/item_gold",
				"item_gold"
			};

			for (var i = 0; i < candidates.Length; i++)
			{
				var sprite = Resources.Load<Sprite>(candidates[i]);
				if (sprite != null)
				{
					return sprite;
				}
			}

			var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
			tex.SetPixel(0, 0, new Color(1f, 0.84f, 0f, 1f));
			tex.Apply();
			return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
		}

		private static Color GetRarityColor(string rarity)
		{
			if (string.Equals(rarity, "SSR", StringComparison.OrdinalIgnoreCase))
			{
				return new Color(1f, 0.84f, 0f, 1f);
			}

			if (string.Equals(rarity, "SR", StringComparison.OrdinalIgnoreCase))
			{
				return new Color(1f, 0.5f, 0f, 1f);
			}

			return Color.white;
		}

		private void SetResultPanelVisible(bool visible)
		{
			if (_resultPanel != null)
			{
				_resultPanel.SetActive(visible);
			}
		}

		private static void OnBackClicked()
		{
			FlowController.Instance.OpenMainMenu();
		}
	}
}

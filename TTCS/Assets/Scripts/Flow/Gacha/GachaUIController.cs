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
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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
		[SerializeField] private TextMeshProUGUI _goldText;
		

		[Header("Banner List (Left)")]
		[SerializeField] private Transform _bannerListRoot;
		[SerializeField] private GachaBannerListItemView _bannerItemPrefab;

		[Header("Banner Preview (Right)")]
		[SerializeField] private Image _bannerBackgroundImage;

		[Header("Roll Actions")]
		[SerializeField] private Button _rollOneButton;
		[SerializeField] private TextMeshProUGUI _rollOneCostText;
		[SerializeField] private Button _rollTenButton;
		[SerializeField] private TextMeshProUGUI _rollTenCostText;

		[Header("Result Panel")]
		[SerializeField] private GameObject _resultPanel;
		[SerializeField] private Image _resultPortrait;
		[SerializeField] private TextMeshProUGUI _resultNameText;
		[SerializeField] private TextMeshProUGUI _resultRarityText;
		[SerializeField] private Image _resultRoleIcon;
		[SerializeField] private Image _resultElementIcon;
		[SerializeField] private Button _resultCloseButton;
		[SerializeField] private RectTransform _resultCardRoot;
		[SerializeField] private RectTransform _rarityStarRoot;
		[SerializeField] private Image _rarityStarPrefab;
		[SerializeField] private float _rarityStarRevealInterval = 0.08f;
		[SerializeField] private GachaTransitionController _transitionController;
		

		private IGachaService _gachaService;
		private DataManager _dataManager;
		private SaveManager _saveManager;
		private readonly List<GachaPoolDataModel> _pools = new List<GachaPoolDataModel>();
		private readonly List<GachaBannerListItemView> _spawnedBannerItems = new List<GachaBannerListItemView>();
		private string _selectedPoolId = "pool_standard";
		private GachaRollResult _activeRollResult;
		private int _activeResultIndex;
		private bool _consumeFirstResultTap;
		private bool _isResultItemTransitionPlaying;
		private Coroutine _rarityStarRoutine;
		private readonly List<GameObject> _spawnedRarityStars = new List<GameObject>();
		private GachaRollResult _pendingRollResult;
		private int _pendingRollCount;

		private void Start()
		{
			_gachaService = MetaServiceHub.Instance?.GachaService;
			_dataManager = DataManager.Instance;
			_saveManager = SaveManager.Instance;
			_saveManager?.EnsureCurrentSave(0);

			WireButtons();
			WireTransitionController();
			BuildBannerList();
			RefreshPoolInfo();
			SetResultPanelVisible(false);
		}

		private void OnDestroy()
		{
			UnwireTransitionController();
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

		}

		private void WireTransitionController()
		{
			if (_transitionController == null)
			{
				Debug.LogWarning("[GachaUI] TransitionController is not assigned. Roll/result will work but no transition effect.");
				return;
			}

			_transitionController.OnSwapToResult += HandleSwapToResultScreen;
			_transitionController.OnResultItemSwap += HandleResultItemSwap;
			Debug.Log("[GachaUI] TransitionController wired successfully.");
		}

		private void UnwireTransitionController()
		{
			if (_transitionController == null)
			{
				return;
			}

			_transitionController.OnSwapToResult -= HandleSwapToResultScreen;
			_transitionController.OnResultItemSwap -= HandleResultItemSwap;
		}

		private void Update()
		{
			if (_resultPanel == null || !_resultPanel.activeSelf)
			{
				return;
			}

			if (_activeRollResult == null || _activeRollResult.Rewards == null || _activeRollResult.Rewards.Count == 0)
			{
				return;
			}

			if (_isResultItemTransitionPlaying)
			{
				return;
			}

			if (IsAdvanceResultInputPressed())
			{
				if (_consumeFirstResultTap)
				{
					_consumeFirstResultTap = false;
					return;
				}

				OnResultNextByTap();
			}
		}

		private static bool IsAdvanceResultInputPressed()
		{
#if ENABLE_INPUT_SYSTEM
			var mousePressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
			var touchPressed = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
			return mousePressed || touchPressed;
#else
			return Input.GetMouseButtonDown(0);
#endif
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
				var bannerSprite = LoadBannerSprite(pool.bannerBackgroundPath);
				item.Bind(pool.id, bannerSprite, OnBannerSelected);
				item.SetSelected(i == 0);
				_spawnedBannerItems.Add(item);
			}

			OnBannerSelected(_selectedPoolId);
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

			var result = _gachaService.Roll(_selectedPoolId, count);
			_gachaService.ApplyRollResult(result);

			if (_transitionController != null)
			{
				_pendingRollResult = result;
				_pendingRollCount = count;
				SetResultPanelVisible(false);

				var rarityColor = ResolveRarityColorFromResult(result);
				var rarityTag = ResolveRarityTagFromResult(result);
				_transitionController.PlayGachaAnimation(rarityColor, rarityTag);
			}
			else
			{
				ShowRollResult(result, count);
			}

			RefreshPoolInfo();
		}

		private Color ResolveRarityColorFromResult(GachaRollResult result)
		{
			if (result?.Rewards == null || result.Rewards.Count == 0)
			{
				return Color.white;
			}

			var reward = result.Rewards[0];
			var character = string.Equals(reward.RewardType, "character", StringComparison.OrdinalIgnoreCase)
				? _dataManager?.LoadCharacter(reward.RewardId)
				: null;
			var rarity = character?.metadata?.rarity ?? (reward.IsRare ? "SSR" : "R");

			if (string.Equals(rarity, "SSR", StringComparison.OrdinalIgnoreCase))
			{
				return HexToColor("#FFD700");
			}

			if (string.Equals(rarity, "SR", StringComparison.OrdinalIgnoreCase))
			{
				return HexToColor("#FF007F");
			}

			if (string.Equals(rarity, "R", StringComparison.OrdinalIgnoreCase))
			{
				return HexToColor("#00F0FF");
			}

			return Color.white;
		}

		private string ResolveRarityTagFromResult(GachaRollResult result)
		{
			if (result?.Rewards == null || result.Rewards.Count == 0)
			{
				return "R";
			}

			return ResolveRarityTagFromReward(result.Rewards[0]);
		}

		private void HandleSwapToResultScreen()
		{
			if (_pendingRollResult == null)
			{
				return;
			}

			ShowRollResult(_pendingRollResult, _pendingRollCount);
			_pendingRollResult = null;
			_pendingRollCount = 0;
		}

		private void HandleResultItemSwap()
		{
			_isResultItemTransitionPlaying = false;
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
				Debug.LogWarning($"[GachaUI] Not enough gold. Need {cost:N0}, current {save.gold:N0}.");
				return false;
			}

			return true;
		}

		private void ShowRollResult(GachaRollResult result, int rollCount)
		{
			_activeRollResult = result;
			_activeResultIndex = 0;
			_consumeFirstResultTap = true;

			SetResultPanelVisible(true);

			if (result == null || result.Rewards == null || result.Rewards.Count == 0)
			{
				if (_resultNameText != null) _resultNameText.text = "No Reward";
				if (_resultRarityText != null)
				{
					_resultRarityText.text = "-";
					ResetResultRarityStyle();
				}
				SetResultIcon(_resultRoleIcon, null);
				SetResultIcon(_resultElementIcon, null);
				ClearSpawnedRarityStars();
				if (_resultPortrait != null)
				{
					_resultPortrait.sprite = null;
					_resultPortrait.color = new Color(1f, 1f, 1f, 0f);
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
				ApplyResultRarityStyle(rarity);
				StartRarityStarReveal(rarity);

				if (_transitionController != null)
				{
					_transitionController.PlayResultCardEntrance(_resultCardRoot, rarity);
					
					// Play SSR seal effect for every SSR result
					if (string.Equals(rarity, "SSR", StringComparison.OrdinalIgnoreCase))
					{
						_transitionController.PlaySsrSealEffectDirect();
					}
				}
			}

			var roleTag = character?.metadata?.roleTag ?? "-";
			SetResultIcon(_resultRoleIcon, LoadRoleIconSprite(roleTag));

			var elementTag = character?.metadata?.element ?? "-";
			SetResultIcon(_resultElementIcon, LoadElementIconSprite(elementTag));

			UpdateResultPortrait(reward);

			
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
				return;
			}

			if (string.Equals(reward.RewardType, "item", StringComparison.OrdinalIgnoreCase))
			{
				var item = _dataManager?.LoadItem(reward.RewardId);
				if (!string.IsNullOrWhiteSpace(item?.iconPath))
				{
					var sprite = Resources.Load<Sprite>(item.iconPath);
					if (sprite != null)
					{
						_resultPortrait.sprite = sprite;
						_resultPortrait.color = Color.white;
					}
				}
			}
		}

		private void OnResultNextByTap()
		{
			if (_activeRollResult == null || _activeRollResult.Rewards == null)
			{
				return;
			}

			if (_activeResultIndex >= _activeRollResult.Rewards.Count - 1)
			{
				if (_transitionController != null)
				{
					_transitionController.ReturnToBannerScreen();
				}

				OnResultCloseClicked();
				return;
			}

			if (_transitionController != null)
			{
				_isResultItemTransitionPlaying = true;
				var nextReward = _activeRollResult.Rewards[_activeResultIndex + 1];
				var rarityColor = ResolveRarityColorFromReward(nextReward);
				var rarityTag = ResolveRarityTagFromReward(nextReward);
				_transitionController.PlayResultItemTransition(rarityColor, rarityTag);
				return;
			}

			if (_activeResultIndex < _activeRollResult.Rewards.Count - 1)
			{
				_activeResultIndex++;
				UpdateResultView();
			}
		}

		private Color ResolveRarityColorFromReward(GachaRollReward reward)
		{
			if (reward == null)
			{
				return Color.white;
			}

			var character = string.Equals(reward.RewardType, "character", StringComparison.OrdinalIgnoreCase)
				? _dataManager?.LoadCharacter(reward.RewardId)
				: null;
			var rarity = character?.metadata?.rarity ?? (reward.IsRare ? "SSR" : "R");

			if (string.Equals(rarity, "SSR", StringComparison.OrdinalIgnoreCase))
			{
				return HexToColor("#FFD700");
			}

			if (string.Equals(rarity, "SR", StringComparison.OrdinalIgnoreCase))
			{
				return HexToColor("#FF007F");
			}

			if (string.Equals(rarity, "R", StringComparison.OrdinalIgnoreCase))
			{
				return HexToColor("#00F0FF");
			}

			return Color.white;
		}

		private string ResolveRarityTagFromReward(GachaRollReward reward)
		{
			if (reward == null)
			{
				return "R";
			}

			var character = string.Equals(reward.RewardType, "character", StringComparison.OrdinalIgnoreCase)
				? _dataManager?.LoadCharacter(reward.RewardId)
				: null;
			var rarity = character?.metadata?.rarity ?? (reward.IsRare ? "SSR" : "R");

			if (string.Equals(rarity, "SSR", StringComparison.OrdinalIgnoreCase))
			{
				return "SSR";
			}

			if (string.Equals(rarity, "SR", StringComparison.OrdinalIgnoreCase))
			{
				return "SR";
			}

			return "R";
		}

		private void OnResultCloseClicked()
		{
			if (_rarityStarRoutine != null)
			{
				StopCoroutine(_rarityStarRoutine);
				_rarityStarRoutine = null;
			}

			ClearSpawnedRarityStars();

			_activeRollResult = null;
			_activeResultIndex = 0;
			_consumeFirstResultTap = false;
			_isResultItemTransitionPlaying = false;
			_pendingRollResult = null;
			_pendingRollCount = 0;
			
			// Return to banner screen before hiding result panel
			if (_transitionController != null)
			{
				_transitionController.ReturnToBannerScreen();
			}
			
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
			var pool = GetSelectedPool();

			if (_saveManager != null && _saveManager.CurrentSave == null)
			{
				_saveManager.EnsureCurrentSave(0);
			}

			

			var oneCost = Math.Max(0, pool?.rollCostSingle ?? 160);
			var tenCost = Math.Max(0, pool?.rollCostTen ?? 1600);

			if (_goldText != null)
			{
				_goldText.text = $"Gold: {(_saveManager?.CurrentSave?.gold ?? 0):N0}";
			}

			if (_rollOneCostText != null)
			{
				_rollOneCostText.text = $"{oneCost:N0}";
			}

			if (_rollTenCostText != null)
			{
				_rollTenCostText.text = $"{tenCost:N0}";
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

		private void ApplyResultRarityStyle(string rarity)
		{
			if (_resultRarityText == null)
			{
				return;
			}

			_resultRarityText.enableVertexGradient = false;
			_resultRarityText.colorGradient = default;

			if (string.Equals(rarity, "SSR", StringComparison.OrdinalIgnoreCase))
			{
				var topColor = HexToColor("#FFFFB3FF");
				var bottomColor = HexToColor("#FFB300FF");
				_resultRarityText.color = HexToColor("#FFD700");
				_resultRarityText.enableVertexGradient = true;
				_resultRarityText.colorGradient = new VertexGradient(topColor, topColor, bottomColor, bottomColor);
				return;
			}

			if (string.Equals(rarity, "SR", StringComparison.OrdinalIgnoreCase))
			{
				_resultRarityText.color = HexToColor("#FF007F");
				return;
			}

			if (string.Equals(rarity, "R", StringComparison.OrdinalIgnoreCase))
			{
				_resultRarityText.color = HexToColor("#00F0FF");
				return;
			}

			_resultRarityText.color = Color.white;
		}

		private void ResetResultRarityStyle()
		{
			if (_resultRarityText == null)
			{
				return;
			}

			_resultRarityText.enableVertexGradient = false;
			_resultRarityText.colorGradient = default;
			_resultRarityText.color = Color.white;
		}

		private static Color HexToColor(string hex)
		{
			if (ColorUtility.TryParseHtmlString(hex, out var color))
			{
				return color;
			}

			return Color.white;
		}

		private void StartRarityStarReveal(string rarity)
		{
			var starCount = GetRarityStarCount(rarity);

			if (_rarityStarRoutine != null)
			{
				StopCoroutine(_rarityStarRoutine);
			}

			_rarityStarRoutine = StartCoroutine(PlayRarityStarsRoutine(starCount));
		}

		private IEnumerator PlayRarityStarsRoutine(int starCount)
		{
			ClearSpawnedRarityStars();

			if (_rarityStarRoot == null || _rarityStarPrefab == null)
			{
				yield break;
			}

			var safeCount = Math.Max(0, starCount);
			for (var i = 0; i < safeCount; i++)
			{
				var star = Instantiate(_rarityStarPrefab, _rarityStarRoot, false);
				star.gameObject.SetActive(true);
				_spawnedRarityStars.Add(star.gameObject);

				var wait = _rarityStarRevealInterval < 0f ? 0f : _rarityStarRevealInterval;
				if (wait > 0f)
				{
					yield return new WaitForSeconds(wait);
				}
			}

			_rarityStarRoutine = null;
		}

		private void ClearSpawnedRarityStars()
		{
			for (var i = 0; i < _spawnedRarityStars.Count; i++)
			{
				var star = _spawnedRarityStars[i];
				if (star != null)
				{
					Destroy(star);
				}
			}

			_spawnedRarityStars.Clear();
		}

		private static int GetRarityStarCount(string rarity)
		{
			if (string.Equals(rarity, "SSR", StringComparison.OrdinalIgnoreCase))
			{
				return 5;
			}

			if (string.Equals(rarity, "SR", StringComparison.OrdinalIgnoreCase))
			{
				return 4;
			}

			if (string.Equals(rarity, "R", StringComparison.OrdinalIgnoreCase))
			{
				return 3;
			}

			return 1;
		}

		private static void SetResultIcon(Image target, Sprite sprite)
		{
			if (target == null)
			{
				return;
			}

			target.sprite = sprite;
			target.color = sprite == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
		}

		private static Sprite LoadRoleIconSprite(string roleTag)
		{
			var key = NormalizeTag(roleTag);
			if (string.IsNullOrEmpty(key))
			{
				return null;
			}

			var candidates = new[]
			{
				$"UI/Role/icon_role_{key}",
				$"Icons/Role/icon_role_{key}",
				$"Role/{key}",
				$"UI/Role/{key}",
				$"Icons/{key}"
			};

			for (var i = 0; i < candidates.Length; i++)
			{
				var sprite = Resources.Load<Sprite>(candidates[i]);
				if (sprite != null)
				{
					return sprite;
				}
			}

			return null;
		}

		private static Sprite LoadElementIconSprite(string elementTag)
		{
			var key = NormalizeTag(elementTag);
			if (string.IsNullOrEmpty(key))
			{
				return null;
			}

			var candidates = new[]
			{
				$"UI/Element/icon_element_{key}",
				$"Icons/Element/icon_element_{key}",
				$"Element/{key}",
				$"UI/Element/{key}",
				$"Icons/{key}"
			};

			for (var i = 0; i < candidates.Length; i++)
			{
				var sprite = Resources.Load<Sprite>(candidates[i]);
				if (sprite != null)
				{
					return sprite;
				}
			}

			return null;
		}

		private static string NormalizeTag(string value)
		{
			return string.IsNullOrWhiteSpace(value)
				? string.Empty
				: value.Trim().ToLowerInvariant();
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

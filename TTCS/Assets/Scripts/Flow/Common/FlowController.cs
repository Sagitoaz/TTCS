using System.Collections.Generic;
using UnityEngine;
using TTCS.Core;
using TTCS.Meta;
using UnityEngine.SceneManagement;

namespace TTCS.Flow
{
    /// <summary>
    /// Main orchestrator for scene flow and navigation.
    /// Implements IFlowController interface locked in Phase 1.
    /// Dev B - Ngày 2-3 Sprint 03.
    /// </summary>
    public class FlowController : MonoBehaviour, IFlowController
    {
        [SerializeField] private string _bootSceneName = "Boot";
        [SerializeField] private string _tutorialSceneName = "TutorialScene";
        [SerializeField] private string _mainMenuSceneName = "MainMenuScene";
        [SerializeField] private string _levelSelectSceneName = "LevelSelectScene";
        [SerializeField] private string _combatSceneName = "CombatScene";

        private IProgressionService _progressionService;
        private ITeamService _teamService;
        private IGachaService _gachaService;
        private IInventoryService _inventoryService;
        private bool _startupRouteDone;

        private static FlowController _instance;

        public static FlowController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<FlowController>();
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }

            InitializeServices();
        }

        private void Start()
        {
            TryRouteFromBoot();
        }

        private void TryRouteFromBoot()
        {
            if (_startupRouteDone)
            {
                return;
            }

            string activeSceneName = SceneManager.GetActiveScene().name;
            if (!string.Equals(activeSceneName, _bootSceneName))
            {
                return;
            }

            _startupRouteDone = true;

            if (!TryEnterTutorial())
            {
                OpenMainMenu();
            }
        }

        private void InitializeServices()
        {
            // TODO: When DevA merges, these will be injected from ServiceManager
            // For now, use mock implementations
            _progressionService = GetComponent<IProgressionService>() ?? new MockProgressionService();
            _teamService = GetComponent<ITeamService>() ?? new MockTeamService();
            _gachaService = GetComponent<IGachaService>() ?? new MockGachaService();
            _inventoryService = GetComponent<IInventoryService>() ?? new MockInventoryService();

            Debug.Log("[Flow] FlowController initialized with services");
        }

        public bool TryEnterTutorial()
        {
            // Check if tutorial was already completed
            bool tutorialCompleted = PlayerPrefs.GetInt("TutorialCompleted", 0) == 1;

            if (!tutorialCompleted)
            {
                Debug.Log("[Flow] Entering tutorial scene (first-time player)");
                UnityEngine.SceneManagement.SceneManager.LoadScene(_tutorialSceneName);
                return true;
            }
            else
            {
                Debug.Log("[Flow] Tutorial already completed, skipping");
                return false;
            }
        }

        public void OpenMainMenu()
        {
            Debug.Log("[Flow] Opening main menu");
            UnityEngine.SceneManagement.SceneManager.LoadScene(_mainMenuSceneName);
        }

        public void OpenLevelSelect(string chapterId)
        {
            Debug.Log($"[Flow] Opening level select for chapter: {chapterId}");
            // TODO: Pass chapterId to LevelSelectController
            UnityEngine.SceneManagement.SceneManager.LoadScene(_levelSelectSceneName);
        }

        public void EnterCombat(string levelId, IReadOnlyList<string> lineupSnapshot)
        {
            Debug.Log($"[Flow] Entering combat: levelId={levelId}, lineup count={lineupSnapshot.Count}");
            // TODO: Pass levelId and lineup to CombatSceneManager
            UnityEngine.SceneManagement.SceneManager.LoadScene(_combatSceneName);
        }

        public void HandleCombatResult(CombatResult result)
        {
            Debug.Log($"[Flow] Combat ended: levelId={result.LevelId}, victory={result.Victory}, stars={result.Stars}");

            if (result.Victory)
            {
                _progressionService.MarkLevelCompleted(result.LevelId, result.Stars, result.Score);
                _progressionService.TryUnlockNextContent();
                Debug.Log("[Flow] Progression updated");
            }

            // Return to level select or main menu
            // TODO: Show result screen first
            OpenLevelSelect("chapter_01"); // Placeholder
        }
    }

    // ===== Mock Implementations for Dev B to test without DevA =====

    public class MockProgressionService : IProgressionService
    {
        public ChapterState GetChapterState(string chapterId)
        {
            return new ChapterState
            {
                ChapterId = chapterId,
                Unlocked = true,
                CompletedLevels = 0,
                TotalStars = 0
            };
        }

        public LevelState GetLevelState(string levelId)
        {
            return new LevelState
            {
                LevelId = levelId,
                Unlocked = true,
                Cleared = false,
                BestStars = 0,
                BestScore = 0
            };
        }

        public bool CanEnterLevel(string levelId) => true;

        public void MarkLevelCompleted(string levelId, int stars, int score)
        {
            Debug.Log($"[Mock] Level {levelId} completed: stars={stars}, score={score}");
        }

        public UnlockResult TryUnlockNextContent() => new UnlockResult { Success = true };
    }

    public class MockTeamService : ITeamService
    {
        public IReadOnlyList<string> GetCurrentLineup()
        {
            return new List<string> { "char_warrior", "char_mage" };
        }

        public ValidationResult ValidateLineup(IReadOnlyList<string> lineup)
        {
            return new ValidationResult { Valid = true };
        }

        public void SaveLineup(IReadOnlyList<string> lineup)
        {
            Debug.Log($"[Mock] Lineup saved: {string.Join(", ", lineup)}");
        }
    }

    public class MockGachaService : IGachaService
    {
        public GachaPoolInfo GetPoolInfo(string poolId)
        {
            return new GachaPoolInfo
            {
                PoolId = poolId,
                Name = "Standard Pool",
                CurrencyId = "gem",
                CostPer1 = 160,
                CostPer10 = 1600
            };
        }

        public GachaRollResult Roll(string poolId, int count)
        {
            return new GachaRollResult
            {
                PoolId = poolId,
                PullCount = count,
                Items = new List<GachaItem>
                {
                    new GachaItem { ItemId = "char_001", Rarity = "common", Quantity = 1 }
                }
            };
        }

        public void ApplyRollResult(GachaRollResult result)
        {
            Debug.Log($"[Mock] Gacha result applied: {result.Items.Count} items");
        }
    }

    public class MockInventoryService : IInventoryService
    {
        public IReadOnlyList<ItemStack> GetItems()
        {
            return new List<ItemStack>
            {
                new ItemStack { ItemId = "potion", Quantity = 5 },
                new ItemStack { ItemId = "ether", Quantity = 2 }
            };
        }

        public bool CanUseItem(string itemId, string targetContext) => true;

        public UseItemResult UseItem(string itemId, int quantity, string targetContext)
        {
            return new UseItemResult
            {
                Success = true,
                Message = "Item used",
                QuantityUsed = quantity
            };
        }
    }
}

using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TTCS.Meta;
using UnityEngine.SceneManagement;
using TTCS.Core.Events;
using TTCS.Meta.Gacha;
using TTCS.Meta.Inventory;
using TTCS.Meta.Progression;
using TTCS.Meta.Team;

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
        [SerializeField] private string _teamFormationSceneName = "TeamFormationScene";
        [SerializeField] private string _gachaSceneName = "GachaScene";
        [SerializeField] private string _inventorySceneName = "InventoryScene";
        [SerializeField] private string _characterCollectionSceneName = "CharacterCollectionScene";
        [SerializeField] private string _levelSelectSceneName = "LevelSelectScene";
        [SerializeField] private string _combatSceneName = "CombatScene";

        private IProgressionService _progressionService;
        private ITeamService _teamService;
        private IGachaService _gachaService;
        private IInventoryService _inventoryService;
        private FlowStateManager _flowStateManager;
        private NavigationController _navigationController;
        private SceneTransitionController _sceneTransitionController;
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

            _flowStateManager = new FlowStateManager();
            _navigationController = GetComponent<NavigationController>();
            if (_navigationController == null)
            {
                _navigationController = gameObject.AddComponent<NavigationController>();
            }

            _sceneTransitionController = GetComponent<SceneTransitionController>();
            if (_sceneTransitionController == null)
            {
                _sceneTransitionController = gameObject.AddComponent<SceneTransitionController>();
            }
        }

        private void OnEnable()
        {
            EventBus.Instance.Subscribe<TutorialCompletedEvent>(OnTutorialCompleted);
        }

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<TutorialCompletedEvent>(OnTutorialCompleted);
        }

        private void Start()
        {
            StartCoroutine(BootstrapAndRoute());
        }

        private IEnumerator BootstrapAndRoute()
        {
            // Wait a few frames for SaveManager/MetaServiceHub boot order.
            const int maxWaitFrames = 120;
            int waited = 0;

            while (_progressionService == null && waited < maxWaitFrames)
            {
                InitializeServices();
                if (_progressionService != null)
                {
                    break;
                }

                waited++;
                yield return null;
            }

            if (_progressionService == null)
            {
                Debug.LogWarning("[Flow] ProgressionService still null after startup wait; using tutorial fallback mode.");
            }

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
            var hub = MetaServiceHub.Instance;
            if (hub == null)
            {
                Debug.LogWarning("[Flow] MetaServiceHub not found. Services can be null until hub is initialized.");
                return;
            }

            hub.EnsureInitialized();
            _progressionService = hub.ProgressionService;
            _teamService = hub.TeamService;
            _gachaService = hub.GachaService;
            _inventoryService = hub.InventoryService;

            Debug.Log("[Flow] FlowController initialized with DevA services from MetaServiceHub");
        }

        public bool TryEnterTutorial()
        {
            bool tutorialCompleted = _progressionService != null
                ? _progressionService.IsTutorialCompleted()
                : PlayerPrefs.GetInt("TutorialCompleted", 0) == 1;

            if (!tutorialCompleted)
            {
                Debug.Log("[Flow] Entering tutorial scene (first-time player)");
                _flowStateManager.NavigateTo(_tutorialSceneName);
                _sceneTransitionController.LoadScene(_tutorialSceneName);
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
            _flowStateManager.NavigateTo(_mainMenuSceneName);
            _sceneTransitionController.LoadScene(_mainMenuSceneName);
        }

        public void OpenLevelSelect(string chapterId)
        {
            Debug.Log($"[Flow] Opening level select for chapter: {chapterId}");
            FlowRuntimeContext.SelectedChapterId = string.IsNullOrWhiteSpace(chapterId) ? "chapter_01" : chapterId;
            _flowStateManager.NavigateTo(_levelSelectSceneName);
            _sceneTransitionController.LoadScene(_levelSelectSceneName);
        }

        public void OpenTeamSelection()
        {
            Debug.Log("[Flow] Opening team formation scene");
            _flowStateManager.NavigateTo(_teamFormationSceneName);
            _sceneTransitionController.LoadScene(_teamFormationSceneName);
        }

        public void OpenGacha()
        {
            Debug.Log("[Flow] Opening gacha scene");
            _flowStateManager.NavigateTo(_gachaSceneName);
            _sceneTransitionController.LoadScene(_gachaSceneName);
        }

        public void OpenInventory()
        {
            Debug.Log("[Flow] Opening inventory scene");
            _flowStateManager.NavigateTo(_inventorySceneName);
            _sceneTransitionController.LoadScene(_inventorySceneName);
        }

        public void OpenCharacterCollection()
        {
            Debug.Log("[Flow] Opening character collection scene");
            _flowStateManager.NavigateTo(_characterCollectionSceneName);
            _sceneTransitionController.LoadScene(_characterCollectionSceneName);
        }

        public void EnterCombat(string levelId, IReadOnlyList<string> lineupSnapshot)
        {
            if (string.IsNullOrWhiteSpace(levelId))
            {
                Debug.LogWarning("[Flow] EnterCombat aborted: levelId is empty.");
                return;
            }

            var safeLineup = new List<string>();
            if (lineupSnapshot != null)
            {
                for (var i = 0; i < lineupSnapshot.Count; i++)
                {
                    var id = lineupSnapshot[i];
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        safeLineup.Add(id);
                    }
                }
            }

            if (safeLineup.Count == 0)
            {
                var currentLineup = _teamService?.GetCurrentLineup();
                if (currentLineup != null)
                {
                    for (var i = 0; i < currentLineup.Count; i++)
                    {
                        var id = currentLineup[i];
                        if (!string.IsNullOrWhiteSpace(id))
                        {
                            safeLineup.Add(id);
                        }
                    }
                }
            }

            Debug.Log($"[Flow] Entering combat: levelId={levelId}, lineup count={safeLineup.Count}");
            FlowRuntimeContext.SelectedLevelId = levelId;
            FlowRuntimeContext.SelectedLineupSnapshot = safeLineup;
            _flowStateManager.NavigateTo(_combatSceneName);
            EventBus.Instance.Publish(new LevelEnteredEvent(levelId));
            _sceneTransitionController.LoadScene(_combatSceneName);
        }

        public void HandleCombatResult(CombatResult result)
        {
            Debug.Log($"[Flow] Combat ended: levelId={result.LevelId}, victory={result.Victory}, stars={result.Stars}");
            FlowRuntimeContext.LastCombatResult = result;
            EventBus.Instance.Publish(new CombatResultReceivedEvent(result.LevelId, result.Victory, result.Stars, result.Score));

            if (result.Victory)
            {
                if (_progressionService != null)
                {
                    _progressionService.MarkLevelCompleted(result.LevelId, result.Stars, result.Score);
                    _progressionService.TryUnlockNextContent();
                    Debug.Log("[Flow] Progression updated");
                }
                else
                {
                    Debug.LogWarning("[Flow] ProgressionService is null, skipping progression update");
                }
            }

            // Return to level select or main menu
            // TODO: Show result screen first
            OpenLevelSelect(FlowRuntimeContext.SelectedChapterId);
        }

        private void OnTutorialCompleted(TutorialCompletedEvent eventData)
        {
            if (_progressionService != null)
            {
                _progressionService.MarkTutorialCompleted();
            }
            else
            {
                // Fallback while save integration is not ready in specific boot order cases.
                PlayerPrefs.SetInt("TutorialCompleted", 1);
                PlayerPrefs.Save();
            }

            Debug.Log($"[Flow] Tutorial completion received (skipped={eventData.Skipped})");
            OpenMainMenu();
        }
    }
}

using System.Collections;
using UnityEngine;
using TTCS.Core.Save;
using TTCS.Meta.Gacha;
using TTCS.Meta.Inventory;
using TTCS.Meta.Progression;
using TTCS.Meta.Team;

namespace TTCS.Meta
{
    public sealed class MetaServiceHub : MonoBehaviour
    {
        private static MetaServiceHub _instance;
        public static MetaServiceHub Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<MetaServiceHub>();
                }

                return _instance;
            }
        }

        public IInventoryService InventoryService { get; private set; }
        public IGachaService GachaService { get; private set; }
        public IProgressionService ProgressionService { get; private set; }
        public ITeamService TeamService { get; private set; }
        public bool IsReady => InventoryService != null && GachaService != null && ProgressionService != null && TeamService != null;

        private Coroutine _initializeRoutine;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            EnsureInitialized();
        }

        public bool InitializeServices()
        {
            var saveManager = SaveManager.Instance;
            if (saveManager == null)
            {
                return false;
            }

            InventoryService ??= new InventoryService(saveManager);
            GachaService ??= new GachaService(saveManager, InventoryService);
            ProgressionService ??= new ProgressionService(saveManager);
            TeamService ??= new TeamService(saveManager);

            return IsReady;
        }

        public void EnsureInitialized()
        {
            if (IsReady)
            {
                return;
            }

            if (InitializeServices())
            {
                return;
            }

            if (_initializeRoutine == null)
            {
                _initializeRoutine = StartCoroutine(InitializeWhenReady());
            }
        }

        private IEnumerator InitializeWhenReady()
        {
            while (!IsReady)
            {
                InitializeServices();
                if (IsReady)
                {
                    break;
                }

                yield return null;
            }

            _initializeRoutine = null;
        }
    }
}

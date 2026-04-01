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
        public static MetaServiceHub Instance => _instance;

        public IInventoryService InventoryService { get; private set; }
        public IGachaService GachaService { get; private set; }
        public IProgressionService ProgressionService { get; private set; }
        public ITeamService TeamService { get; private set; }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeServices();
        }

        public void InitializeServices()
        {
            var saveManager = SaveManager.Instance;
            if (saveManager == null)
            {
                return;
            }

            InventoryService ??= new InventoryService(saveManager);
            GachaService ??= new GachaService(saveManager, InventoryService);
            ProgressionService ??= new ProgressionService(saveManager);
            TeamService ??= new TeamService(saveManager);
        }
    }
}

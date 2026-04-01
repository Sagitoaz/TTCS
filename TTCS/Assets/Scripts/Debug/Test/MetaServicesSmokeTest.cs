using UnityEngine;
using TTCS.Core.Save;
using TTCS.Meta;
using TTCS.Meta.Gacha;

namespace TTCS.Debugging.Test
{
    public class MetaServicesSmokeTest : MonoBehaviour
    {
        [ContextMenu("Run Meta Services Smoke Test")]
        public void RunSmokeTest()
        {
            if (SaveManager.Instance == null)
            {
                Debug.LogError("[MetaServicesSmokeTest] SaveManager instance not found.");
                return;
            }

            if (SaveManager.Instance.CurrentSave == null)
            {
                SaveManager.Instance.NewGame();
            }

            var hub = MetaServiceHub.Instance;
            if (hub == null)
            {
                Debug.LogError("[MetaServicesSmokeTest] MetaServiceHub instance not found.");
                return;
            }

            hub.InitializeServices();

            var lineupResult = hub.TeamService.ValidateLineup(new[] { "char_warrior" });
            if (!lineupResult.IsValid)
            {
                Debug.LogError($"[MetaServicesSmokeTest] Team validation failed: {lineupResult.Message}");
                return;
            }

            hub.InventoryService.AddItem("item_potion", 3);
            var useResult = hub.InventoryService.UseItem("item_potion", 1, "menu");
            if (!useResult.Success)
            {
                Debug.LogError($"[MetaServicesSmokeTest] UseItem failed: {useResult.Message}");
                return;
            }

            var rollResult = hub.GachaService.Roll("pool_standard", 1);
            hub.GachaService.ApplyRollResult(rollResult);

            var poolInfo = hub.GachaService.GetPoolInfo("pool_standard");
            Debug.Log($"[MetaServicesSmokeTest] PASS. Gacha pity={poolInfo.PityCount}, rewards={rollResult.Rewards.Count}");
        }
    }
}

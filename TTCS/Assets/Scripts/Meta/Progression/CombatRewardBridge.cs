using TTCS.Combat.Managers;
using TTCS.Core.Data;
using TTCS.Core.Events;
using TTCS.Debugging;
using TTCS.Meta.Gacha;
using TTCS.Meta.Inventory;
using UnityEngine;

namespace TTCS.Meta.Progression
{
    public sealed class CombatRewardBridge : MonoBehaviour
    {
        private void OnEnable()
        {
            if (EventBus.Instance != null)
            {
                EventBus.Instance.Subscribe<CombatEndedEvent>(OnCombatEnded);
            }
        }

        private void OnDisable()
        {
            if (EventBus.Instance != null)
            {
                EventBus.Instance.Unsubscribe<CombatEndedEvent>(OnCombatEnded);
            }
        }

        private void OnCombatEnded(CombatEndedEvent evt)
        {
            if (!evt.Victory || MetaServiceHub.Instance == null)
            {
                return;
            }

            // Rewards are now applied by CombatFlowController. Avoid double grant if this bridge exists in scene.
            if (CombatSceneManager.Instance != null && CombatSceneManager.Instance.RewardsAppliedThisBattle)
            {
                return;
            }

            var stage = CombatSceneManager.Instance?.GetCurrentStageData();
            var stageId = stage?.id ?? string.Empty;
            var levelId = DataManager.Instance?.ResolveLevelIdByStageId(stageId) ?? stageId;

            var progressionService = MetaServiceHub.Instance.ProgressionService;
            progressionService?.MarkLevelCompleted(levelId, stars: 3, score: 0);

            var inventoryService = MetaServiceHub.Instance.InventoryService;
            if (stage?.rewards?.firstClear?.items != null)
            {
                foreach (var reward in stage.rewards.firstClear.items)
                {
                    if (reward == null || string.IsNullOrWhiteSpace(reward.id))
                    {
                        continue;
                    }

                    inventoryService?.AddItem(reward.id, reward.amount <= 0 ? 1 : reward.amount);
                }
            }

            // Minimal gacha progression feed after victory.
            var gachaService = MetaServiceHub.Instance.GachaService;
            var rollResult = gachaService?.Roll("pool_standard", 1);
            if (rollResult != null)
            {
                gachaService.ApplyRollResult(rollResult);
            }

            DebugLogger.Log(
                $"[CombatRewardBridge] Applied rewards for stage='{stageId}' level='{levelId}'",
                DebugLogger.LogCategory.Save);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using TTCS.Combat.Entities;
using TTCS.Combat.Managers;
using TTCS.Core;
using TTCS.Data;

namespace TTCS.Combat
{
    /// <summary>
    /// Boot-strapper for the TestCombat scene.
    /// Attach to a GameObject in the scene. Requires CombatFlowController,
    /// TurnManager, SkillManager, DataManager, and SaveManager to be present.
    /// </summary>
    public class CombatTestLoader : MonoBehaviour
    {
        [Header("Party Configuration")]
        [SerializeField] private List<string> _partyCharacterIds = new List<string>
        {
            "char_warrior",
            "char_mage"
        };

        [Header("Enemy Wave Configuration")]
        [SerializeField] private List<string> _wave1EnemyIds = new List<string>
        {
            "enemy_bandit",
            "enemy_bandit"
        };

        [Header("Combat Settings")]
        [SerializeField] private int _randomSeed = 12345;
        [SerializeField] private bool _autoStartOnPlay = true;

        // ── Runtime references ────────────────────────────────────────────
        private List<Character> _party;
        private List<Enemy>     _enemies;

        // ── Debug buttons ─────────────────────────────────────────────────
        [Header("Debug — Player Input (Runtime only)")]
        [SerializeField] private string _debugSkillId    = "skill_warrior_slash";
        [SerializeField] private string _debugTargetId   = "enemy_bandit_0";

        // ─────────────────────────────────────────────────────────────────

        private void Start()
        {
            if (_autoStartOnPlay)
                StartTestCombat();
        }

        /// <summary>Call this from the Inspector Play button or from Start().</summary>
        [ContextMenu("Start Test Combat")]
        public void StartTestCombat()
        {
            Debug.Log("[CombatTestLoader] Building entities...");

            _party   = EntityFactory.CreateParty(_partyCharacterIds);
            _enemies = EntityFactory.CreateWave(_wave1EnemyIds);

            if (_party == null || _party.Count == 0)
            {
                Debug.LogError("[CombatTestLoader] Party is empty — check CharacterId JSON keys in DataManager.");
                return;
            }
            if (_enemies == null || _enemies.Count == 0)
            {
                Debug.LogError("[CombatTestLoader] Enemy wave is empty — check EnemyId JSON keys in DataManager.");
                return;
            }

            Debug.Log($"[CombatTestLoader] Party: {_party.Count} | Enemies: {_enemies.Count} | Seed: {_randomSeed}");
            CombatFlowController.Instance.StartBattle(_party, _enemies, _randomSeed);
        }

        /// <summary>
        /// Simulate player selecting a skill and a target.
        /// Bind this to a UI Button or call from another script.
        /// </summary>
        public void DebugSubmitAction()
        {
            CombatFlowController.Instance.SubmitPlayerAction(
                _debugSkillId,
                new List<string> { _debugTargetId }
            );
        }

        /// <summary>Skip the current player turn (for AI/debug testing).</summary>
        public void DebugSkipTurn()
        {
            CombatFlowController.Instance.SkipPlayerTurn();
        }

        // ── OnGUI debug panel ─────────────────────────────────────────────

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 280, 200));
            GUILayout.Label("=== CombatTestLoader Debug ===");

            if (GUILayout.Button("Start Combat"))
                StartTestCombat();

            if (GUILayout.Button($"Submit: {_debugSkillId} → {_debugTargetId}"))
                DebugSubmitAction();

            if (GUILayout.Button("Skip Player Turn"))
                DebugSkipTurn();

            if (_party != null)
            {
                foreach (var c in _party)
                    GUILayout.Label($"  {c.DisplayName}: {c.Health.CurrentHP}/{c.Health.MaxHP} HP");
            }
            if (_enemies != null)
            {
                foreach (var e in _enemies)
                    GUILayout.Label($"  {e.DisplayName}: {e.Health.CurrentHP}/{e.Health.MaxHP} HP");
            }

            GUILayout.EndArea();
        }
    }
}

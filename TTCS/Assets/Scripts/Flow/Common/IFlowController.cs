using System.Collections.Generic;
using UnityEngine;

namespace TTCS.Flow
{
    /// <summary>
    /// Orchestrates scene flow and navigation across game screens.
    /// Locked interface from Sprint 3 Phase 1 Kickoff.
    /// </summary>
    public interface IFlowController
    {

        /// <summary>
        /// Opens main menu from any scene.
        /// </summary>
        void OpenMainMenu();

        /// <summary>
        /// Opens level select for a specific chapter.
        /// </summary>
        void OpenLevelSelect(string chapterId);

        /// <summary>
        /// Opens character collection screen.
        /// </summary>
        void OpenCharacterCollection();

        /// <summary>
        /// Enters combat with a specific level and player lineup snapshot.
        /// </summary>
        void EnterCombat(string levelId, IReadOnlyList<string> lineupSnapshot);

        /// <summary>
        /// Handles end of combat, applies rewards, and determines next screen.
        /// </summary>
        void HandleCombatResult(CombatResult result);
    }

    /// <summary>
    /// Result from combat that flows to reward/progression.
    /// </summary>
    public class CombatResult
    {
        public string LevelId { get; set; }
        public bool Victory { get; set; }
        public int Stars { get; set; } // 0-3 stars
        public int Score { get; set; }
        public List<string> RewardItemIds { get; set; } = new List<string>();
    }
}

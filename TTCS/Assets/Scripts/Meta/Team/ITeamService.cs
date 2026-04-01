using System.Collections.Generic;
using TTCS.Meta.Common;

namespace TTCS.Meta.Team
{
    /// <summary>
    /// Manages player team/lineup management.
    /// Locked interface from Sprint 3 Phase 1 Kickoff.
    /// </summary>
    public interface ITeamService
    {
        /// <summary>
        /// Gets the current active lineup (read-only list of character IDs).
        /// </summary>
        IReadOnlyList<string> GetCurrentLineup();

        /// <summary>
        /// Validates if a lineup is valid per game rules.
        /// </summary>
        ValidationResult ValidateLineup(IReadOnlyList<string> lineup);

        /// <summary>
        /// Saves a new lineup as the current active lineup.
        /// Should validate before saving.
        /// </summary>
        void SaveLineup(IReadOnlyList<string> lineup);
    }
}

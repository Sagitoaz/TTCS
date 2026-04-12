using System.Collections.Generic;

namespace TTCS.Flow
{
    /// <summary>
    /// Lightweight runtime context to pass data between flow scenes before DevA integration.
    /// </summary>
    public static class FlowRuntimeContext
    {
        public static string SelectedChapterId { get; set; } = "chapter_01";
        public static string SelectedLevelId { get; set; }
        public static IReadOnlyList<string> SelectedLineupSnapshot { get; set; }
        public static CombatResult LastCombatResult { get; set; }

        public static bool HasCombatLaunchData =>
            !string.IsNullOrWhiteSpace(SelectedLevelId) && SelectedLineupSnapshot != null;

        public static void ClearCombatLaunchData()
        {
            SelectedLevelId = null;
            SelectedLineupSnapshot = null;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using TTCS.Core.Save;
using TTCS.Debugging;
using TTCS.Meta.Common;

namespace TTCS.Meta.Team
{
    public sealed class TeamService : ITeamService
    {
        private const int MaxLineupSize = 3;
        private readonly SaveManager _saveManager;

        public TeamService(SaveManager saveManager)
        {
            _saveManager = saveManager;
        }

        public IReadOnlyList<string> GetCurrentLineup()
        {
            var save = _saveManager.CurrentSave;
            if (save == null)
            {
                return new List<string>();
            }

            return save.lineup.ToList();
        }

        public ValidationResult ValidateLineup(IReadOnlyList<string> lineup)
        {
            if (lineup == null || lineup.Count == 0)
            {
                return ValidationResult.Invalid("Lineup cannot be empty.");
            }

            if (lineup.Count > MaxLineupSize)
            {
                return ValidationResult.Invalid($"Lineup cannot exceed {MaxLineupSize} units.");
            }

            if (lineup.Any(string.IsNullOrWhiteSpace))
            {
                return ValidationResult.Invalid("Lineup contains invalid character id.");
            }

            if (lineup.Distinct().Count() != lineup.Count)
            {
                return ValidationResult.Invalid("Lineup cannot contain duplicate characters.");
            }

            var save = _saveManager.CurrentSave;
            if (save != null)
            {
                foreach (var characterId in lineup)
                {
                    if (!save.unlockedCharacters.Contains(characterId))
                    {
                        return ValidationResult.Invalid($"Character '{characterId}' is not unlocked.");
                    }
                }
            }

            return ValidationResult.Valid();
        }

        public void SaveLineup(IReadOnlyList<string> lineup)
        {
            var save = _saveManager.CurrentSave;
            if (save == null)
            {
                return;
            }

            var validation = ValidateLineup(lineup);
            if (!validation.IsValid)
            {
                DebugLogger.LogWarning($"[TeamService] SaveLineup rejected: {validation.Message}", DebugLogger.LogCategory.Save);
                return;
            }

            save.lineup.Clear();
            save.lineup.AddRange(lineup);

            // keep backward compatibility for old systems
            save.currentParty.Clear();
            save.currentParty.AddRange(lineup);

            var slotIndex = _saveManager.ActiveSlotIndex >= 0 ? _saveManager.ActiveSlotIndex : 0;
            _saveManager.Save(slotIndex);

            DebugLogger.Log($"[TeamService] Saved lineup: {string.Join(",", lineup)}", DebugLogger.LogCategory.Save);
        }
    }
}

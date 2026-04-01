using System.Collections.Generic;
using TTCS.Meta.Common;

namespace TTCS.Meta.Team
{
    public interface ITeamService
    {
        IReadOnlyList<string> GetCurrentLineup();
        ValidationResult ValidateLineup(IReadOnlyList<string> lineup);
        void SaveLineup(IReadOnlyList<string> lineup);
    }
}

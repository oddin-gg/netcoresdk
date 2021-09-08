using System.Collections.Generic;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions
{
    public interface ISport : ISportSummary
    {
        IEnumerable<ITournament> Tournaments { get; }

        string IconPath { get; }
    }
}

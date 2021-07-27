using System;
using System.Collections.Generic;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions
{
    public interface ITournament : ISportEvent
    {
        IEnumerable<ICompetitor> GetCompetitors();

        DateTime? GetEndDate();

        DateTime? GetStartDate();
    }
}

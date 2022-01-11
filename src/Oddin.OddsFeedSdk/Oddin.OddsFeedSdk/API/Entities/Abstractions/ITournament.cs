using System;
using System.Collections.Generic;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions
{
    public interface ITournament : ISportEvent
    {
        string IconPath { get; }

        IEnumerable<ICompetitor> GetCompetitors();

        DateTime? GetEndDate();

        DateTime? GetStartDate();

        int? RiskTier();
    }
}
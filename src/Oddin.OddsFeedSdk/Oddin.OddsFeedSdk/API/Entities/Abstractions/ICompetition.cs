using System.Collections.Generic;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions;

public interface ICompetition : ISportEvent
{
    ICompetitionStatus Status { get; }

    IEnumerable<ICompetitor> Competitors { get; }
}
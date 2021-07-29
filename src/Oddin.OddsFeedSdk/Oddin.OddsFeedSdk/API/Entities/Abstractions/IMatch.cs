using System.Collections.Generic;
using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions
{
    public interface IMatch : ISportEvent
    {
        URN SportId { get; }

        LiveOddsAvailability? LiveOddsAvailability { get; }

        IEnumerable<ITeamCompetitor> Competitors { get; }

        ITeamCompetitor HomeCompetitor { get; }

        ITeamCompetitor AwayCompetitor { get; }

        ITournament Tournament { get; }

        IFixture Fixture { get; }
    }
}

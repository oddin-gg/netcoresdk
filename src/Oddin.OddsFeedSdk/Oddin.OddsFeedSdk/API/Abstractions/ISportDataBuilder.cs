using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdk.API.Abstractions
{
    internal interface ISportDataBuilder
    {
        Task<IEnumerable<ISport>> BuildSports(IEnumerable<CultureInfo> locales);

        IEnumerable<ITournament> BuildTournaments(IEnumerable<URN> ids, URN sportId, IEnumerable<CultureInfo> locales);

        ITournament BuildTournament(URN id, URN sportId, IEnumerable<CultureInfo> locales);

        ISport BuildSport(URN id, IEnumerable<CultureInfo> locales);

        IEnumerable<ICompetitor> BuildCompetitors(IEnumerable<URN> ids, IEnumerable<CultureInfo> cultures);

        IEnumerable<IMatch> BuildMatches(IEnumerable<URN> ids, IEnumerable<CultureInfo> cultures);

        IMatch BuildMatch(URN id, IEnumerable<CultureInfo> cultures, URN sportId = null);

        ICompetitor BuildCompetitor(URN id, IEnumerable<CultureInfo> cultures);

        IFixture BuildFixture(URN id, IEnumerable<CultureInfo> cultures);

        IMatchStatus BuildMatchStatus(URN id, IEnumerable<CultureInfo> cultures);
    }
}

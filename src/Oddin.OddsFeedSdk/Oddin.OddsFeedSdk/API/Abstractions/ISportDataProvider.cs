using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdk.API.Abstractions
{
    public interface ISportDataProvider
    {
        Task<IEnumerable<ISport>> GetSportsAsync(CultureInfo culture = null);
        Task<ISport> GetSportAsync(URN id, CultureInfo culture = null);

        IEnumerable<ITournament> GetActiveTournaments(CultureInfo culture);
        IEnumerable<ITournament> GetActiveTournaments(string name, CultureInfo culture);
        IEnumerable<ITournament> GetAvailableTournaments(URN sportId, CultureInfo culture);

        IEnumerable<IMatch> GetMatchesFor(DateTime dateTime, CultureInfo culture = null);
        IEnumerable<IMatch> GetLiveMatches(CultureInfo culture = null);
        IMatch GetMatch(URN id, CultureInfo culture = null);

        ICompetitor GetCompetitor(URN id, CultureInfo culture = null);

        IEnumerable<IFixtureChange> GetFixtureChanges(CultureInfo culture = null); 

        IEnumerable<IMatch> GetListOfMatches(int stardIndex, int limit, CultureInfo culture = null);

        void DeleteTournamentFromCache(URN id);
        void DeleteCompetitorFromCache(URN id);
        void DeleteMatchFromCache(URN id);
    }
}

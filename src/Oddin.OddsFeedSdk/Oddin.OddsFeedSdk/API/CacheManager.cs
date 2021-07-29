using Oddin.OddsFeedSdk.API.Abstractions;

namespace Oddin.OddsFeedSdk.API
{
    internal class CacheManager : ICacheManager
    {
        public ISportDataCache SportDataCache { get; }

        public ITournamentsCache TournamentsCache { get; }

        public ICompetitorCache CompetitorCache { get; }

        public IMatchCache MatchCache { get; }

        public CacheManager(ISportDataCache sportDataCache, ITournamentsCache tournamentsCache, ICompetitorCache competitorCache, IMatchCache matchCache)
        {
            SportDataCache = sportDataCache;
            TournamentsCache = tournamentsCache;
            CompetitorCache = competitorCache;
            MatchCache = matchCache;
        }
    }
}

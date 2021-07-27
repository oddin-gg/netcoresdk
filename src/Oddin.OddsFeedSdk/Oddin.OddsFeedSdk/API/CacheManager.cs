using Oddin.OddsFeedSdk.API.Abstractions;

namespace Oddin.OddsFeedSdk.API
{
    internal class CacheManager : ICacheManager
    {
        public ISportDataCache SportDataCache { get; }

        public ITournamentsCache TournamentsCache { get; }

        public ICompetitorCache CompetitorCache { get; }

        public CacheManager(ISportDataCache sportDataCache, ITournamentsCache tournamentsCache, ICompetitorCache competitorCache)
        {
            SportDataCache = sportDataCache;
            TournamentsCache = tournamentsCache;
            CompetitorCache = competitorCache;
        }
    }
}

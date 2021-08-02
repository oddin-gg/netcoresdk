using System;
using Oddin.OddsFeedSdk.API.Abstractions;

namespace Oddin.OddsFeedSdk.API
{
    internal class CacheManager : ICacheManager, IDisposable
    {
        public ILocalizedStaticDataCache LocalizedStaticDataCache { get; }

        public IFixtureCache FixtureCache { get; }
        public IMarketDescriptionCache MarketDescriptionCache { get; }
        public ISportDataCache SportDataCache { get; }

        public ITournamentsCache TournamentsCache { get; }

        public ICompetitorCache CompetitorCache { get; }

        public IMatchCache MatchCache { get; }

        public IMatchStatusCache MatchStatusCache { get; }

        public CacheManager(
            ISportDataCache sportDataCache,
            ITournamentsCache tournamentsCache,
            ICompetitorCache competitorCache,
            IMatchCache matchCache,
            ILocalizedStaticDataCache localizedStaticDataCache,
            IFixtureCache fixtureCache,
            IMatchStatusCache matchStatusCache,
            IMarketDescriptionCache marketDescriptionCache)
        {
            SportDataCache = sportDataCache;
            TournamentsCache = tournamentsCache;
            CompetitorCache = competitorCache;
            MatchCache = matchCache;
            LocalizedStaticDataCache = localizedStaticDataCache;
            FixtureCache = fixtureCache;
            MatchStatusCache = matchStatusCache;
            MarketDescriptionCache = marketDescriptionCache;
        }

        public void Dispose()
        {
            CompetitorCache.Dispose();
            MatchCache.Dispose();
            TournamentsCache.Dispose();
            SportDataCache.Dispose();
            MatchStatusCache.Dispose();
        }
    }
}

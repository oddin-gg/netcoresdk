using System;
using System.Collections.Generic;
using Oddin.OddsFeedSdk.AMQP.EventArguments;
using Oddin.OddsFeedSdk.AMQP.Messages;
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

        public ISet<string> DispatchedFixtureChanges { get; }

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
            DispatchedFixtureChanges = new HashSet<string>();
        }

        public void Dispose()
        {
            CompetitorCache.Dispose();
            MatchCache.Dispose();
            TournamentsCache.Dispose();
            SportDataCache.Dispose();
            MatchStatusCache.Dispose();
        }

        public void OnFeedMessageReceived(FeedMessageModel message)
        {
            switch (message)
            {
                case odds_change oddsChangeMessage:
                    MatchStatusCache.OnFeedMessageReceived(oddsChangeMessage);
                    break;
                case fixture_change fixtureChange:
                    MatchCache.OnFeedMessageReceived(fixtureChange);
                    TournamentsCache.OnFeedMessageReceived(fixtureChange);
                    FixtureCache.OnFeedMessageReceived(fixtureChange);
                    break;
           }

        }
    }
}
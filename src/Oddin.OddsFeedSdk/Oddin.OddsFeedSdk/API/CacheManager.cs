using System;
using System.Collections.Generic;
using Oddin.OddsFeedSdk.AMQP.Messages;
using Oddin.OddsFeedSdk.API.Abstractions;

namespace Oddin.OddsFeedSdk.API;

internal class CacheManager : ICacheManager, IDisposable
{
    public CacheManager(
        ISportDataCache sportDataCache,
        ITournamentsCache tournamentsCache,
        ICompetitorCache competitorCache,
        IPlayerCache playerCache,
        IMatchCache matchCache,
        ILocalizedStaticDataCache localizedStaticDataCache,
        IFixtureCache fixtureCache,
        IMatchStatusCache matchStatusCache,
        IMarketDescriptionCache marketDescriptionCache)
    {
        SportDataCache = sportDataCache;
        TournamentsCache = tournamentsCache;
        CompetitorCache = competitorCache;
        PlayerCache = playerCache;
        MatchCache = matchCache;
        LocalizedStaticDataCache = localizedStaticDataCache;
        FixtureCache = fixtureCache;
        MatchStatusCache = matchStatusCache;
        MarketDescriptionCache = marketDescriptionCache;
        DispatchedFixtureChanges = new HashSet<string>();
    }

    public IMatchStatusCache MatchStatusCache { get; }

    public ISet<string> DispatchedFixtureChanges { get; }
    public ILocalizedStaticDataCache LocalizedStaticDataCache { get; }

    public IFixtureCache FixtureCache { get; }

    public IMarketDescriptionCache MarketDescriptionCache { get; }

    public ISportDataCache SportDataCache { get; }

    public ITournamentsCache TournamentsCache { get; }

    public ICompetitorCache CompetitorCache { get; }

    public IPlayerCache PlayerCache { get; }

    public IMatchCache MatchCache { get; }

    public void Dispose()
    {
        CompetitorCache.Dispose();
        PlayerCache.Dispose();
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
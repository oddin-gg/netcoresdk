namespace Oddin.OddsFeedSdk.API.Abstractions;

internal interface ICacheManager
{
    ISportDataCache SportDataCache { get; }

    ITournamentsCache TournamentsCache { get; }

    ICompetitorCache CompetitorCache { get; }

    IPlayerCache PlayerCache { get; }

    IMatchCache MatchCache { get; }

    ILocalizedStaticDataCache LocalizedStaticDataCache { get; }

    IFixtureCache FixtureCache { get; }

    IMarketDescriptionCache MarketDescriptionCache { get; }
}
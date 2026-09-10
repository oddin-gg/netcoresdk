using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using Oddin.OddsFeedSdk.API;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.API.Models;
using Oddin.OddsFeedSdk.Common;
using Xunit;

namespace Oddin.OddsFeedSdk.Tests.API;

// Rx disposes the subscription permanently when an observer throws, and Subject
// walks observers in order without catching, so later caches miss the response too.
[Collection(CacheSideLoadCollection.Name)]
public class CacheObserverResilienceTests
{
    private static readonly CultureInfo Culture = new("en");

    private static URN PlayerId => new("od:player:1");
    private static URN CompetitorId => new("od:competitor:1");
    private static URN MatchId => new("od:match:1");
    private static URN SportId => new("od:sport:1");
    private static URN TournamentId => new("od:tournament:1");

    // A payload that throws inside each cache's observer, then a good one. The
    // subscription has to survive the first for the second to land.
    [Fact]
    public void PlayerCacheKeepsSideLoadingAfterAResponseThatThrows()
    {
        var (api, proxy) = NewApi();
        using var cache = new PlayerCache(api);

        var escaped = Publish(proxy, new competitorProfileEndpoint { players = null });
        Publish(proxy, Profile(new player_profilePlayer { id = "od:player:1", name = "Dendi" }));

        Assert.False(escaped, EscapeMessage);
        Assert.NotNull(cache.GetPlayer(PlayerId, new[] { Culture }));
    }

    [Fact]
    public void CompetitorCacheKeepsSideLoadingAfterAResponseThatThrows()
    {
        var (api, proxy) = NewApi();
        using var cache = new CompetitorCache(api);

        var escaped = Publish(proxy, new MatchSummaryModel { sport_event = new sportEvent { id = "od:match:1" } });
        Publish(proxy, new TournamentInfoModel
        {
            tournament = new tournamentExtended { id = "od:tournament:1", competitors = Array.Empty<team>() },
            competitors = new[] { new team { id = "od:competitor:1", name = "Team Secret" } }
        });

        Assert.False(escaped, EscapeMessage);
        Assert.NotNull(cache.GetCompetitor(CompetitorId, new[] { Culture }));
    }

    [Fact]
    public void MatchStatusCacheKeepsSideLoadingAfterAResponseThatThrows()
    {
        var (api, proxy) = NewApi();
        using var cache = new MatchStatusCache(api);

        // sport_event_status is null, and RefreshOrInsertApiItem dereferences status.
        var escaped = Publish(proxy, new MatchSummaryModel { sport_event = new sportEvent { id = "od:match:1" } });
        Publish(proxy, new MatchSummaryModel
        {
            sport_event = new sportEvent { id = "od:match:1" },
            sport_event_status = new sportEventStatus { status = "live", match_status_code = 1 }
        });

        Assert.False(escaped, EscapeMessage);
        Assert.NotNull(cache.GetMatchStatus(MatchId));
    }

    [Fact]
    public void SportDataCacheKeepsSideLoadingAfterAResponseThatThrows()
    {
        var (api, proxy) = NewApi();
        using var cache = new SportDataCache(api);

        // tournament is null, so the switch arm dereferences it building the dictionary.
        var escaped = Publish(proxy, new TournamentInfoModel { tournament = null });
        Publish(proxy, TournamentInfo());

        Assert.False(escaped, EscapeMessage);
        Assert.NotNull(ReadSport(cache));
    }

    [Fact]
    public void TournamentsCacheKeepsSideLoadingAfterAResponseThatThrows()
    {
        var (api, proxy) = NewApi();
        using var cache = new TournamentsCache(api);

        var escaped = Publish(proxy, new FixturesEndpointModel { fixture = null });
        Publish(proxy, new TournamentsModel
        {
            tournaments = new List<tournament> { new() { id = "od:tournament:1", name = "The International" } }
        });

        Assert.False(escaped, EscapeMessage);
        Assert.NotNull(cache.GetTournament(TournamentId, new[] { Culture }));
    }

    [Fact]
    public void MatchCacheKeepsSideLoadingAfterAResponseThatThrows()
    {
        var (api, proxy) = NewApi();
        using var cache = new MatchCache(api);

        var escaped = Publish(proxy, new ScheduleEndpointModel { sport_event = null });
        Publish(proxy, new FixturesEndpointModel
        {
            fixture = new fixture { id = "od:match:1", name = "Secret vs Liquid" }
        });

        Assert.False(escaped, EscapeMessage);
        Assert.NotNull(cache.PeekMatch(MatchId));
    }

    // One malformed item must not take its siblings with it: the response-level
    // guard would otherwise abort the whole batch.
    [Fact]
    public void PlayerCacheCachesTheGoodItemsInABatchWithABadId()
    {
        var (api, proxy) = NewApi();
        using var cache = new PlayerCache(api);

        Publish(proxy, Profile(
            new player_profilePlayer { id = "bogus", name = "Malformed" },
            new player_profilePlayer { id = "od:player:1", name = "Dendi" }));

        Assert.NotNull(cache.GetPlayer(PlayerId, new[] { Culture }));
    }

    [Fact]
    public void CompetitorCacheCachesTheGoodItemsInABatchWithABadId()
    {
        var (api, proxy) = NewApi();
        using var cache = new CompetitorCache(api);

        Publish(proxy, new TournamentInfoModel
        {
            tournament = new tournamentExtended { id = "od:tournament:1", competitors = Array.Empty<team>() },
            competitors = new[]
            {
                new team { id = "bogus", name = "Malformed" },
                new team { id = "od:competitor:1", name = "Team Secret" }
            }
        });

        Assert.NotNull(cache.GetCompetitor(CompetitorId, new[] { Culture }));
    }

    [Fact]
    public void SportDataCacheCachesTheGoodItemsInABatchWithABadId()
    {
        var (api, proxy) = NewApi();
        using var cache = new SportDataCache(api);

        Publish(proxy, new TournamentScheduleModel
        {
            tournament = new[]
            {
                new tournamentExtended { id = "bogus", sport = new sport { id = "bogus", name = "Malformed" } },
                new tournamentExtended { id = "od:tournament:1", sport = Sport() }
            }
        });

        Assert.NotNull(ReadSport(cache));
    }

    [Fact]
    public void TournamentsCacheCachesTheGoodItemsInABatchWithABadId()
    {
        var (api, proxy) = NewApi();
        using var cache = new TournamentsCache(api);

        Publish(proxy, new TournamentsModel
        {
            tournaments = new List<tournament>
            {
                new() { id = "bogus", name = "Malformed" },
                new() { id = "od:tournament:1", name = "The International" }
            }
        });

        Assert.NotNull(cache.GetTournament(TournamentId, new[] { Culture }));
    }

    [Fact]
    public void MatchCacheCachesTheGoodItemsInABatchWithABadId()
    {
        var (api, proxy) = NewApi();
        using var cache = new MatchCache(api);

        Publish(proxy, new ScheduleEndpointModel
        {
            sport_event = new[]
            {
                new sportEvent { id = "bogus", name = "Malformed" },
                new sportEvent { id = "od:match:1", name = "Secret vs Liquid" }
            }
        });

        Assert.NotNull(cache.PeekMatch(MatchId));
    }

    private const string EscapeMessage =
        "a bad side-load response escaped into the API caller, which sees it as a failed request";

    private static LocalizedSport ReadSport(SportDataCache cache) =>
        cache.GetSport(SportId, new[] { Culture }).ConfigureAwait(false).GetAwaiter().GetResult();

    private static (IApiClient Api, PublishOnlyApiClientProxy Proxy) NewApi()
    {
        var api = DispatchProxy.Create<IApiClient, PublishOnlyApiClientProxy>();
        return (api, (PublishOnlyApiClientProxy)api);
    }

    private static bool Publish(PublishOnlyApiClientProxy proxy, object data)
    {
        try
        {
            proxy.Publish(data, Culture);
            return false;
        }
        catch
        {
            return true;
        }
    }

    private static sport Sport() => new() { id = "od:sport:1", name = "Dota 2" };

    private static TournamentInfoModel TournamentInfo() =>
        new()
        {
            tournament = new tournamentExtended
            {
                id = "od:tournament:1",
                name = "The International",
                sport = Sport(),
                competitors = Array.Empty<team>()
            }
        };

    private static competitorProfileEndpoint Profile(params player_profilePlayer[] players) =>
        new()
        {
            competitor = new teamExtended { id = "od:competitor:1", name = "Team Secret" },
            players = new List<player_profilePlayer>(players)
        };

    // Every direct API call fails, so a cache hit can only be a side-load.
    public class PublishOnlyApiClientProxy : DispatchProxy
    {
        private readonly Subject<IRequestResult<object>> _responses = new();

        public void Publish(object data, CultureInfo culture) =>
            _responses.OnNext(RequestResult<object>.Success(
                data,
                HttpStatusCode.OK,
                string.Empty,
                culture: culture));

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            if (targetMethod.Name == nameof(IApiClient.SubscribeForClass))
                return _responses.OfType<IRequestResult<object>>();

            throw new InvalidOperationException($"API unavailable: {targetMethod.Name}");
        }
    }
}

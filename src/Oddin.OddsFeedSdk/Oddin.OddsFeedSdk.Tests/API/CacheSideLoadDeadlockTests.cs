using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Oddin.OddsFeedSdk.API;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Models;
using Oddin.OddsFeedSdk.Common;
using Xunit;

namespace Oddin.OddsFeedSdk.Tests.API;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class CacheSideLoadCollection
{
    public const string Name = "Cache side-load";
}

// A response is published on the calling thread, so a sibling cache's observer
// takes a second semaphore there. Sport and tournament do it in opposite orders.
[Collection(CacheSideLoadCollection.Name)]
public class CacheSideLoadDeadlockTests
{
    private static readonly TimeSpan JoinTimeout = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan BarrierTimeout = TimeSpan.FromSeconds(5);
    private static readonly CultureInfo Culture = new("en");

    private static URN SportId => new("od:sport:1");
    private static URN TournamentId => new("od:tournament:1");
    private static URN MatchId => new("od:match:1");
    private static URN CompetitorId => new("od:competitor:1");
    private static URN PlayerId => new("od:player:1");

    [Fact]
    public void ConcurrentColdSportAndTournamentLoadsDoNotDeadlock()
    {
        var api = DispatchProxy.Create<IApiClient, SideLoadApiClientProxy>();
        var proxy = (SideLoadApiClientProxy)api;
        proxy.ArmOverlapBarrier(BarrierTimeout);

        using var sportCache = new SportDataCache(api);
        using var tournamentsCache = new TournamentsCache(api);

        var sportLoad = Worker.Start(
            "sport-load",
            () => sportCache.GetSportTournaments(SportId, Culture));
        var tournamentLoad = Worker.Start(
            "tournament-load",
            () => tournamentsCache.GetTournament(TournamentId, new[] { Culture }));

        var sportFinished = sportLoad.Join(JoinTimeout);
        var tournamentFinished = tournamentLoad.Join(JoinTimeout);

        // Overlap first: without it a green run proves nothing.
        Assert.True(
            proxy.OverlapAchieved,
            "the two loads never sat inside their API calls at the same time, so this run proved nothing");
        Assert.True(sportFinished, "sport load never finished — the two caches deadlocked");
        Assert.True(tournamentFinished, "tournament load never finished — the two caches deadlocked");

        sportLoad.Rethrow();
        tournamentLoad.Rethrow();
    }

    [Theory]
    [InlineData(CacheUnderTest.SportData)]
    [InlineData(CacheUnderTest.Tournaments)]
    [InlineData(CacheUnderTest.Competitor)]
    [InlineData(CacheUnderTest.Player)]
    [InlineData(CacheUnderTest.Match)]
    [InlineData(CacheUnderTest.MatchStatus)]
    public void CacheDoesNotHoldItsSemaphoreWhileTheApiCallIsInFlight(CacheUnderTest which)
    {
        var api = DispatchProxy.Create<IApiClient, SideLoadApiClientProxy>();
        var proxy = (SideLoadApiClientProxy)api;

        var (cache, exercise) = Build(which, api);

        using (cache as IDisposable)
        {
            bool? freeDuringCall = null;
            proxy.WhileServing = () => freeDuringCall ??= SemaphoreIsFree(cache);

            exercise();

            Assert.True(freeDuringCall.HasValue, $"{which} never made an API call, so this run proved nothing");
            Assert.True(
                freeDuringCall.Value,
                $"{which} held its semaphore across the API call — a sibling cache's observer "
                + "runs on this thread and can be left waiting for it");
        }
    }

    private static (object Cache, Action Exercise) Build(CacheUnderTest which, IApiClient api)
    {
        switch (which)
        {
            case CacheUnderTest.SportData:
                var sportData = new SportDataCache(api);
                return (sportData, () => sportData.GetSportTournaments(SportId, Culture));
            case CacheUnderTest.Tournaments:
                var tournaments = new TournamentsCache(api);
                return (tournaments, () => tournaments.GetTournament(TournamentId, new[] { Culture }));
            case CacheUnderTest.Competitor:
                var competitor = new CompetitorCache(api);
                return (competitor, () => competitor.GetCompetitor(CompetitorId, new[] { Culture }));
            case CacheUnderTest.Player:
                var player = new PlayerCache(api);
                return (player, () => player.GetPlayer(PlayerId, new[] { Culture }));
            case CacheUnderTest.Match:
                var match = new MatchCache(api);
                return (match, () => match.GetMatch(MatchId, new[] { Culture }));
            case CacheUnderTest.MatchStatus:
                var matchStatus = new MatchStatusCache(api);
                return (matchStatus, () => matchStatus.GetMatchStatus(MatchId));
            default:
                throw new ArgumentOutOfRangeException(nameof(which));
        }
    }

    // GetSports/GetSport share an async loader that was changed separately; the
    // theory above only reaches the synchronous GetSportTournaments.
    [Fact]
    public async Task SportCacheDoesNotHoldItsSemaphoreDuringTheAsyncSportsCall()
    {
        var api = DispatchProxy.Create<IApiClient, SideLoadApiClientProxy>();
        var proxy = (SideLoadApiClientProxy)api;

        using var cache = new SportDataCache(api);

        bool? freeDuringCall = null;
        proxy.WhileServing = () => freeDuringCall ??= SemaphoreIsFree(cache);

        var sports = await cache.GetSports(new[] { Culture });

        Assert.True(freeDuringCall.HasValue, "GetSports never made an API call");
        Assert.True(
            freeDuringCall.Value,
            "SportDataCache held its semaphore across the async GetSports call");
        Assert.Contains(SportId, sports);
    }

    // Semaphore(1,1) has no owning thread, so this reports the permit state from anywhere.
    private static bool SemaphoreIsFree(object cache)
    {
        var field = cache.GetType().GetField("_semaphore", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);

        var semaphore = (Semaphore)field.GetValue(cache);
        if (semaphore.WaitOne(TimeSpan.Zero) == false)
            return false;

        semaphore.Release();
        return true;
    }

    public enum CacheUnderTest
    {
        SportData,
        Tournaments,
        Competitor,
        Player,
        Match,
        MatchStatus
    }

    // Not the pool: on a failing run both bodies block forever.
    private sealed class Worker
    {
        private readonly Thread _thread;
        private Exception _error;

        private Worker(string name, Action body)
        {
            _thread = new Thread(() =>
            {
                try
                {
                    body();
                }
                catch (Exception e)
                {
                    _error = e;
                }
            })
            {
                IsBackground = true,
                Name = name
            };
        }

        internal static Worker Start(string name, Action body)
        {
            var worker = new Worker(name, body);
            worker._thread.Start();
            return worker;
        }

        internal bool Join(TimeSpan timeout) => _thread.Join(timeout);

        // After the deadlock assertions, or it hides which failure happened.
        internal void Rethrow()
        {
            if (_error is not null)
                throw new Xunit.Sdk.XunitException($"{_thread.Name} threw: {_error}");
        }
    }

    // Mirrors RestClient: publishes synchronously on the calling thread.
    public class SideLoadApiClientProxy : DispatchProxy
    {
        private readonly Subject<IRequestResult<object>> _responses = new();
        private Barrier _overlap;
        private TimeSpan _overlapTimeout;
        private int _overlapReached;

        public Action WhileServing { get; set; }

        public bool OverlapAchieved => Volatile.Read(ref _overlapReached) >= 2;

        public void ArmOverlapBarrier(TimeSpan timeout)
        {
            _overlap = new Barrier(2);
            _overlapTimeout = timeout;
        }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            switch (targetMethod.Name)
            {
                case nameof(IApiClient.SubscribeForClass):
                    return _responses.OfType<IRequestResult<object>>();
                case nameof(IApiClient.GetSports):
                    return Task.FromResult(Serve(SportsModel(), (CultureInfo)args[0]));
                case nameof(IApiClient.GetTournaments):
                    return Serve(Tournaments(), (CultureInfo)args[1]);
                case nameof(IApiClient.GetTournament):
                    return Serve(TournamentInfo(), (CultureInfo)args[1]);
                case nameof(IApiClient.GetCompetitorProfileWithPlayers):
                    return Serve(CompetitorProfile(), (CultureInfo)args[1]);
                case nameof(IApiClient.GetPlayerProfile):
                    return Serve(Player(), (CultureInfo)args[1]);
                case nameof(IApiClient.GetMatchSummary):
                    return Serve(MatchSummary(), (CultureInfo)args[1]);
                default:
                    throw new NotSupportedException($"Unexpected API call: {targetMethod.Name}");
            }
        }

        private T Serve<T>(T data, CultureInfo culture)
            where T : class
        {
            WhileServing?.Invoke();

            // Timed, so "never overlapped" fails as itself rather than hanging.
            if (_overlap is not null && _overlap.SignalAndWait(_overlapTimeout))
                Interlocked.Increment(ref _overlapReached);

            _responses.OnNext(RequestResult<object>.Success(
                data,
                HttpStatusCode.OK,
                string.Empty,
                culture: culture));

            return data;
        }

        // Only the fields the observers and loaders read.
        private static sport Sport() => new() { id = "od:sport:1", name = "Dota 2" };

        private static SportsModel SportsModel() =>
            new() { sport = new[] { new sportExtended { id = "od:sport:1", name = "Dota 2" } } };

        private static TournamentsModel Tournaments() =>
            new()
            {
                sport = Sport(),
                tournaments = new List<tournament>
                {
                    new() { id = "od:tournament:1", name = "The International", sport = Sport() }
                }
            };

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

        private static competitorProfileEndpoint CompetitorProfile() =>
            new()
            {
                competitor = new teamExtended { id = "od:competitor:1", name = "Team Secret" },
                players = new List<player_profilePlayer>()
            };

        private static player_profilePlayer Player() =>
            new() { id = "od:player:1", name = "Dendi", full_name = "Danil Ishutin" };

        private static MatchSummaryModel MatchSummary() =>
            new()
            {
                sport_event = new sportEvent
                {
                    id = "od:match:1",
                    name = "Secret vs Liquid",
                    tournament = new tournament { id = "od:tournament:1", sport = Sport() }
                }
            };
    }
}

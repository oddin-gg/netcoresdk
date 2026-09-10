using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading;
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

// An API response is published on the thread that made the call, so a sibling
// cache's side-load observer runs on that thread and asks for a second
// semaphore. If the caller is still holding its own, two caches can end up
// waiting on each other: SportDataCache.GetSportTournaments takes the sport
// semaphore and publishes a TournamentsModel that TournamentsCache observes,
// while TournamentsCache.GetTournament takes the tournament semaphore and
// publishes a TournamentInfoModel that SportDataCache observes.
[Collection(CacheSideLoadCollection.Name)]
public class CacheSideLoadDeadlockTests
{
    private static readonly TimeSpan JoinTimeout = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan BarrierTimeout = TimeSpan.FromSeconds(5);
    private static readonly CultureInfo Culture = new("en");

    private static URN SportId => new("od:sport:1");
    private static URN TournamentId => new("od:tournament:1");

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

        // Assert the overlap first: without it a green run proves nothing, because
        // the two loads may simply have run one after the other.
        Assert.True(
            proxy.OverlapAchieved,
            "the two loads never sat inside their API calls at the same time, so this run proved nothing");
        Assert.True(sportFinished, "sport load never finished — the two caches deadlocked");
        Assert.True(tournamentFinished, "tournament load never finished — the two caches deadlocked");

        sportLoad.Rethrow();
        tournamentLoad.Rethrow();
    }

    [Fact]
    public void SportCacheDoesNotHoldItsSemaphoreWhileTheApiCallIsInFlight()
    {
        var api = DispatchProxy.Create<IApiClient, SideLoadApiClientProxy>();
        var proxy = (SideLoadApiClientProxy)api;

        using var sportCache = new SportDataCache(api);

        bool? freeDuringCall = null;
        proxy.WhileServing = () => freeDuringCall = SemaphoreIsFree(sportCache);

        sportCache.GetSportTournaments(SportId, Culture);

        Assert.True(freeDuringCall.HasValue, "the API call was never served");
        Assert.True(
            freeDuringCall.Value,
            "SportDataCache held its semaphore across the API call — a sibling cache's observer "
            + "runs on this thread and can be left waiting for it");
    }

    [Fact]
    public void TournamentsCacheDoesNotHoldItsSemaphoreWhileTheApiCallIsInFlight()
    {
        var api = DispatchProxy.Create<IApiClient, SideLoadApiClientProxy>();
        var proxy = (SideLoadApiClientProxy)api;

        using var tournamentsCache = new TournamentsCache(api);

        bool? freeDuringCall = null;
        proxy.WhileServing = () => freeDuringCall = SemaphoreIsFree(tournamentsCache);

        tournamentsCache.GetTournament(TournamentId, new[] { Culture });

        Assert.True(freeDuringCall.HasValue, "the API call was never served");
        Assert.True(
            freeDuringCall.Value,
            "TournamentsCache held its semaphore across the API call — a sibling cache's observer "
            + "runs on this thread and can be left waiting for it");
    }

    // Semaphore(1,1) has no owning thread, so a zero-timeout Wait reports whether
    // the permit is taken regardless of which thread asks.
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

    // Dedicated background threads rather than the pool: on a failing run both
    // bodies block forever, and permanently consuming pool threads would starve
    // every later test in the run. Background threads still let the host exit.
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

        // Only after the deadlock assertions: a body that threw still finished,
        // and reporting the throw first would hide which failure actually happened.
        internal void Rethrow()
        {
            if (_error is not null)
                throw new Xunit.Sdk.XunitException($"{_thread.Name} threw: {_error}");
        }
    }

    // Mirrors RestClient: publishes the response synchronously, on the calling
    // thread, before returning it.
    public class SideLoadApiClientProxy : DispatchProxy
    {
        private readonly Subject<IRequestResult<object>> _responses = new();
        private Barrier _overlap;
        private TimeSpan _overlapTimeout;
        private int _overlapReached;

        public Action WhileServing { get; set; }

        public bool OverlapAchieved => Volatile.Read(ref _overlapReached) == 2;

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
                case nameof(IApiClient.GetTournaments):
                    return Serve(Tournaments(), (CultureInfo)args[1]);
                case nameof(IApiClient.GetTournament):
                    return Serve(TournamentInfo(), (CultureInfo)args[1]);
                default:
                    throw new NotSupportedException($"Unexpected API call: {targetMethod.Name}");
            }
        }

        private T Serve<T>(T data, CultureInfo culture)
            where T : class
        {
            WhileServing?.Invoke();

            // Timed, so "the two loads never overlapped" fails as itself instead of
            // hanging the run and being reported as a deadlock.
            if (_overlap is not null && _overlap.SignalAndWait(_overlapTimeout))
                Interlocked.Increment(ref _overlapReached);

            _responses.OnNext(RequestResult<object>.Success(
                data,
                HttpStatusCode.OK,
                string.Empty,
                culture: culture));

            return data;
        }

        // Only the fields the two observers and their loaders actually read.
        private static TournamentsModel Tournaments() =>
            new()
            {
                sport = new sport { id = "od:sport:1", name = "Dota 2" },
                tournaments = new List<tournament>
                {
                    new()
                    {
                        id = "od:tournament:1",
                        name = "The International",
                        sport = new sport { id = "od:sport:1", name = "Dota 2" }
                    }
                }
            };

        private static TournamentInfoModel TournamentInfo() =>
            new()
            {
                tournament = new tournamentExtended
                {
                    id = "od:tournament:1",
                    name = "The International",
                    sport = new sport { id = "od:sport:1", name = "Dota 2" },
                    competitors = Array.Empty<team>()
                }
            };
    }
}

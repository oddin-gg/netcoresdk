using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading.Tasks;
using Oddin.OddsFeedSdk.API;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.API.Models;
using Oddin.OddsFeedSdk.Common;
using Xunit;

namespace Oddin.OddsFeedSdk.Tests.API;

// The sport entry is NotRemovable with no expiry, so anything put in TournamentIds
// lives for the process and ISport.Tournaments hands it straight to the client.
[Collection(CacheSideLoadCollection.Name)]
public class SportDataCacheTournamentIdsTests
{
    private static readonly CultureInfo Culture = new("en");

    private static URN SportId => new("od:sport:1");

    [Fact]
    public async Task RepeatedSideLoadsKeepOneEntryPerTournament()
    {
        var api = DispatchProxy.Create<IApiClient, SportsApiClientProxy>();
        var proxy = (SportsApiClientProxy)api;
        using var cache = new SportDataCache(api);

        proxy.Publish(TournamentInfo(), Culture);
        proxy.Publish(TournamentInfo(), Culture);

        var sport = await cache.GetSport(SportId, new[] { Culture });

        Assert.NotNull(sport.TournamentIds);
        Assert.Equal(1, sport.TournamentIds.Count);
        Assert.Contains(new URN("od:tournament:1"), sport.TournamentIds);
    }

    [Fact]
    public async Task GetSportTournamentsPutsTheIdsOnTheSport()
    {
        var api = DispatchProxy.Create<IApiClient, SportsApiClientProxy>();
        var proxy = (SportsApiClientProxy)api;
        using var cache = new SportDataCache(api);

        var returned = cache.GetSportTournaments(SportId, Culture).ToList();

        var sport = await cache.GetSport(SportId, new[] { Culture });

        Assert.Equal(2, returned.Count);
        Assert.NotNull(sport.TournamentIds);
        Assert.Equal(
            new[] { new URN("od:tournament:1"), new URN("od:tournament:2") }.OrderBy(u => u.Id),
            sport.TournamentIds.OrderBy(u => u.Id));
    }

    // Entities are handed out by reference and read outside the semaphore, so a
    // set that is already published must never be mutated again.
    [Fact]
    public async Task ASideLoadDoesNotMutateAnAlreadyPublishedTournamentIdSet()
    {
        var api = DispatchProxy.Create<IApiClient, SportsApiClientProxy>();
        var proxy = (SportsApiClientProxy)api;
        using var cache = new SportDataCache(api);

        proxy.Publish(TournamentInfo(), Culture);
        var published = (await cache.GetSport(SportId, new[] { Culture })).TournamentIds;
        Assert.Equal(1, published.Count);

        proxy.Publish(TournamentInfo("od:tournament:2"), Culture);

        Assert.Equal(1, published.Count);
        Assert.Equal(2, (await cache.GetSport(SportId, new[] { Culture })).TournamentIds.Count);
    }

    // A client enumerating ISport.Tournaments must get the same order every call,
    // not server order once and hash order afterwards.
    [Fact]
    public async Task TheSportKeepsServerOrderAcrossCalls()
    {
        var api = DispatchProxy.Create<IApiClient, SportsApiClientProxy>();
        var proxy = (SportsApiClientProxy)api;
        proxy.TournamentIds = new[] { "od:tournament:9", "od:tournament:3", "od:tournament:7" };

        using var cache = new SportDataCache(api);

        var firstCall = cache.GetSportTournaments(SportId, Culture).ToList();
        var laterCall = (await cache.GetSport(SportId, new[] { Culture })).TournamentIds.ToList();

        Assert.Equal(
            new[] { new URN("od:tournament:9"), new URN("od:tournament:3"), new URN("od:tournament:7") },
            firstCall);
        Assert.Equal(firstCall, laterCall);
    }

    [Fact]
    public void GetSportTournamentsSkipsAMalformedIdAndKeepsTheRest()
    {
        var api = DispatchProxy.Create<IApiClient, SportsApiClientProxy>();
        var proxy = (SportsApiClientProxy)api;
        proxy.TournamentIds = new[] { "bogus", "od:tournament:2" };

        using var cache = new SportDataCache(api);

        var returned = cache.GetSportTournaments(SportId, Culture).ToList();

        Assert.Equal(new[] { new URN("od:tournament:2") }, returned);
    }

    private static sport Sport() => new() { id = "od:sport:1", name = "Dota 2" };

    private static TournamentInfoModel TournamentInfo(string tournamentId = "od:tournament:1") =>
        new()
        {
            tournament = new tournamentExtended
            {
                id = tournamentId,
                name = "The International",
                sport = Sport(),
                competitors = Array.Empty<team>()
            }
        };

    private static SportsModel Sports() =>
        new() { sport = new[] { new sportExtended { id = "od:sport:1", name = "Dota 2" } } };

    public class SportsApiClientProxy : DispatchProxy
    {
        private readonly Subject<IRequestResult<object>> _responses = new();

        public string[] TournamentIds { get; set; } = { "od:tournament:1", "od:tournament:2" };

        public void Publish(object data, CultureInfo culture) =>
            _responses.OnNext(RequestResult<object>.Success(
                data,
                HttpStatusCode.OK,
                string.Empty,
                culture: culture));

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            switch (targetMethod.Name)
            {
                case nameof(IApiClient.SubscribeForClass):
                    return _responses.OfType<IRequestResult<object>>();
                case nameof(IApiClient.GetSports):
                    return Task.FromResult(Sports());
                case nameof(IApiClient.GetTournaments):
                    var tournaments = new TournamentsModel
                    {
                        sport = Sport(),
                        tournaments = TournamentIds
                            .Select(id => new tournament { id = id, sport = Sport() })
                            .ToList()
                    };
                    Publish(tournaments, (CultureInfo)args[1]);
                    return tournaments;
                default:
                    throw new InvalidOperationException($"API unavailable: {targetMethod.Name}");
            }
        }
    }
}

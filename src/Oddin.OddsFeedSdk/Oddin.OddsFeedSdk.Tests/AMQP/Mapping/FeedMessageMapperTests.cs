using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.AMQP.Mapping;
using Oddin.OddsFeedSdk.AMQP.Messages;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Managers.Abstractions;
using Xunit;

namespace Oddin.OddsFeedSdk.Tests.AMQP.Mapping;

public class FeedMessageMapperTests
{
    private const string MatchId = "od:match:123";
    private const string TournamentId = "od:tournament:456";

    [Fact]
    public void MapBetStopPassesRoutingKeySportToMatchBuilder()
    {
        var builder = new CapturingSportDataBuilder();
        var mapper = CreateMapper(builder);

        mapper.MapBetStop<IMatch>(CreateMessage(MatchId, "42"), Array.Empty<CultureInfo>(), Array.Empty<byte>());

        Assert.Equal(MatchId, builder.MatchId?.ToString());
        Assert.Equal("od:sport:42", builder.MatchSportId?.ToString());
        Assert.Equal(1, builder.MatchBuildCount);
    }

    [Fact]
    public void MapOddsChangePassesRoutingKeySportToMatchBuilder()
    {
        var builder = new CapturingSportDataBuilder();
        var mapper = CreateMapper(builder);
        var message = new odds_change
        {
            event_id = MatchId,
            product = 1,
            timestamp = 1,
            RoutingKey = "hi.pre.-.odds_change.42.-.123.-"
        };

        mapper.MapOddsChange<IMatch>(message, Array.Empty<CultureInfo>(), Array.Empty<byte>());

        Assert.Equal("od:sport:42", builder.MatchSportId?.ToString());
        Assert.Equal(1, builder.MatchBuildCount);
    }

    [Fact]
    public void MapOddsChangePreservesMatchFallbackWhenRoutingKeyHasNoSport()
    {
        var builder = new CapturingSportDataBuilder();
        var mapper = CreateMapper(builder);
        var message = new odds_change
        {
            event_id = MatchId,
            product = 1,
            timestamp = 1,
            RoutingKey = "hi.pre.-.odds_change.-.-.123.-"
        };

        mapper.MapOddsChange<IMatch>(message, Array.Empty<CultureInfo>(), Array.Empty<byte>());

        Assert.Null(builder.MatchSportId);
        Assert.Equal(1, builder.MatchBuildCount);
    }

    [Fact]
    public void MapBetStopPreservesMatchFallbackWhenRoutingKeyHasNoSport()
    {
        var builder = new CapturingSportDataBuilder();
        var mapper = CreateMapper(builder);

        mapper.MapBetStop<IMatch>(CreateMessage(MatchId, "-"), Array.Empty<CultureInfo>(), Array.Empty<byte>());

        Assert.Null(builder.MatchSportId);
        Assert.Equal(1, builder.MatchBuildCount);
    }

    [Fact]
    public void MapBetStopStillPassesRoutingKeySportToTournamentBuilder()
    {
        var builder = new CapturingSportDataBuilder();
        var mapper = CreateMapper(builder);

        mapper.MapBetStop<ITournament>(CreateMessage(TournamentId, "42"), Array.Empty<CultureInfo>(), Array.Empty<byte>());

        Assert.Equal(TournamentId, builder.TournamentId?.ToString());
        Assert.Equal("od:sport:42", builder.TournamentSportId?.ToString());
        Assert.Equal(1, builder.TournamentBuildCount);
    }

    private static FeedMessageMapper CreateMapper(CapturingSportDataBuilder builder) =>
        new(
            new StubProducerManager(),
            new StubFeedConfiguration(),
            null,
            builder);

    private static bet_stop CreateMessage(string eventId, string sportSection) =>
        new()
        {
            event_id = eventId,
            product = 1,
            timestamp = 1,
            RoutingKey = $"hi.pre.-.bet_stop.{sportSection}.-.123.-"
        };

    private sealed class CapturingSportDataBuilder : ISportDataBuilder
    {
        public URN MatchId { get; private set; }
        public URN MatchSportId { get; private set; }
        public int MatchBuildCount { get; private set; }
        public URN TournamentId { get; private set; }
        public URN TournamentSportId { get; private set; }
        public int TournamentBuildCount { get; private set; }

        public IMatch BuildMatch(URN id, IEnumerable<CultureInfo> cultures, URN sportId = null)
        {
            MatchId = id;
            MatchSportId = sportId;
            MatchBuildCount++;
            return null;
        }

        public ITournament BuildTournament(URN id, URN sportId, IEnumerable<CultureInfo> locales)
        {
            TournamentId = id;
            TournamentSportId = sportId;
            TournamentBuildCount++;
            return null;
        }

        public Task<IEnumerable<ISport>> BuildSports(IEnumerable<CultureInfo> locales) =>
            Task.FromResult<IEnumerable<ISport>>(Array.Empty<ISport>());

        public IEnumerable<ITournament> BuildTournaments(IEnumerable<URN> ids, URN sportId,
            IEnumerable<CultureInfo> locales) => Array.Empty<ITournament>();

        public ISport BuildSport(URN id, IEnumerable<CultureInfo> locales) => null;
        public IEnumerable<ICompetitor> BuildCompetitors(IEnumerable<URN> ids,
            IEnumerable<CultureInfo> cultures) => Array.Empty<ICompetitor>();
        public IEnumerable<IMatch> BuildMatches(IEnumerable<URN> ids, IEnumerable<CultureInfo> cultures) =>
            Array.Empty<IMatch>();
        public ICompetitor BuildCompetitor(URN id, IEnumerable<CultureInfo> cultures) => null;
        public IPlayer BuildPlayer(URN id, IEnumerable<CultureInfo> cultures) => null;
        public IFixture BuildFixture(URN id, IEnumerable<CultureInfo> cultures) => null;
        public IMatchStatus BuildMatchStatus(URN id, IEnumerable<CultureInfo> cultures) => null;
    }

    private sealed class StubProducerManager : IProducerManager
    {
        public IReadOnlyCollection<IProducer> Producers => Array.Empty<IProducer>();
        public void DisableProducer(int id) { }
        public IProducer Get(int id) => null;
        public IProducer Get(string name) => null;
        public bool Exists(int id) => false;
        public bool Exists(string name) => false;
        public void AddTimestampBeforeDisconnect(int id, DateTime timestamp) { }
        public void RemoveTimestampBeforeDisconnect(int id) { }
        public void Lock() { }
    }

    private sealed class StubFeedConfiguration : IFeedConfiguration
    {
        public string AccessToken => string.Empty;
        public CultureInfo DefaultLocale => CultureInfo.GetCultureInfo("en-US");
        public int MaxInactivitySeconds => 0;
        public int MaxRecoveryExecutionMinutes => 0;
        public int? NodeId => null;
        public ExceptionHandlingStrategy ExceptionHandlingStrategy => ExceptionHandlingStrategy.CATCH;
        public string Host => string.Empty;
        public int Port => 0;
        public bool UseSsl => false;
        public string ApiHost => string.Empty;
        public bool UseApiSsl => false;
        public int HttpClientTimeout => 0;
        public int InitialSnapshotTimeInMinutes => 0;
    }
}

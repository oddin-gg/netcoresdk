using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.API;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.API.Models;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Exceptions;
using Xunit;

namespace Oddin.OddsFeedSdk.Tests.API.Entities;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class MatchLoggerCollection
{
    public const string Name = "Match logger";
}

[Collection(MatchLoggerCollection.Name)]
public class MatchTests
{
    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");
    private static readonly URN MatchId = new("od:match:123");
    private static readonly URN SportId = new("od:sport:42");
    private static readonly URN TournamentId = new("od:tournament:456");
    private static readonly DateTime ScheduledTime = new(2026, 8, 5, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime ScheduledEndTime = ScheduledTime.AddHours(2);
    private static readonly TestSdkLogger.RecordingLogger Logger = TestSdkLogger.Logger;

    [Fact]
    public void SportIdUsesRoutingKeyWithoutReadingMatchCache()
    {
        var matchCache = new SequenceMatchCache((LocalizedMatch)null);
        var match = CreateMatch(matchCache, ExceptionHandlingStrategy.CATCH);

        Assert.Equal(SportId, match.SportId);
        Assert.Equal(0, matchCache.GetMatchCount);
    }

    [Theory]
    [InlineData(ExceptionHandlingStrategy.CATCH)]
    [InlineData(ExceptionHandlingStrategy.THROW)]
    public async Task TournamentResolvesThroughFixtureFallbackWhenSummaryFails(
        ExceptionHandlingStrategy strategy)
    {
        var apiClient = DispatchProxy.Create<IApiClient, PublishingApiClientProxy>();
        var proxy = (PublishingApiClientProxy)(object)apiClient;
        proxy.Fixture = CreateFixture(SportId, TournamentId);
        using var matchCache = new MatchCache(apiClient);
        var match = CreateMatch(matchCache, strategy);

        var tournament = match.Tournament;
        var sport = await tournament.GetSportAsync();

        Assert.Equal(TournamentId, tournament.Id);
        Assert.Equal(SportId, sport.Id);
        Assert.Equal(1, proxy.GetMatchSummaryCount);
        Assert.Equal(1, proxy.GetFixtureCount);
    }

    [Fact]
    public async Task TournamentFallbackWorksForMatchWithoutRoutingKeySport()
    {
        var apiClient = DispatchProxy.Create<IApiClient, PublishingApiClientProxy>();
        var proxy = (PublishingApiClientProxy)(object)apiClient;
        proxy.Fixture = CreateFixture(SportId, TournamentId);
        using var matchCache = new MatchCache(apiClient);
        var match = CreateMatchWithoutRoutingSport(matchCache, ExceptionHandlingStrategy.CATCH);

        var tournament = match.Tournament;
        var sport = await tournament.GetSportAsync();

        Assert.Equal(TournamentId, tournament.Id);
        Assert.Equal(SportId, sport.Id);
        Assert.Equal(1, proxy.GetFixtureCount);
    }

    [Fact]
    public void WarmFixtureCacheDoesNotMakeTournamentFallbackANoop()
    {
        var apiClient = DispatchProxy.Create<IApiClient, PublishingApiClientProxy>();
        var proxy = (PublishingApiClientProxy)(object)apiClient;
        proxy.Fixture = CreateFixture(SportId, TournamentId);
        var fixtureCache = new FixtureCache(apiClient);
        _ = fixtureCache.GetFixture(MatchId, English);
        using var matchCache = new MatchCache(apiClient);
        var match = CreateMatch(matchCache, ExceptionHandlingStrategy.CATCH);

        var tournament = match.Tournament;

        Assert.Equal(TournamentId, tournament.Id);
        Assert.Equal(2, proxy.GetFixtureCount);
    }

    [Fact]
    public void TournamentRemainsNullWhenSummaryAndFixtureFailUnderCatchStrategy()
    {
        var apiClient = DispatchProxy.Create<IApiClient, PublishingApiClientProxy>();
        var proxy = (PublishingApiClientProxy)(object)apiClient;
        using var matchCache = new MatchCache(apiClient);
        var match = CreateMatch(matchCache, ExceptionHandlingStrategy.CATCH);

        Assert.Null(match.Tournament);
        Assert.Equal(1, proxy.GetMatchSummaryCount);
        Assert.Equal(1, proxy.GetFixtureCount);
    }

    [Fact]
    public void TournamentPreservesUnableToFetchMatchExceptionWhenBothEndpointsFailUnderThrowStrategy()
    {
        var apiClient = DispatchProxy.Create<IApiClient, PublishingApiClientProxy>();
        var proxy = (PublishingApiClientProxy)(object)apiClient;
        using var matchCache = new MatchCache(apiClient);
        var match = CreateMatch(matchCache, ExceptionHandlingStrategy.THROW);

        var exception = Assert.Throws<ItemNotFoundException>(() => _ = match.Tournament);

        Assert.Equal("Unable to fetch match", exception.Message);
        Assert.Equal(MatchId.ToString(), exception.Id);
        Assert.Equal(1, proxy.GetMatchSummaryCount);
        Assert.Equal(1, proxy.GetFixtureCount);
    }

    [Fact]
    public void TournamentWithCachedIdSkipsFixtureFallback()
    {
        var matchCache = new SequenceMatchCache(CreateLocalizedMatch(SportId, TournamentId));
        var match = CreateMatch(matchCache, ExceptionHandlingStrategy.CATCH);

        Assert.Equal(TournamentId, match.Tournament.Id);
        Assert.Equal(1, matchCache.GetMatchCount);
    }

    [Fact]
    public void TournamentUsesRoutingKeySportWhenSummaryHasTournamentWithoutSport()
    {
        var matchCache = new SequenceMatchCache(CreateLocalizedMatch(null, TournamentId));
        var builder = new StubSportDataBuilder();
        var match = CreateMatch(matchCache, ExceptionHandlingStrategy.CATCH, builder);

        var tournament = match.Tournament;

        Assert.Equal(TournamentId, tournament.Id);
        Assert.Equal(SportId, builder.LastTournamentSportId);
    }

    [Theory]
    [InlineData(ExceptionHandlingStrategy.CATCH)]
    [InlineData(ExceptionHandlingStrategy.THROW)]
    public void CachedMatchWithoutTournamentIdPreservesExistingBehaviorAndSkipsFixtureFallback(
        ExceptionHandlingStrategy strategy)
    {
        var matchCache = new SequenceMatchCache(CreateLocalizedMatch(SportId, null));
        var match = CreateMatch(matchCache, strategy);

        if (strategy == ExceptionHandlingStrategy.THROW)
        {
            var exception = Assert.Throws<ItemNotFoundException>(() => _ = match.Tournament);
            Assert.Equal("Cannot load tournament", exception.Message);
            Assert.Equal("null", exception.Id);
        }
        else
        {
            Assert.Null(match.Tournament);
        }

        Assert.Equal(1, matchCache.GetMatchCount);
    }

    [Fact]
    public void TournamentLogsSportConflictAndKeepsRoutingKeyPrecedence()
    {
        Logger.Clear();
        var summarySportId = new URN("od:sport:99");
        var builder = new StubSportDataBuilder();
        var match = CreateMatch(
            new SequenceMatchCache(CreateLocalizedMatch(summarySportId, TournamentId)),
            ExceptionHandlingStrategy.CATCH,
            builder);

        _ = match.Tournament;
        _ = match.Tournament;

        Assert.Equal(SportId, builder.LastTournamentSportId);
        Assert.Single(
            Logger.Entries,
            entry => entry.Level == LogLevel.Warning
                     && entry.Message.Contains(MatchId.ToString())
                     && entry.Message.Contains(SportId.ToString())
                     && entry.Message.Contains(summarySportId.ToString()));
    }

    [Fact]
    public void FixtureFallbackCompletesWithoutReenteringMatchCacheSemaphore()
    {
        var apiClient = DispatchProxy.Create<IApiClient, PublishingApiClientProxy>();
        var proxy = (PublishingApiClientProxy)(object)apiClient;
        proxy.Fixture = CreateFixture(SportId, TournamentId);
        using var matchCache = new MatchCache(apiClient);
        var fixtureCache = new FixtureCache(apiClient);
        var match = CreateMatch(matchCache, ExceptionHandlingStrategy.CATCH);

        var task = Task.Run(() => match.Tournament);

        Assert.True(task.Wait(TimeSpan.FromSeconds(2)), "Tournament lookup deadlocked.");
        Assert.Equal(TournamentId, task.Result.Id);
        Assert.Equal(1, proxy.GetFixtureCount);
    }

    [Fact]
    public void PartialFixtureUpdatePreservesCompleteCachedMatchFieldsAndUpdatesName()
    {
        var apiClient = DispatchProxy.Create<IApiClient, PublishingApiClientProxy>();
        var proxy = (PublishingApiClientProxy)(object)apiClient;
        proxy.Summary = CreateCompleteSummary();
        using var matchCache = new MatchCache(apiClient);

        _ = matchCache.GetMatch(MatchId, new[] { English });
        proxy.Summary = null;
        proxy.Publish(
            new FixturesEndpointModel
            {
                fixture = new fixture { id = MatchId, name = "Updated fixture name" }
            },
            English);
        var updated = matchCache.GetMatch(MatchId, new[] { English });

        Assert.Equal(1, proxy.GetMatchSummaryCount);
        Assert.Contains(English, updated.LoadedLocals);
        Assert.Equal(new URN("od:match:999"), updated.RefId);
        Assert.Equal(ScheduledTime, updated.ScheduledTime);
        Assert.Equal(ScheduledEndTime, updated.ScheduledEndTime);
        Assert.Equal(SportId, updated.SportId);
        Assert.Equal(TournamentId, updated.TournamentId);
        var competitor = Assert.Single(updated.Competitors);
        Assert.Equal(new URN("od:team:777"), competitor.Id);
        Assert.Equal("home", competitor.Qualifier);
        Assert.True(updated.SportFormat.IsRace());
        Assert.Equal(LiveOddsAvailability.NOT_AVAILABLE, updated.LiveOddsAvailability);
        Assert.Equal("Updated fixture name", updated.Name[English]);

        proxy.Publish(
            new ScheduleEndpointModel
            {
                sport_event = new[]
                {
                    new sportEvent { id = MatchId, name = "Updated schedule name" }
                }
            },
            English);
        updated = matchCache.GetMatch(MatchId, new[] { English });

        Assert.Equal("keep", updated.ExtraInfo["custom"]);
    }

    [Fact]
    public void PartialExtraInfoUpdatePreservesSportFormatEntryWhenOmitted()
    {
        var apiClient = DispatchProxy.Create<IApiClient, PublishingApiClientProxy>();
        var proxy = (PublishingApiClientProxy)(object)apiClient;
        proxy.Summary = CreateCompleteSummary();
        using var matchCache = new MatchCache(apiClient);

        _ = matchCache.GetMatch(MatchId, new[] { English });
        proxy.Publish(
            new ScheduleEndpointModel
            {
                sport_event = new[]
                {
                    new sportEvent
                    {
                        id = MatchId,
                        extra_info = new[]
                        {
                            new info { key = "custom", value = "updated" }
                        }
                    }
                }
            },
            English);
        var updated = matchCache.GetMatch(MatchId, new[] { English });

        Assert.True(updated.SportFormat.IsRace());
        Assert.Equal(SportFormat.Race.Value, updated.ExtraInfo[MatchCache.EXTRA_INFO_KEY_SPORT_FORMAT]);
        Assert.Equal("updated", updated.ExtraInfo["custom"]);
    }

    [Fact]
    public void FixtureExtraInfoUpdatesSportFormatAndCachedExtraInfo()
    {
        var apiClient = DispatchProxy.Create<IApiClient, PublishingApiClientProxy>();
        var proxy = (PublishingApiClientProxy)(object)apiClient;
        proxy.Summary = CreateCompleteSummary();
        proxy.Summary.sport_event.extra_info = new[]
        {
            new info { key = "custom", value = "keep" }
        };
        using var matchCache = new MatchCache(apiClient);

        var cached = matchCache.GetMatch(MatchId, new[] { English });
        Assert.True(cached.SportFormat.IsClassic());
        proxy.Summary = null;

        proxy.Publish(
            new FixturesEndpointModel
            {
                fixture = new fixture
                {
                    id = MatchId,
                    extra_info = new[]
                    {
                        new info { key = MatchCache.EXTRA_INFO_KEY_SPORT_FORMAT, value = SportFormat.Race.Value },
                        new info { key = "fixture", value = "delivered" }
                    }
                }
            },
            English);
        var updated = matchCache.GetMatch(MatchId, new[] { English });

        Assert.True(updated.SportFormat.IsRace());
        Assert.Equal(SportFormat.Race.Value, updated.ExtraInfo[MatchCache.EXTRA_INFO_KEY_SPORT_FORMAT]);
        Assert.Equal("delivered", updated.ExtraInfo["fixture"]);
        // Merge, not replace: the summary-cached key the fixture omits must survive.
        Assert.Equal("keep", updated.ExtraInfo["custom"]);
    }

    [Fact]
    public void FixtureExtraInfoMergeLetsPayloadKeyOverrideCachedValue()
    {
        var apiClient = DispatchProxy.Create<IApiClient, PublishingApiClientProxy>();
        var proxy = (PublishingApiClientProxy)(object)apiClient;
        proxy.Summary = CreateCompleteSummary();
        proxy.Summary.sport_event.extra_info = new[]
        {
            new info { key = "custom", value = "old" }
        };
        using var matchCache = new MatchCache(apiClient);

        _ = matchCache.GetMatch(MatchId, new[] { English });
        proxy.Summary = null;
        proxy.Publish(
            new FixturesEndpointModel
            {
                fixture = new fixture
                {
                    id = MatchId,
                    extra_info = new[]
                    {
                        new info { key = "custom", value = "new" }
                    }
                }
            },
            English);
        var updated = matchCache.PeekMatch(MatchId);

        // Present key wins over the cached value (merge is overlay, not cached-wins).
        Assert.Equal("new", updated.ExtraInfo["custom"]);
    }

    [Fact]
    public void EmptyExtraInfoPreservesCachedExtraInfo()
    {
        var apiClient = DispatchProxy.Create<IApiClient, PublishingApiClientProxy>();
        var proxy = (PublishingApiClientProxy)(object)apiClient;
        proxy.Summary = CreateCompleteSummary(); // extra_info = { sport_format = race, custom = keep }
        using var matchCache = new MatchCache(apiClient);

        _ = matchCache.GetMatch(MatchId, new[] { English });
        proxy.Summary = null;
        proxy.Publish(
            new FixturesEndpointModel
            {
                fixture = new fixture { id = MatchId, extra_info = Array.Empty<info>() }
            },
            English);
        var updated = matchCache.PeekMatch(MatchId);

        // Deliberate divergence from "empty collection clears": an empty extra_info from a
        // subset-carrying endpoint preserves the cached dictionary rather than wiping it.
        Assert.Equal("keep", updated.ExtraInfo["custom"]);
        Assert.Equal(SportFormat.Race.Value, updated.ExtraInfo[MatchCache.EXTRA_INFO_KEY_SPORT_FORMAT]);
        Assert.True(updated.SportFormat.IsRace());
    }

    [Fact]
    public void PreviouslyReturnedExtraInfoReferenceIsNotMutatedByLaterUpdate()
    {
        var apiClient = DispatchProxy.Create<IApiClient, PublishingApiClientProxy>();
        var proxy = (PublishingApiClientProxy)(object)apiClient;
        proxy.Summary = CreateCompleteSummary();
        proxy.Summary.sport_event.extra_info = new[]
        {
            new info { key = "custom", value = "keep" }
        };
        using var matchCache = new MatchCache(apiClient);

        // The reference a consumer holds after reading ExtraInfo must stay a stable snapshot.
        var firstRead = matchCache.GetMatch(MatchId, new[] { English }).ExtraInfo;
        proxy.Summary = null;
        proxy.Publish(
            new FixturesEndpointModel
            {
                fixture = new fixture
                {
                    id = MatchId,
                    extra_info = new[] { new info { key = "fixture", value = "delivered" } }
                }
            },
            English);
        var afterUpdate = matchCache.PeekMatch(MatchId).ExtraInfo;

        Assert.False(firstRead.ContainsKey("fixture")); // in-place merge would have leaked into it
        Assert.NotSame(firstRead, afterUpdate);
        Assert.Equal("delivered", afterUpdate["fixture"]);
        Assert.Equal("keep", afterUpdate["custom"]);
    }

    [Fact]
    public void UnknownSportFormatInsertsUnknownWithoutThrowingAndReachesLaterSubscriber()
    {
        Logger.Clear();
        var apiClient = DispatchProxy.Create<IApiClient, PublishingApiClientProxy>();
        var proxy = (PublishingApiClientProxy)(object)apiClient;
        using var matchCache = new MatchCache(apiClient);

        // Subscribe AFTER MatchCache: with a plain Subject the throw this PR removes would escape
        // MatchCache's OnNext and this later observer would never see the response.
        var received = new List<object>();
        using var later = apiClient
            .SubscribeForClass<IRequestResult<object>>()
            .Subscribe(r => received.Add(r.Data));

        var exception = Record.Exception(() => proxy.Publish(
            new FixturesEndpointModel
            {
                fixture = new fixture
                {
                    id = MatchId,
                    extra_info = new[]
                    {
                        new info { key = MatchCache.EXTRA_INFO_KEY_SPORT_FORMAT, value = "future_value" }
                    }
                }
            },
            English));

        Assert.Null(exception);
        Assert.Single(received);
        var cached = matchCache.PeekMatch(MatchId);
        Assert.NotNull(cached);
        Assert.True(cached.SportFormat.IsUnknown());
        Assert.Contains(
            Logger.Entries,
            entry => entry.Level == LogLevel.Warning
                     && entry.Message.Contains("Unknown sport format 'future_value'")
                     && entry.Message.Contains(MatchId.ToString()));
    }

    [Fact]
    public void UnknownSportFormatUpdatePreservesCachedKnownSportFormat()
    {
        var apiClient = DispatchProxy.Create<IApiClient, PublishingApiClientProxy>();
        var proxy = (PublishingApiClientProxy)(object)apiClient;
        proxy.Summary = CreateCompleteSummary(); // sport_format = race
        using var matchCache = new MatchCache(apiClient);

        var cached = matchCache.GetMatch(MatchId, new[] { English });
        Assert.True(cached.SportFormat.IsRace());
        proxy.Summary = null;

        proxy.Publish(
            new FixturesEndpointModel
            {
                fixture = new fixture
                {
                    id = MatchId,
                    extra_info = new[]
                    {
                        new info { key = MatchCache.EXTRA_INFO_KEY_SPORT_FORMAT, value = "future_value" }
                    }
                }
            },
            English);
        var updated = matchCache.PeekMatch(MatchId);

        // Unknown on insert is honest, but an unrecognised value must never demote a known cached one.
        Assert.True(updated.SportFormat.IsRace());
    }

    [Fact]
    public void MalformedIdInScheduleBatchDoesNotDropSiblingMatches()
    {
        var siblingId = new URN("od:match:124");
        var apiClient = DispatchProxy.Create<IApiClient, PublishingApiClientProxy>();
        var proxy = (PublishingApiClientProxy)(object)apiClient;
        using var matchCache = new MatchCache(apiClient);

        var exception = Record.Exception(() => proxy.Publish(
            new ScheduleEndpointModel
            {
                sport_event = new[]
                {
                    // Malformed competitor id throws inside RefreshOrInsertItem, before this item is cached.
                    new sportEvent
                    {
                        id = MatchId,
                        competitors = new[] { new teamCompetitor { id = "garbage", qualifier = "home" } }
                    },
                    // Clean sibling that must still be cached because the catch is per item, not per batch.
                    new sportEvent { id = siblingId, name = "Sibling" }
                }
            },
            English));

        Assert.Null(exception);
        Assert.Null(matchCache.PeekMatch(MatchId));
        var sibling = matchCache.PeekMatch(siblingId);
        Assert.NotNull(sibling);
        Assert.Equal("Sibling", sibling.Name[English]);
    }

    [Fact]
    public void SportIdLogsSportConflictAndKeepsRoutingKeyPrecedence()
    {
        Logger.Clear();
        var summarySportId = new URN("od:sport:99");
        var matchCache = new SequenceMatchCache(CreateLocalizedMatch(summarySportId, TournamentId));
        var match = CreateMatch(matchCache, ExceptionHandlingStrategy.CATCH);

        _ = match.SportId;
        _ = match.SportId;

        Assert.Equal(SportId, match.SportId);
        // Routing-key sport short-circuits, so the conflict is surfaced without loading the match.
        Assert.Equal(0, matchCache.GetMatchCount);
        Assert.Single(
            Logger.Entries,
            entry => entry.Level == LogLevel.Warning
                     && entry.Message.Contains(MatchId.ToString())
                     && entry.Message.Contains(SportId.ToString())
                     && entry.Message.Contains(summarySportId.ToString()));
    }

    [Fact]
    public void FixtureCacheToleratesDuplicateAndNullExtraInfoKeys()
    {
        Logger.Clear();
        var apiClient = DispatchProxy.Create<IApiClient, PublishingApiClientProxy>();
        var proxy = (PublishingApiClientProxy)(object)apiClient;
        proxy.Fixture = new FixturesEndpointModel
        {
            fixture = new fixture
            {
                id = MatchId,
                extra_info = new[]
                {
                    new info { key = "dup", value = "first" },
                    new info { key = "dup", value = "last" },
                    new info { key = null, value = "orphan" }
                }
            }
        };
        var fixtureCache = new FixtureCache(apiClient);

        var exception = Record.Exception(() => fixtureCache.GetFixture(MatchId, English));

        Assert.Null(exception);
        var localizedFixture = fixtureCache.GetFixture(MatchId, English);
        Assert.Equal("last", localizedFixture.ExtraInfo["dup"]);
        Assert.False(localizedFixture.ExtraInfo.ContainsKey("orphan"));
        Assert.Single(localizedFixture.ExtraInfo);
        Assert.Contains(
            Logger.Entries,
            entry => entry.Level == LogLevel.Warning
                     && entry.Message.Contains("Duplicate extra_info key 'dup'"));
    }

    [Fact]
    public void FixtureExtraInfoBridgeDoesNotMutateIncomingPayload()
    {
        var apiClient = DispatchProxy.Create<IApiClient, PublishingApiClientProxy>();
        var proxy = (PublishingApiClientProxy)(object)apiClient;
        using var matchCache = new MatchCache(apiClient);
        var payload = new fixture
        {
            id = MatchId,
            extra_info = new[]
            {
                new info { key = "fixture", value = "delivered" }
            }
        };
        var asSportEvent = (sportEvent)payload;

        proxy.Publish(new FixturesEndpointModel { fixture = payload }, English);
        var updated = matchCache.GetMatch(MatchId, new[] { English });

        Assert.Equal("delivered", updated.ExtraInfo["fixture"]);
        Assert.Null(asSportEvent.extra_info);
    }

    [Fact]
    public void DuplicateExtraInfoKeysUseLastValueAndLogWarning()
    {
        Logger.Clear();
        var apiClient = DispatchProxy.Create<IApiClient, PublishingApiClientProxy>();
        var proxy = (PublishingApiClientProxy)(object)apiClient;
        proxy.Summary = CreateCompleteSummary();
        proxy.Summary.sport_event.extra_info = new[]
        {
            new info { key = "custom", value = "first" },
            new info { key = "custom", value = "last" }
        };
        using var matchCache = new MatchCache(apiClient);

        var cached = matchCache.GetMatch(MatchId, new[] { English });

        Assert.Equal("last", cached.ExtraInfo["custom"]);
        Assert.Contains(
            Logger.Entries,
            entry => entry.Level == LogLevel.Warning
                     && entry.Message.Contains("Duplicate extra_info key 'custom'")
                     && entry.Message.Contains(MatchId.ToString()));
    }

    [Fact]
    public void DuplicateSportFormatKeysUseLastValueForSportFormatAndExtraInfo()
    {
        var apiClient = DispatchProxy.Create<IApiClient, PublishingApiClientProxy>();
        var proxy = (PublishingApiClientProxy)(object)apiClient;
        proxy.Summary = CreateCompleteSummary();
        proxy.Summary.sport_event.extra_info = new[]
        {
            new info { key = MatchCache.EXTRA_INFO_KEY_SPORT_FORMAT, value = SportFormat.Classic.Value },
            new info { key = MatchCache.EXTRA_INFO_KEY_SPORT_FORMAT, value = SportFormat.Race.Value }
        };
        using var matchCache = new MatchCache(apiClient);

        var cached = matchCache.GetMatch(MatchId, new[] { English });

        Assert.True(cached.SportFormat.IsRace());
        Assert.Equal(SportFormat.Race.Value, cached.ExtraInfo[MatchCache.EXTRA_INFO_KEY_SPORT_FORMAT]);
    }

    [Fact]
    public void InvalidFirstSportFormatDuplicateDoesNotAbortWhenLastValueIsValid()
    {
        var apiClient = DispatchProxy.Create<IApiClient, PublishingApiClientProxy>();
        var proxy = (PublishingApiClientProxy)(object)apiClient;
        proxy.Summary = CreateCompleteSummary();
        proxy.Summary.sport_event.extra_info = new[]
        {
            new info { key = MatchCache.EXTRA_INFO_KEY_SPORT_FORMAT, value = "invalid" },
            new info { key = MatchCache.EXTRA_INFO_KEY_SPORT_FORMAT, value = SportFormat.Race.Value }
        };
        using var matchCache = new MatchCache(apiClient);

        var cached = matchCache.GetMatch(MatchId, new[] { English });

        Assert.NotNull(cached);
        Assert.True(cached.SportFormat.IsRace());
        Assert.Equal(SportFormat.Race.Value, cached.ExtraInfo[MatchCache.EXTRA_INFO_KEY_SPORT_FORMAT]);
    }

    [Fact]
    public void FixtureOnlyPopulationLeavesCultureEligibleForSummaryRetry()
    {
        var apiClient = DispatchProxy.Create<IApiClient, PublishingApiClientProxy>();
        var proxy = (PublishingApiClientProxy)(object)apiClient;
        using var matchCache = new MatchCache(apiClient);

        proxy.Publish(CreateFixture(SportId, TournamentId), English);
        var fixtureOnly = matchCache.GetMatch(MatchId, new[] { English });

        Assert.Equal(1, proxy.GetMatchSummaryCount);
        Assert.DoesNotContain(English, fixtureOnly.LoadedLocals);
    }

    [Fact]
    public void SuccessfulSummaryAfterFixtureFallbackStopsFurtherRetries()
    {
        var apiClient = DispatchProxy.Create<IApiClient, PublishingApiClientProxy>();
        var proxy = (PublishingApiClientProxy)(object)apiClient;
        using var matchCache = new MatchCache(apiClient);

        var fixture = CreateFixture(SportId, TournamentId);
        fixture.fixture.name = "Fixture-only name";
        proxy.Publish(fixture, English);
        var fixtureOnly = matchCache.GetMatch(MatchId, new[] { English });
        Assert.DoesNotContain(English, fixtureOnly.LoadedLocals);
        Assert.Equal("Fixture-only name", fixtureOnly.Name[English]);

        proxy.Summary = CreateCompleteSummary();
        var summaryLoaded = matchCache.GetMatch(MatchId, new[] { English });
        Assert.Contains(English, summaryLoaded.LoadedLocals);
        Assert.Equal("Complete summary name", summaryLoaded.Name[English]);
        Assert.Equal(2, proxy.GetMatchSummaryCount);

        proxy.Summary = null;
        _ = matchCache.GetMatch(MatchId, new[] { English });

        Assert.Equal(2, proxy.GetMatchSummaryCount);
    }

    [Fact]
    public void ExplicitlyEmptyCompetitorsStillClearCachedCompetitors()
    {
        var apiClient = DispatchProxy.Create<IApiClient, PublishingApiClientProxy>();
        var proxy = (PublishingApiClientProxy)(object)apiClient;
        proxy.Summary = CreateCompleteSummary();
        using var matchCache = new MatchCache(apiClient);

        _ = matchCache.GetMatch(MatchId, new[] { English });
        proxy.Summary = null;
        proxy.Publish(
            new FixturesEndpointModel
            {
                fixture = new fixture
                {
                    id = MatchId,
                    name = "Empty competitors update",
                    competitors = Array.Empty<teamCompetitor>()
                }
            },
            English);
        var updated = matchCache.GetMatch(MatchId, new[] { English });

        Assert.Empty(updated.Competitors);
        Assert.Equal("Empty competitors update", updated.Name[English]);
    }

    [Fact]
    public void CompetitorsReturnsEmptyWhenCachedCompetitorCollectionIsMissing()
    {
        var cached = CreateLocalizedMatch(SportId, TournamentId);
        cached.Competitors = null;
        var match = CreateMatch(
            new SequenceMatchCache(cached),
            ExceptionHandlingStrategy.CATCH);

        Assert.Empty(match.Competitors);
    }

    [Theory]
    [InlineData(ExceptionHandlingStrategy.CATCH)]
    [InlineData(ExceptionHandlingStrategy.THROW)]
    public void HomeCompetitorTreatsMissingCachedCompetitorsAsInsufficientData(
        ExceptionHandlingStrategy strategy)
    {
        var cached = CreateLocalizedMatch(SportId, TournamentId);
        cached.Competitors = null;
        var match = CreateMatch(
            new SequenceMatchCache(cached),
            strategy);

        if (strategy == ExceptionHandlingStrategy.THROW)
        {
            var exception = Assert.Throws<ArgumentException>(() => _ = match.HomeCompetitor);
            Assert.Contains("less than 2 competitors", exception.Message);
        }
        else
        {
            Assert.Null(match.HomeCompetitor);
        }
    }

    private static Match CreateMatch(
        IMatchCache matchCache,
        ExceptionHandlingStrategy strategy,
        StubSportDataBuilder builder = null) =>
        new(
            MatchId,
            SportId,
            matchCache,
            builder ?? new StubSportDataBuilder(),
            strategy,
            new[] { English });

    private static Match CreateMatchWithoutRoutingSport(
        IMatchCache matchCache,
        ExceptionHandlingStrategy strategy,
        StubSportDataBuilder builder = null) =>
        new(
            MatchId,
            null,
            matchCache,
            builder ?? new StubSportDataBuilder(),
            strategy,
            new[] { English });

    private static LocalizedMatch CreateLocalizedMatch(URN sportId, URN tournamentId) =>
        new(MatchId)
        {
            SportId = sportId,
            TournamentId = tournamentId
        };

    private static FixturesEndpointModel CreateFixture(URN sportId, URN tournamentId) =>
        new()
        {
            fixture = new fixture
            {
                id = MatchId,
                tournament = new tournament
                {
                    id = tournamentId,
                    sport = sportId is null ? null : new sport { id = sportId }
                }
            }
        };

    private static MatchSummaryModel CreateCompleteSummary() =>
        new()
        {
            sport_event = new sportEvent
            {
                id = MatchId,
                name = "Complete summary name",
                refid = "od:match:999",
                scheduled = ScheduledTime,
                scheduledSpecified = true,
                scheduled_end = ScheduledEndTime,
                scheduled_endSpecified = true,
                liveodds = "not_available",
                tournament = new tournament
                {
                    id = TournamentId,
                    sport = new sport { id = SportId }
                },
                competitors = new[]
                {
                    new teamCompetitor { id = "od:team:777", qualifier = "home" }
                },
                extra_info = new[]
                {
                    new info { key = MatchCache.EXTRA_INFO_KEY_SPORT_FORMAT, value = SportFormat.Race.Value },
                    new info { key = "custom", value = "keep" }
                }
            }
        };

    public class PublishingApiClientProxy : DispatchProxy
    {
        private readonly Subject<IRequestResult<object>> _responses = new();

        public FixturesEndpointModel Fixture { get; set; }
        public MatchSummaryModel Summary { get; set; }
        public int GetFixtureCount { get; private set; }
        public int GetMatchSummaryCount { get; private set; }

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
                case nameof(IApiClient.GetMatchSummary):
                    GetMatchSummaryCount++;
                    return Summary ?? throw new InvalidOperationException("summary unavailable");
                case nameof(IApiClient.GetFixture):
                    GetFixtureCount++;
                    Publish(Fixture, (CultureInfo)args[1]);
                    return Fixture;
                case nameof(IApiClient.SubscribeForClass):
                    return _responses.OfType<IRequestResult<object>>();
                default:
                    throw new NotSupportedException($"Unexpected API call: {targetMethod.Name}");
            }
        }
    }

    private sealed class SequenceMatchCache : IMatchCache
    {
        private readonly LocalizedMatch[] _matches;

        public SequenceMatchCache(params LocalizedMatch[] matches) => _matches = matches;

        public int GetMatchCount { get; private set; }

        public LocalizedMatch GetMatch(URN id, IEnumerable<CultureInfo> cultures)
        {
            var index = Math.Min(GetMatchCount, _matches.Length - 1);
            GetMatchCount++;
            return _matches[index];
        }

        public void ClearCacheItem(URN id) { }
        public void LoadFixture(URN id, CultureInfo culture) { }
        public void OnFeedMessageReceived(fixture_change e) { }
        public LocalizedMatch PeekMatch(URN id) => _matches.LastOrDefault();
        public void Dispose() { }
    }

    private sealed class StubSportDataBuilder : ISportDataBuilder
    {
        public URN LastTournamentSportId { get; private set; }

        public ITournament BuildTournament(URN id, URN sportId, IEnumerable<CultureInfo> locales)
        {
            LastTournamentSportId = sportId;
            return new StubTournament(id, sportId);
        }

        public ISport BuildSport(URN id, IEnumerable<CultureInfo> locales) => new StubSport(id);
        public Task<IEnumerable<ISport>> BuildSports(IEnumerable<CultureInfo> locales) =>
            Task.FromResult<IEnumerable<ISport>>(Array.Empty<ISport>());
        public IEnumerable<ITournament> BuildTournaments(IEnumerable<URN> ids, URN sportId,
            IEnumerable<CultureInfo> locales) => Array.Empty<ITournament>();
        public IEnumerable<ICompetitor> BuildCompetitors(IEnumerable<URN> ids,
            IEnumerable<CultureInfo> cultures) => Array.Empty<ICompetitor>();
        public IEnumerable<IMatch> BuildMatches(IEnumerable<URN> ids, IEnumerable<CultureInfo> cultures) =>
            Array.Empty<IMatch>();
        public IMatch BuildMatch(URN id, IEnumerable<CultureInfo> cultures, URN sportId = null) => null;
        public ICompetitor BuildCompetitor(URN id, IEnumerable<CultureInfo> cultures) => null;
        public IPlayer BuildPlayer(URN id, IEnumerable<CultureInfo> cultures) => null;
        public IFixture BuildFixture(URN id, IEnumerable<CultureInfo> cultures) => null;
        public IMatchStatus BuildMatchStatus(URN id, IEnumerable<CultureInfo> cultures) => null;
    }

    private sealed class StubTournament : ITournament
    {
        private readonly URN _sportId;

        public StubTournament(URN id, URN sportId)
        {
            Id = id;
            _sportId = sportId;
        }

        public URN Id { get; }
        public URN RefId => null;
        public string IconPath => null;
        public Task<string> GetNameAsync(CultureInfo culture) => Task.FromResult<string>(null);
        public Task<URN> GetSportIdAsync() => Task.FromResult(_sportId);
        public Task<ISport> GetSportAsync() => Task.FromResult<ISport>(new StubSport(_sportId));
        public Task<DateTime?> GetScheduledTimeAsync() => Task.FromResult<DateTime?>(null);
        public Task<DateTime?> GetScheduledEndTimeAsync() => Task.FromResult<DateTime?>(null);
        public IEnumerable<ICompetitor> GetCompetitors() => Array.Empty<ICompetitor>();
        public DateTime? GetEndDate() => null;
        public DateTime? GetStartDate() => null;
        public int? RiskTier() => null;
    }

    private sealed class StubSport : ISport
    {
        public StubSport(URN id) => Id = id;

        public URN Id { get; }
        public URN RefId => null;
        public IReadOnlyDictionary<CultureInfo, string> Names { get; } =
            new Dictionary<CultureInfo, string>();
        public IEnumerable<ITournament> Tournaments => Array.Empty<ITournament>();
        public string IconPath => null;
        public string GetName(CultureInfo culture) => null;
    }

}

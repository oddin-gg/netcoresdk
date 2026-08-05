using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.API.Models;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;

namespace Oddin.OddsFeedSdk.API;

internal class MatchCache : IMatchCache
{
    private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(MatchCache));

    private readonly IApiClient _apiClient;
    private readonly MemoryCache _cache = new(nameof(MatchCache));
    private readonly TimeSpan _cacheTtl = TimeSpan.FromHours(12);
    private readonly Semaphore _semaphore = new(1, 1);
    private readonly IDisposable _subscription;

    public const string EXTRA_INFO_KEY_SPORT_FORMAT = "sport_format";

    public MatchCache(IApiClient apiClient)
    {
        _apiClient = apiClient;

        _subscription = apiClient.SubscribeForClass<IRequestResult<object>>()
            .Subscribe(response =>
            {
                if (response.Culture is null || response.Data is null)
                    return;

                if (response.Data is FixturesEndpointModel fixture)
                {
                    _semaphore.WaitOne();
                    try
                    {
                        _log.LogDebug($"Updating Match cache from API: {response.Data.GetType()}");
                        HandleMatchData(
                            response.Culture,
                            new List<sportEvent> { fixture.fixture },
                            fromFixture: true,
                            fixture.fixture?.extra_info);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }

                    return;
                }

                var matches = response.Data switch
                {
                    ScheduleEndpointModel s => s.sport_event.ToList(),
                    TournamentScheduleModel t => t.sport_events.SelectMany(s => s).ToList(),
                    _ => new List<sportEvent>()
                };


                if (matches.Any())
                {
                    _semaphore.WaitOne();
                    try
                    {
                        _log.LogDebug($"Updating Match cache from API: {response.Data.GetType()}");
                        HandleMatchData(response.Culture, matches, fromFixture: false);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }
            });
    }

    public void OnFeedMessageReceived(fixture_change e)
    {
        var id = string.IsNullOrEmpty(e?.event_id)
            ? null
            : new URN(e.event_id);

        if (id != null)
        {
            _log.LogDebug($"Invalidating Tournament cache from FEED for: {id}");
            _cache.Remove(id.ToString());
        }
    }

    public LocalizedMatch GetMatch(URN id, IEnumerable<CultureInfo> cultures)
    {
        _semaphore.WaitOne();
        try
        {
            var localizedMatch = _cache.Get(id.ToString()) as LocalizedMatch;
            var localizedAlready = localizedMatch?.LoadedLocals ?? new List<CultureInfo>();

            var culturesToLoad = cultures.Except(localizedAlready);
            if (culturesToLoad.Any())
                LoadAndCacheItem(id, culturesToLoad);

            return _cache.Get(id.ToString()) as LocalizedMatch;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void ClearCacheItem(URN id) => _cache.Remove(id.ToString());

    public LocalizedMatch PeekMatch(URN id) => _cache.Get(id.ToString()) as LocalizedMatch;

    public void LoadFixture(URN id, CultureInfo culture)
    {
        try
        {
            _apiClient.GetFixture(id, culture);
        }
        catch (Exception e)
        {
            _log.LogError($"Error while fetching fixture for {id} ({culture.TwoLetterISOLanguageName}): {e}");
        }
    }

    public void Dispose() => _subscription.Dispose();

    private void LoadAndCacheItem(URN id, IEnumerable<CultureInfo> cultures)
    {
        foreach (var culture in cultures)
        {
            MatchSummaryModel matchData;
            try
            {
                matchData = _apiClient.GetMatchSummary(id, culture);
            }
            catch (Exception e)
            {
                _log.LogError($"Error while fetching match summary {culture.TwoLetterISOLanguageName}: {e}");
                continue;
            }

            try
            {
                RefreshOrInsertItem(id, culture, matchData.sport_event, fromFixture: false);
            }
            catch (Exception e)
            {
                _log.LogError($"Failed to refresh or load match {culture.TwoLetterISOLanguageName}: {e}");
            }
        }
    }

    private void RefreshOrInsertItem(
        URN id,
        CultureInfo culture,
        sportEvent model,
        bool fromFixture,
        IEnumerable<info> extraInfoSource = null)
    {
        var refId = string.IsNullOrEmpty(model.refid) ? null : new URN(model.refid);
        var sportId = string.IsNullOrEmpty(model.tournament?.sport?.id)
            ? null
            : new URN(model.tournament.sport.id);
        var tournamentId = string.IsNullOrEmpty(model.tournament?.id) ? null : new URN(model.tournament.id);
        var competitors = model.competitors?.Select(c => new LocalizedMatch.Competitor
        {
            Id = new URN(c.id),
            Qualifier = c.qualifier,
        }).ToList();
        var extraInfoItems = extraInfoSource ?? model.extra_info;
        var extraInfo = ExtraInfoHelper.ToExtraInfoDictionary(extraInfoItems, id, _log);

        var sportFormat = SportFormat.Classic;
        var hasSportFormat = false;

        if (extraInfo?.TryGetValue(EXTRA_INFO_KEY_SPORT_FORMAT, out var sportFormatValue) == true)
        {
            if (sportFormatValue == SportFormat.Classic.Value)
            {
                sportFormat = SportFormat.Classic;
                hasSportFormat = true;
            } else if (sportFormatValue == SportFormat.Race.Value)
            {
                sportFormat = SportFormat.Race;
                hasSportFormat = true;
            } else
            {
                // A sport_format value a newer backend introduced that this SDK version does not
                // recognise. Do not throw: this runs inside the Rx subscription arms, which have no
                // catch, so the exception would escape OnNext and starve every cache that subscribed
                // after MatchCache. Report Unknown, but leave hasSportFormat false so the insert
                // branch surfaces it honestly while the update branch preserves a known cached value
                // rather than demoting it.
                _log.LogWarning($"Unknown sport format '{sportFormatValue}' for match '{id}', treating as Unknown.");
                sportFormat = SportFormat.Unknown;
            }
        }

        if (_cache.Get(id.ToString()) is LocalizedMatch item)
        {
            item.RefId = refId ?? item.RefId;
            if (model.scheduledSpecified)
                item.ScheduledTime = model.scheduled;
            if (model.scheduled_endSpecified)
                item.ScheduledEndTime = model.scheduled_end;
            item.SportId = sportId ?? item.SportId;
            item.TournamentId = tournamentId ?? item.TournamentId;
            item.Competitors = competitors ?? item.Competitors;
            if (!string.IsNullOrEmpty(model.liveodds))
                item.LiveOddsAvailability = model.liveodds.ParseToLiveOddsAvailability();
            if (hasSportFormat)
                item.SportFormat = sportFormat;
            if (extraInfo is not null)
            {
                // Merge rather than replace: different endpoints carry different extra_info keys, so
                // a present-but-partial payload must not erase keys it omits (the same preservation
                // rule the fields above follow). Present keys win; absent keys are preserved — which
                // also keeps ExtraInfo[sport_format] aligned with the SportFormat gate above without a
                // special case. An explicitly empty extra_info therefore preserves rather than clears,
                // a deliberate divergence from the "empty collection clears" rule for Competitors:
                // an empty bag from a subset-carrying endpoint is not a credible "server cleared all".
                if (item.ExtraInfo is null)
                {
                    item.ExtraInfo = extraInfo;
                }
                else
                {
                    // Copy-on-write: IMatch.ExtraInfo returns the live cached instance, so mutating it
                    // in place would race a consumer enumerating it on another thread and strip the
                    // snapshot semantics every sibling field keeps by swapping a reference. Build a
                    // merged dictionary and swap.
                    var merged = new Dictionary<string, string>(item.ExtraInfo);
                    foreach (var entry in extraInfo)
                        merged[entry.Key] = entry.Value;
                    item.ExtraInfo = merged;
                }
            }
        }
        else
        {
            item = new LocalizedMatch(id)
            {
                RefId = refId,
                ScheduledTime = model.scheduledSpecified ? model.scheduled : default(DateTime?),
                ScheduledEndTime = model.scheduled_endSpecified ? model.scheduled_end : default(DateTime?),
                SportId = sportId,
                TournamentId = tournamentId,
                Competitors = competitors,
                LiveOddsAvailability = model.liveodds.ParseToLiveOddsAvailability(),
                SportFormat = sportFormat,
                ExtraInfo = extraInfo,
            };
        }

        item.Name[culture] = model.name;
        item.MarkCultureLoaded(culture, fromFixture);

        _cache.Set(id.ToString(), item, _cacheTtl.AsCachePolicy());
    }

    private void HandleMatchData(
        CultureInfo culture,
        List<sportEvent> tournaments,
        bool fromFixture,
        IEnumerable<info> fixtureExtraInfo = null)
    {
        foreach (var tournament in tournaments)
        {
            // Per-item, not around the whole loop: a schedule/tournament-schedule payload carries
            // many sportEvents, and both the URN construction below and RefreshOrInsertItem can throw
            // on a malformed server value (new URN(...) rejects anything that isn't three
            // colon-separated parts with a positive numeric id). These run inside the Rx subscription
            // arms, which have no catch, so an escape would starve every later subscriber. Isolating
            // per item keeps one bad match from dropping its siblings, matching LoadAndCacheItem's
            // log-and-continue contract.
            try
            {
                var id = string.IsNullOrEmpty(tournament?.id) ? null : new URN(tournament.id);
                if (id is null)
                    continue;
                RefreshOrInsertItem(id, culture, tournament, fromFixture, fixtureExtraInfo);
            }
            catch (Exception e)
            {
                // Include the raw id: URN's exception message names only the parameter and type, not
                // the offending value, so without this the operator cannot tell which event failed.
                _log.LogError($"Failed to refresh or load match '{tournament?.id}' ({culture.TwoLetterISOLanguageName}): {e}");
            }
        }
    }
}

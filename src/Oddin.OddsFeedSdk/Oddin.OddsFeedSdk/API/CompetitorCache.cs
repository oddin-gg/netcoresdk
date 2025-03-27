using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.API.Models;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.API;

internal class CompetitorCache : ICompetitorCache
{
    private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(CompetitorCache));

    private readonly IApiClient _apiClient;
    private readonly MemoryCache _cache = new(nameof(CompetitorCache));
    private readonly TimeSpan _cacheTtl = TimeSpan.FromHours(24);
    private readonly Semaphore _semaphore = new(1, 1);
    private readonly IDisposable _subscription;

    public CompetitorCache(IApiClient apiClient)
    {
        _apiClient = apiClient;

        _subscription = apiClient.SubscribeForClass<IRequestResult<object>>()
            .Subscribe(response =>
            {
                if (response.Culture is null || response.Data is null)
                    return;

                var competitors = response.Data switch
                {
                    FixturesEndpointModel f => f.fixture.competitors.ToArray(),
                    MatchSummaryModel m => m.sport_event.competitors.ToArray(),
                    ScheduleEndpointModel s => s.sport_event.SelectMany(e => e.competitors).ToArray(),
                    TournamentScheduleModel t => t.tournament.SelectMany(e => e.competitors).ToArray(),
                    TournamentInfoModel t => t.competitors.ToArray(),
                    _ => new team[0]
                };

                if (competitors.Any())
                {
                    _semaphore.WaitOne();
                    try
                    {
                        _log.LogDebug($"Updating Competitor cache from API: {response.Data.GetType()}");
                        HandleTeamData(response.Culture, competitors);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }
            });
    }

    public LocalizedCompetitor GetCompetitor(URN id, IEnumerable<CultureInfo> cultures)
    {
        _semaphore.WaitOne();
        try
        {
            var alreadyExisting = _cache.Get(id.ToString()) as LocalizedCompetitor;
            var localesExisting = alreadyExisting?.LoadedLocals ?? new List<CultureInfo>();
            var toFetch = cultures.Except(localesExisting);

            if (toFetch.Any())
            {
                LoadAndCacheItem(id, toFetch);
            }

            return _cache.Get(id.ToString()) as LocalizedCompetitor;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public string GetCompetitorIconPath(URN id, CultureInfo culture)
    {
        var competitor = _cache.Get(id.ToString()) as LocalizedCompetitor;
        if (competitor?.IconPathLoaded == true)
            return competitor.IconPath;

        _semaphore.WaitOne();
        try
        {
            LoadAndCacheItem(id, new[] { culture });
            competitor = _cache.Get(id.ToString()) as LocalizedCompetitor;
            return competitor?.IconPath;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void ClearCacheItem(URN id) => _cache.Remove(id.ToString());

    public void Dispose() => _subscription.Dispose();

    public void LoadAndCacheItem(URN id, IEnumerable<CultureInfo> cultures)
    {
        foreach (var culture in cultures)
        {
            competitorProfileEndpoint data;
            try
            {
                data = _apiClient.GetCompetitorProfileWithPlayers(id, culture);
            }
            catch (Exception e)
            {
                _log.LogError($"Error while fetching competitor profile {e}");
                continue;
            }

            try
            {
                RefreshOrInsertItem(id, culture, data);
            }
            catch (Exception e)
            {
                _log.LogError($"Error while refreshing competitor {e}");
            }
        }
    }

    private struct Team
    {
        internal URN RefId { get; set; }
        internal string Name { get; set; }
        internal string Abbreviation { get; set; }
        internal string Country { get; set; }
        internal string CountryCode { get; set; }
        internal bool? IsVirtual { get; set; }
        internal string Underage { get; set; }
    }

    private void RefreshOrInsertItem(URN id, CultureInfo culture, ITeamable data)
    {
        var team = data switch
        {
            team t => new Team {
                RefId = string.IsNullOrEmpty(t.refid) ? null : new URN(t.refid),
                Name = t.name,
                Abbreviation = t.abbreviation,
                Country = t.country,
                CountryCode = t.country_code,
                IsVirtual = t.virtualSpecified ? t.@virtual : default(bool?),
                Underage = t.underage
            },
            competitorProfileEndpoint e => new Team {
                RefId = string.IsNullOrEmpty(e.competitor.refid) ? null : new URN(e.competitor.refid),
                Name = e.competitor.name,
                Abbreviation = e.competitor.abbreviation,
                Country = e.competitor.country,
                CountryCode = e.competitor.country_code,
                IsVirtual = e.competitor.virtualSpecified ? e.competitor.@virtual : default(bool?),
                Underage = e.competitor.underage
            },
            _ => throw new ArgumentException($"Unknown resource type: {data.GetType().Name}")
        };

        if (_cache.Get(id.ToString()) is LocalizedCompetitor item)
        {
            item.RefId = team.RefId;
            item.IsVirtual = team.IsVirtual;
            item.CountryCode = team.CountryCode;
            item.Underage = team.Underage;
        }
        else
        {
            item = new LocalizedCompetitor(id)
            {
                RefId = team.RefId,
                IsVirtual = team.IsVirtual,
                CountryCode = team.CountryCode,
                Underage = team.Underage
            };
        }

        item.Name[culture] = team.Name;

        if (team.Abbreviation != null)
            item.Abbreviation[culture] = team.Abbreviation;

        if (team.Country != null)
            item.Country[culture] = team.Country;

        if (data is competitorProfileEndpoint competitorProfileEndpoint)
        {
            var playerURNs = new List<URN>(competitorProfileEndpoint.players.Count);
            foreach (var player in competitorProfileEndpoint.players)
            {
                var playerURN = new URN(player.id);
                playerURNs.Add(playerURN);
            }
            item.PlayerIDs = playerURNs;

            item.SportIds = competitorProfileEndpoint.competitor?.sport?
                .Where(s => string.IsNullOrEmpty(s.id) == false)
                .Select(s => new URN(s.id));

            item.IconPath = competitorProfileEndpoint.competitor?.icon_path;
            item.IconPathLoaded = true;
        }

        _cache.Set(id.ToString(), item, _cacheTtl.AsCachePolicy());
    }

    private void HandleTeamData(CultureInfo culture, team[] teams)
    {
        foreach (var team in teams)
        {
            var id = string.IsNullOrEmpty(team?.id) ? null : new URN(team.id);
            RefreshOrInsertItem(id, culture, team);
        }
    }
}
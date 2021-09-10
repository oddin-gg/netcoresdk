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

namespace Oddin.OddsFeedSdk.API
{
    internal class CompetitorCache : ICompetitorCache
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(CompetitorCache));

        private readonly IApiClient _apiClient;
        private readonly MemoryCache _cache = new(nameof(CompetitorCache));
        private readonly Semaphore _semaphore = new(1, 1);
        private readonly TimeSpan _cacheTTL = TimeSpan.FromHours(24);
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

                if(toFetch.Any())
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

        private void LoadAndCacheItem(URN id, IEnumerable<CultureInfo> cultures)
        {
            foreach (var culture in cultures)
            {
                teamExtended team;
                try
                {
                    team = _apiClient.GetCompetitorProfile(id, culture);
                }
                catch (Exception e)
                {
                    _log.LogError($"Error while fetching competitor profile {e}");
                    continue;
                }

                try
                {
                    RefreshOrInsertItem(id, culture, team);
                }
                catch (Exception e)
                {
                    _log.LogError($"Error while refreshing competitor {e}");
                }
            }
        }

        private void RefreshOrInsertItem(URN id, CultureInfo culture, team data)
        {
            if (_cache.Get(id.ToString()) is LocalizedCompetitor item)
            {
                item.RefId = string.IsNullOrEmpty(data?.refid) ? null : new URN(data.refid);
                item.IsVirtual = data.virtualSpecified ? data.@virtual : default(bool?);
                item.CountryCode = data.country_code;
                item.Underage = data.underage;
            }
            else
            {
                item = new LocalizedCompetitor(id)
                {
                    RefId = string.IsNullOrEmpty(data?.refid) ? null : new URN(data.refid),
                    IsVirtual = data.virtualSpecified ? data.@virtual : default(bool?),
                    CountryCode = data.country_code,
                    Underage = data.underage
                };
            }
            item.Name[culture] = data.name;

            if (data.abbreviation != null)
                item.Abbreviation[culture] = data.abbreviation;

            if (data.country != null)
                item.Country[culture] = data.country;

            if (data is teamExtended dataExtended)
            {
                item.SportId = string.IsNullOrEmpty(dataExtended?.sport?.id) ? null : new URN(dataExtended.sport?.id);
                item.IconPath = dataExtended?.icon_path;
            }


            _cache.Set(id.ToString(), item, _cacheTTL.AsCachePolicy());
        }

        public void ClearCacheItem(URN id)
        {
            _cache.Remove(id.ToString());
        }

        private void HandleTeamData(CultureInfo culture, team[] teams)
        {
            foreach(var team in teams)
            {
                var id = string.IsNullOrEmpty(team?.id) ? null : new URN(team.id);
                RefreshOrInsertItem(id, culture, team);
            }
        }

        public void Dispose() => _subscription.Dispose();
    }
}

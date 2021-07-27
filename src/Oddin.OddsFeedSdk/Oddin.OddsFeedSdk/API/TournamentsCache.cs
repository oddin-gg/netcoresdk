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
    internal interface ITournamentsCache
    {
        LocalizedTournament GetTournament(URN id, IEnumerable<CultureInfo> cultures);

        IEnumerable<URN> GetTournamentCompetitors(URN id, CultureInfo culture);
        
        void ClearCacheItem(URN id);
    }

    internal class TournamentsCache : ITournamentsCache
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(TournamentsCache));

        private readonly IApiClient _apiClient;
        private readonly MemoryCache _cache;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public TournamentsCache(IApiClient apiClient)
        {
            _apiClient = apiClient;
            _cache = MemoryCache.Default;
        }

        // TODO: Subscribe + dispose to events from api

        public LocalizedTournament GetTournament(URN id, IEnumerable<CultureInfo> cultures)
        {
            _semaphore.Wait();
            try
            {
                var localizedTournament = _cache.Get(id.ToString()) as LocalizedTournament;
                var localizedAlready = localizedTournament?.LoadedLocals ?? new List<CultureInfo>();

                var culturesToLoad = cultures.Except(localizedAlready);
                if (culturesToLoad.Any())
                    LoadAndCacheItem(id, culturesToLoad);

                return _cache.Get(id.ToString()) as LocalizedTournament;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public IEnumerable<URN> GetTournamentCompetitors(URN id, CultureInfo culture)
        {
            _semaphore.Wait();
            try
            {
                LoadAndCacheItem(id, new[] { culture });
                var tournament = _cache.Get(id.ToString()) as LocalizedTournament;
                return tournament?.CompetitorIds;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        internal void LoadAndCacheItem(URN id, IEnumerable<CultureInfo> cultures)
        {
            foreach(var culture in cultures)
            {
                TournamentInfoModel tournamentData;
                try
                {
                    tournamentData = _apiClient.GetTournament(id, culture);
                }
                catch(Exception e)
                {
                    _log.LogError($"Error while fetching tournament {culture.TwoLetterISOLanguageName}: {e}");
                    continue;
                }
                try
                {
                    RefreshOrInsertItem(id, culture, tournamentData.tournament);
                }
                catch(Exception e)
                {
                    _log.LogError($"Failed to refresh or load tournament {culture.TwoLetterISOLanguageName}: {e}");
                }
            }
        }

        internal void RefreshOrInsertItem(URN id, CultureInfo culture, tournamentExtended model)
        {
            if (_cache.Get(id.ToString()) is LocalizedTournament item)
            {
                item.StartDate = model?.tournament_length?.start_date;
                item.EndDate = model?.tournament_length?.end_date;
                item.SportId = new URN(model.sport.id);
                item.ScheduledTime = model?.scheduled;
                item.ScheduledEndTime = model?.scheduled_end;
            }
            else
            {
                item = new LocalizedTournament(id)
                {
                    StartDate = model?.tournament_length?.start_date,
                    EndDate = model?.tournament_length?.end_date,
                    SportId = new URN(model.sport.id),
                    ScheduledTime = model?.scheduled,
                    ScheduledEndTime = model?.scheduled_end
                };
            }

            item.Name[culture] = model.name;

            if(model.competitors != null && model.competitors.Any())
            {
                var ids = model.competitors.Select(c => new URN(c.id));
                var alreadyExistingIds = item.CompetitorIds.ToHashSet();
                
                foreach(var competitorId in ids)
                    alreadyExistingIds.Add(competitorId);

                item.CompetitorIds = alreadyExistingIds.ToList();
            }

            _cache.Set(id.ToString(), item, new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromHours(12) });
        }

        public void ClearCacheItem(URN id)
        {
            _cache.Remove(id.ToString());
        }
    }
}

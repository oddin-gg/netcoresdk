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
using Oddin.OddsFeedSdk.Configuration.Abstractions;

namespace Oddin.OddsFeedSdk.API;

internal class PlayerCache : IPlayerCache
{
    private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(PlayerCache));

    private readonly IApiClient _apiClient;
    private readonly MemoryCache _cache = new(nameof(PlayerCache));
    private readonly TimeSpan _cacheTtl = TimeSpan.FromHours(12);
    private readonly IFeedConfiguration _config;
    private readonly Semaphore _semaphore = new(1, 1);

    public PlayerCache(
        IApiClient apiClient,
        IFeedConfiguration config
    )
    {
        _apiClient = apiClient;
        _config = config;
    }

    public LocalizedPlayer GetPlayer(URN id, IEnumerable<CultureInfo> cultures)
    {
        _semaphore.WaitOne();
        try
        {
            var alreadyExisting = _cache.Get(id.ToString()) as LocalizedPlayer;
            var localesExisting = alreadyExisting?.LoadedLocals ?? new List<CultureInfo>();
            var toFetch = cultures.Except(localesExisting);

            if (toFetch.Any())
            {
                LoadAndCacheItem(id, toFetch);
            }

            return _cache.Get(id.ToString()) as LocalizedPlayer;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void ClearCacheItem(URN id) => _cache.Remove(id.ToString());

    public void Dispose()
    {
    }

    private void LoadAndCacheItem(URN id, IEnumerable<CultureInfo> cultures)
    {
        foreach (var culture in cultures)
        {
            player_profilePlayer player;
            try
            {
                player = _apiClient.GetPlayerProfile(id, culture);
            }
            catch (Exception e)
            {
                _log.LogError("Error while fetching player profile {E}", e);
                e.HandleAccordingToStrategy(GetType().Name, _log, _config.ExceptionHandlingStrategy);
                continue;
            }

            try
            {
                RefreshOrInsertItem(id, culture, player);
            }
            catch (Exception e)
            {
                _log.LogError("Error while refreshing player {E}", e);
                e.HandleAccordingToStrategy(GetType().Name, _log, _config.ExceptionHandlingStrategy);
            }
        }
    }

    private void RefreshOrInsertItem(URN id, CultureInfo culture, player_profilePlayer data)
    {
        if (_cache.Get(id.ToString()) is LocalizedPlayer item)
        {
            item.Name[culture] = data.name;
            item.FullName[culture] = data.full_name;
        }
        else
        {
            item = new LocalizedPlayer(id)
            {
                Name =
                {
                    [culture] = data.name
                },
                FullName =
                {
                    [culture] = data.full_name
                }
            };
        }

        _cache.Set(id.ToString(), item, _cacheTtl.AsCachePolicy());
    }

    private void HandlePlayersData(CultureInfo culture, player_profilePlayer[] players)
    {
        foreach (var player in players)
        {
            var id = string.IsNullOrEmpty(player?.id) ? null : new URN(player.id);
            RefreshOrInsertItem(id, culture, player);
        }
    }
}
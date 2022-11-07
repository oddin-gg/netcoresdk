using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Timers;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.API.Models;
using Oddin.OddsFeedSdk.Configuration.Abstractions;

namespace Oddin.OddsFeedSdk.API;

internal class LocalizedStaticDataOfMatchStatusCache : ILocalizedStaticDataCache, IDisposable
{
    private readonly IApiClient _apiClient;

    private readonly IEnumerable<CultureInfo> _locales;
    private readonly object _lock = new();

    private readonly IDictionary<long, IDictionary<CultureInfo, string>> _pseudoCache
        = new Dictionary<long, IDictionary<CultureInfo, string>>();

    private readonly Timer _timer;

    public LocalizedStaticDataOfMatchStatusCache(IFeedConfiguration feedConfiguration, IApiClient apiClient)
    {
        _apiClient = apiClient;
        _locales = new[] { feedConfiguration.DefaultLocale };
        _timer = new Timer
        {
            Interval = TimeSpan.FromHours(24).TotalMilliseconds,
            AutoReset = true
        };
        _timer.Elapsed += OnRefresh;
        _timer.Start();
    }

    public void Dispose()
        => _timer.Dispose();

    public ILocalizedNamedValue Get(long id, IEnumerable<CultureInfo> cultures)
    {
        lock (_lock)
        {
            var missingLocales = cultures.Except(GetFetchedLocals());

            if (missingLocales.Any())
                FetchData(missingLocales);

            if (_pseudoCache.TryGetValue(id, out var item))
                return new LocalizedNamedValue(id, item);
            return null;
        }
    }

    public ILocalizedNamedValue Get(long id) => Get(id, _locales);

    public bool Exists(long id)
    {
        lock (_lock)
        {
            if (GetFetchedLocals().Any() == false)
                FetchData(_locales);

            return _pseudoCache.TryGetValue(id, out _);
        }
    }

    private IEnumerable<CultureInfo> GetFetchedLocals()
        => _pseudoCache.Values.SelectMany(v => v.Keys).ToHashSet();

    private void OnRefresh(object sender, ElapsedEventArgs e)
    {
        lock (_lock)
        {
            FetchData(GetFetchedLocals());
        }
    }

    private bool FetchData(IEnumerable<CultureInfo> cultures)
    {
        foreach (var culture in cultures)
        {
            MatchStatusModel statusModel;
            try
            {
                statusModel = _apiClient.GetMatchStatusDescriptions(culture);
            }
            catch (Exception)
            {
                return false;
            }

            foreach (var status in statusModel.match_status)
            {
                if (_pseudoCache.TryGetValue(status.id, out var statusDescriptionCache))
                {
                    statusDescriptionCache[culture] = status.description;
                }
                else
                {
                    _pseudoCache[status.id] = new Dictionary<CultureInfo, string> { { culture, status.description } };
                }
            }
        }

        return true;
    }
}
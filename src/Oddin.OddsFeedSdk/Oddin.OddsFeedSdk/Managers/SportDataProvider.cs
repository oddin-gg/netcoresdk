using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Exceptions;
using Oddin.OddsFeedSdk.Managers.Abstractions;

namespace Oddin.OddsFeedSdk.Managers;

internal class SportDataProvider : ISportDataProvider
{
    private readonly IApiClient _apiClient;
    private readonly ISportDataBuilder _builder;
    private readonly ICacheManager _cacheManager;
    private readonly IExceptionWrapper _exceptionWrapper;
    private readonly IFeedConfiguration _feedConfiguration;

    public SportDataProvider(
        ICacheManager cacheManager,
        IFeedConfiguration feedConfiguration,
        ISportDataBuilder builder,
        IExceptionWrapper exceptionWrapper,
        IApiClient apiClient)
    {
        _cacheManager = cacheManager;
        _feedConfiguration = feedConfiguration;
        _builder = builder;
        _exceptionWrapper = exceptionWrapper;
        _apiClient = apiClient;
    }

    public async Task<IEnumerable<ISport>> GetSportsAsync(CultureInfo culture = null)
    {
        culture ??= _feedConfiguration.DefaultLocale;

        return await _exceptionWrapper.Wrap(async () => await _builder.BuildSports(new[] { culture }));
    }

    public async Task<ISport> GetSportAsync(URN id, CultureInfo culture = null)
    {
        var sports = await GetSportsAsync(culture);
        return sports.FirstOrDefault(s => s.Id == id);
    }

    public IEnumerable<ITournament> GetActiveTournaments(CultureInfo culture = null)
    {
        culture ??= _feedConfiguration.DefaultLocale;

        var sports = GetSportsAsync(culture).ConfigureAwait(false).GetAwaiter().GetResult();
        var sportTournaments = sports.SelectMany(s => s.Tournaments);

        return new HashSet<ITournament>(sportTournaments, new TournamentIdComparer());
    }

    public IEnumerable<ITournament> GetActiveTournaments(string name, CultureInfo culture = null)
    {
        culture ??= _feedConfiguration.DefaultLocale;

        var sports = GetSportsAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        var sport = sports?.FirstOrDefault(s => s.GetName(culture).Equals(name));
        return sport?.Tournaments ?? new List<ITournament>();
    }

    public IEnumerable<ITournament> GetAvailableTournaments(URN sportId, CultureInfo culture = null)
    {
        culture ??= _feedConfiguration.DefaultLocale;

        var result = _exceptionWrapper.Wrap(()
            => _apiClient.GetTournaments(sportId, culture));

        return result.tournaments.Select(
            t => _builder.BuildTournament(string.IsNullOrEmpty(t?.id) ? null : new URN(t.id), sportId,
                new[] { culture }));
    }

    public void DeleteTournamentFromCache(URN id) => _cacheManager.TournamentsCache.ClearCacheItem(id);

    public void DeleteMatchFromCache(URN id)
    {
        _cacheManager.MatchCache.ClearCacheItem(id);
        _cacheManager.FixtureCache.ClearCacheItem(id);
    }

    public void DeleteCompetitorFromCache(URN id) => _cacheManager.CompetitorCache.ClearCacheItem(id);

    public ICompetitor GetCompetitor(URN id, CultureInfo culture = null)
    {
        culture ??= _feedConfiguration.DefaultLocale;

        return _exceptionWrapper.Wrap(()
            => _builder.BuildCompetitor(id, new[] { culture }));
    }

    public void DeletePlayerFromCache(URN id) => _cacheManager.PlayerCache.ClearCacheItem(id);

    public IPlayer GetPlayer(URN id, CultureInfo culture = null)
    {
        culture ??= _feedConfiguration.DefaultLocale;

        return _exceptionWrapper.Wrap(()
            => _builder.BuildPlayer(id, new[] { culture }));
    }

    public IMatch GetMatch(URN id, CultureInfo culture = null)
    {
        culture ??= _feedConfiguration.DefaultLocale;

        return _exceptionWrapper.Wrap(()
            => _builder.BuildMatch(id, new[] { culture }));
    }

    public IEnumerable<IMatch> GetMatchesFor(DateTime dateTime, CultureInfo culture = null)
    {
        culture ??= _feedConfiguration.DefaultLocale;

        return _exceptionWrapper.Wrap(() =>
        {
            var result = _apiClient.GetMatches(dateTime, culture);
            return result.sport_event.Select(s
                => _builder.BuildMatch(string.IsNullOrEmpty(s?.id) ? null : new URN(s.id), new[] { culture }));
        });
    }

    public IEnumerable<IMatch> GetLiveMatches(CultureInfo culture = null)
    {
        culture ??= _feedConfiguration.DefaultLocale;

        return _exceptionWrapper.Wrap(() =>
        {
            var result = _apiClient.GetLiveMatches(culture);
            return result.sport_event.Select(s
                => _builder.BuildMatch(string.IsNullOrEmpty(s?.id) ? null : new URN(s.id), new[] { culture }));
        });
    }

    public IEnumerable<IMatch> GetListOfMatches(int startIndex, int limit, CultureInfo culture = null)
    {
        if (startIndex < 0)
            throw new ArgumentException("Requires startIndex >= 0", nameof(startIndex));

        if (limit is > 1000 or < 1)
            throw new ArgumentException("Requires limit <= 1000 && limit >= 1", nameof(limit));

        culture ??= _feedConfiguration.DefaultLocale;

        return _exceptionWrapper.Wrap(() =>
        {
            var result = _apiClient.GetSchedule(startIndex, limit, culture);

            var ids = result.sport_event.Select(s => string.IsNullOrEmpty(s?.id) ? null : new URN(s.id));
            return _builder.BuildMatches(ids, new[] { culture });
        });
    }

    public IEnumerable<IFixtureChange> GetFixtureChanges(CultureInfo culture = null)
    {
        culture ??= _feedConfiguration.DefaultLocale;

        return _exceptionWrapper.Wrap(() =>
        {
            var result = _apiClient.GetFixtureChanges(culture);

            return result.fixture_change.Select(f =>
                new FixtureChange(string.IsNullOrEmpty(f?.sport_event_id) ? null : new URN(f.sport_event_id),
                    f.update_time));
        });
    }
}
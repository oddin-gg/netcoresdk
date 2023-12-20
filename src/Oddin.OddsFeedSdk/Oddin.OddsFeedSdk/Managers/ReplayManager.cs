using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Managers.Abstractions;

namespace Oddin.OddsFeedSdk.Managers;

internal class ReplayManager : IReplayManager
{
    private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(ReplayManager));
    private readonly IApiClient _apiClient;

    private readonly IFeedConfiguration _config;
    private readonly ISportDataProvider _sportsDataProvider;

    public ReplayManager(IFeedConfiguration config, IApiClient apiClient,
        ISportDataProvider sportsDataProvider)
    {
        _config = config;
        _apiClient = apiClient;
        _sportsDataProvider = sportsDataProvider;
    }

    public async Task<IEnumerable<URN>> GetEventsInQueue()
    {
        try
        {
            var data = await _apiClient.GetReplaySetContent(_config.NodeId);

            return data?.replay_event?
                       .Where(e => e.id != null)
                       .Select(e => string.IsNullOrEmpty(e.id) ? null : new URN(e.id))
                   ?? Enumerable.Empty<URN>();
        }
        catch (Exception e)
        {
            _log.LogError("Failed to fetch replay events with error: {E}", e);
            e.HandleAccordingToStrategy(GetType().Name, _log, _config.ExceptionHandlingStrategy);
            return null;
        }
    }

    public async Task<IEnumerable<ISportEvent>> GetReplayList()
    {
        try
        {
            var data = await _apiClient.GetReplaySetContent(_config.NodeId);

            return data?.replay_event?
                       .Where(e => e.id != null)
                       .Select(e => _sportsDataProvider.GetMatch(string.IsNullOrEmpty(e.id) ? null : new URN(e.id)))
                   ?? Enumerable.Empty<ISportEvent>();
        }
        catch (Exception e)
        {
            _log.LogError("Failed to fetch replay events with error: {E}", e);
            e.HandleAccordingToStrategy(GetType().Name, _log, _config.ExceptionHandlingStrategy);
            return null;
        }
    }

    public async Task<bool> AddMessagesToReplayQueue(ISportEvent sportEvent)
    {
        if (sportEvent?.Id is null)
            return false;

        return await AddMessagesToReplayQueue(sportEvent.Id);
    }

    public async Task<bool> AddMessagesToReplayQueue(URN eventId)
    {
        try
        {
            return await _apiClient.PutReplayEvent(eventId, _config.NodeId);
        }
        catch (Exception e)
        {
            _log.LogError("Failed to add replay events with error: {E}", e);
            e.HandleAccordingToStrategy(GetType().Name, _log, _config.ExceptionHandlingStrategy);
            return false;
        }
    }

    public async Task<bool> RemoveEventFromReplayQueue(ISportEvent sportEvent)
    {
        if (sportEvent?.Id is null)
            return false;

        return await RemoveEventFromReplayQueue(sportEvent.Id);
    }

    public async Task<bool> RemoveEventFromReplayQueue(URN eventId)
    {
        try
        {
            return await _apiClient.DeleteReplayEvent(eventId, _config.NodeId);
        }
        catch (Exception e)
        {
            _log.LogError("Failed to remove event id {EventId} with error: {E}", eventId, e);
            e.HandleAccordingToStrategy(GetType().Name, _log, _config.ExceptionHandlingStrategy);
            return false;
        }
    }

    public async Task<bool> StartReplay(int? speed = null, int? maxDelay = null, bool? useReplayTimestamp = null,
        string product = null, bool? runParallel = null)
    {
        try
        {
            return await _apiClient.PostReplayStart(
                _config.NodeId,
                speed,
                maxDelay,
                useReplayTimestamp,
                product,
                runParallel);
        }
        catch (Exception e)
        {
            _log.LogError("Failed play replay with error: {E}", e);
            e.HandleAccordingToStrategy(GetType().Name, _log, _config.ExceptionHandlingStrategy);
            return false;
        }
    }

    public async Task<bool> StopReplay()
    {
        try
        {
            return await _apiClient.PostReplayStop(_config.NodeId);
        }
        catch (Exception e)
        {
            _log.LogError("Failed to clear replay with: {E}", e);
            e.HandleAccordingToStrategy(GetType().Name, _log, _config.ExceptionHandlingStrategy);
            return false;
        }
    }

    public async Task<bool> StopAndClearReplay()
    {
        try
        {
            return await _apiClient.PostReplayClear(_config.NodeId);
        }
        catch (Exception e)
        {
            _log.LogError("Failed to clear replay with: {E}", e);
            e.HandleAccordingToStrategy(GetType().Name, _log, _config.ExceptionHandlingStrategy);
            return false;
        }
    }

    public async Task<string> GetStatusOfReplay()
    {
        try
        {
            var result = await _apiClient.GetStatusOfReplay();
            return result.status;
        }
        catch (Exception e)
        {
            _log.LogError("Failed to get replay status: {E}", e);
            e.HandleAccordingToStrategy(GetType().Name, _log, _config.ExceptionHandlingStrategy);
            return null;
        }
    }
}
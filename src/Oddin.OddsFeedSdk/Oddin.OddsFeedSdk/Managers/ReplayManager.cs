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

namespace Oddin.OddsFeedSdk.Managers
{
    internal class ReplayManager : IReplayManager
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(ReplayManager));

        private readonly IFeedConfiguration _feedConfiguration;
        private readonly IApiClient _apiClient;
        private readonly ISportDataProvider _sportsDataProvider;

        public ReplayManager(IFeedConfiguration feedConfiguration, IApiClient apiClient, ISportDataProvider sportsDataProvider)
        {
            _feedConfiguration = feedConfiguration;
            _apiClient = apiClient;
            _sportsDataProvider = sportsDataProvider;
        }

        public async Task<IEnumerable<URN>> GetEventsInQueue()
        {
            try
            {
                var data = await _apiClient.GetReplaySetContent(_feedConfiguration.NodeId);

                return data?.replay_event?
                    .Where(e => e.id != null)
                    .Select(e => new URN(e.id))
                    ?? Enumerable.Empty<URN>();
            }
            catch (Exception e)
            {
                _log.LogError($"Failed to fetch replay events with error: {e}");
                return null;
            }
        }

        public async Task<IEnumerable<ISportEvent>> GetReplayList()
        {
            try
            {
                var data = await _apiClient.GetReplaySetContent(_feedConfiguration.NodeId);

                return data?.replay_event?
                    .Where(e => e.id != null)
                    .Select(e => _sportsDataProvider.GetMatch(new URN(e.id)))
                    ?? Enumerable.Empty<ISportEvent>();
            }
            catch(Exception e)
            {
                _log.LogError($"Failed to fetch replay events with error: {e}");
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
                return await _apiClient.PutReplayEvent(eventId, _feedConfiguration.NodeId);
            }
            catch (Exception e)
            {
                _log.LogError($"Failed to add replay events with error: {e}");
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
                return await _apiClient.DeleteReplayEvent(eventId, _feedConfiguration.NodeId);
            }
            catch (Exception e)
            {
                _log.LogError($"Failed to remove event id {eventId} with error: { e}");
                return false;
            }
        }

        public async Task<bool> StartReplay(int? speed = null, int? maxDelay = null, bool? useReplayTimestamp = null, string product = null, bool? runParallel = null)
        {
            try
            {
                return await _apiClient.PostReplayStart(
                    _feedConfiguration.NodeId,
                    speed,
                    maxDelay,
                    useReplayTimestamp,
                    product,
                    runParallel);
            }
            catch (Exception e)
            {
                _log.LogError($"Failed play replay with errror: { e}");
                return false;
            }
        }

        public async Task<bool> StopReplay()
        {
            try
            {
                return await _apiClient.PostReplayStop(_feedConfiguration.NodeId);
            }
            catch (Exception e)
            {
                _log.LogError($"Failed to clear replay with: {e}");
                return false;
            }
        }

        public async Task<bool> StopAndClearReplay()
        {
            try
            {
                return await _apiClient.PostReplayClear(_feedConfiguration.NodeId);
            }
            catch (Exception e)
            {
                _log.LogError($"Failed to clear replay with: {e}");
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
                _log.LogError($"Failed to get replay status: {e}");
                return null;
            }
        }
    }
}

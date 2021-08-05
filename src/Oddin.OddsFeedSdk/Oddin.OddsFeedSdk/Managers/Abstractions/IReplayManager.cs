using System.Collections.Generic;
using System.Threading.Tasks;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.Managers.Abstractions
{
    public interface IReplayManager
    {
        Task<IEnumerable<ISportEvent>> GetReplayList();
        Task<IEnumerable<URN>> GetEventsInQueue();

        Task<bool> AddMessagesToReplayQueue(ISportEvent sportEvent);
        Task<bool> AddMessagesToReplayQueue(URN eventId);

        Task<bool> RemoveEventFromReplayQueue(ISportEvent sportEvent);
        Task<bool> RemoveEventFromReplayQueue(URN eventId);

        Task<bool> StartReplay(
            int? speed = null,
            int? maxDelay = null,
            bool? useReplayTimestamp = null,
            string product = null,
            bool? runParallel = null);

        Task<bool> StopReplay();

        Task<bool> StopAndClearReplay();

        Task<string> GetStatusOfReplay();
    }
}

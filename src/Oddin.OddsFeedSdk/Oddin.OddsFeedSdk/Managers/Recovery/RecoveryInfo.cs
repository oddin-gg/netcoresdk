using System.Net;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.Managers.Recovery
{
    public class RecoveryInfo: IRecoveryInfo
    {
        public long After { get; }
        public long Timestamp { get; }
        public long RequestId { get; }
        public bool Successful { get; }
        public int? NodeId { get; }

        public RecoveryInfo(long after, long timestamp, long requestId, int? nodeId, bool successful)
        {
            After = after;
            Timestamp = timestamp;
            Timestamp = timestamp;
            RequestId = requestId;
            Successful = successful;
            NodeId = nodeId;
        }
    }
}
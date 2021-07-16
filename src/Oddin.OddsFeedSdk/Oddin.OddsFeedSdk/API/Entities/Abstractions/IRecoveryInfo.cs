namespace Oddin.OddsFeedSdk.API.Entities.Abstractions
{
    public interface IRecoveryInfo
    {
        public long After { get; }

        public long Timestamp { get; }

        public long RequestId { get; }

        public int ResponseCode { get; }

        public string ResponseMessage { get; }

        public int NodeId { get; }
    }
}

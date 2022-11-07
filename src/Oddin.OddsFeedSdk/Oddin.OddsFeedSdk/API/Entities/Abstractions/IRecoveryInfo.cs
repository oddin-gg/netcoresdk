namespace Oddin.OddsFeedSdk.API.Entities.Abstractions;

public interface IRecoveryInfo
{
    public long After { get; }

    public long Timestamp { get; }

    public long RequestId { get; }

    public bool Successful { get; }

    public int? NodeId { get; }
}
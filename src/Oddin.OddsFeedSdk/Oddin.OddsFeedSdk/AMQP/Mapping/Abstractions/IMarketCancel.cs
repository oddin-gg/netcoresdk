namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions
{
    public interface IMarketCancel : IMarket
    {
        int? VoidReason { get; }
        int? VoidReasonId { get; }
        string? VoidReasonParams { get; }
    }
}
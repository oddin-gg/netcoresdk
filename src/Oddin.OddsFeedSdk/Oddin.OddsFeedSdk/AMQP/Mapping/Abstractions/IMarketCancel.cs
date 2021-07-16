namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions
{
    public interface IMarketCancel : IMarket
    {
        int? VoidReason { get; }
    }
}
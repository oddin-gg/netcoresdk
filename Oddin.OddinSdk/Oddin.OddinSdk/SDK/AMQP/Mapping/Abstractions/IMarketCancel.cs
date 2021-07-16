namespace Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions
{
    public interface IMarketCancel : IMarket
    {
        int? VoidReason { get; }
    }
}
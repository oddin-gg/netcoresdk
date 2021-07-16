using Oddin.OddinSdk.SDK.API.Entities.Abstractions;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions
{
    public interface IBetCancel<out T> : IMarketMessage<IMarketCancel, T> 
        where T : ISportEvent
    {
        long? StartTime { get; }

        long? EndTime { get; }

        string SupersededBy { get; }
    }
}

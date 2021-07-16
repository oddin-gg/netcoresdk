using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using System.Collections.Generic;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions
{
    public interface IMarketMessage<out T, out T1> : IEventMessage<T1> 
        where T : IMarket where T1 : ISportEvent
    {
        IEnumerable<T> Markets { get; }
    }
}

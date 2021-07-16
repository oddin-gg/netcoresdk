using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using System.Collections.Generic;

namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions
{
    public interface IBetStop<out T> : IEventMessage<T> 
        where T : ISportEvent
    {
        MarketStatus MarketStatus { get; }

        IEnumerable<string> Groups { get; }
    }
}

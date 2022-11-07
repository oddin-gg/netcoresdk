using System.Collections.Generic;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;

public interface IMarketMessage<out T, out T1> : IEventMessage<T1>
    where T : IMarket where T1 : ISportEvent
{
    IEnumerable<T> Markets { get; }
}
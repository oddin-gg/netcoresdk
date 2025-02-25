using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping;

internal class RollbackBetCancel<T> : MarketMessage<IMarket, T>, IRollbackBetCancel<T> where T : ISportEvent
{
    public RollbackBetCancel(
        IMessageTimestamp timestamp,
        IProducer producer,
        T @event,
        long? requestId,
        IEnumerable<IMarket> markets,
        long? startTime,
        long? endTime,
        byte[] rawMessage
    )
        : base(producer, timestamp, @event, requestId, rawMessage, markets)
    {
        Markets = markets is null ? null : new ReadOnlyCollection<IMarket>(markets.ToList());
        StartTime = startTime;
        EndTime = endTime;
    }

    public IEnumerable<IMarket> Markets { get; }

    public long? StartTime { get; }
    public long? EndTime { get; }
}
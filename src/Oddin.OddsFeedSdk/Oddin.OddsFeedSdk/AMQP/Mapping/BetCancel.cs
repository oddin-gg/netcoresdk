using System.Collections.Generic;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.AMQP.Mapping;

internal class BetCancel<T> : MarketMessage<IMarketCancel, T>, IBetCancel<T> where T : ISportEvent
{
    public BetCancel(IMessageTimestamp timestamp, IProducer producer, T @event, long? requestId, long? startTime,
        long? endTime, string supersededBy, IEnumerable<IMarketCancel> markets, byte[] rawMessage)
        : base(producer, timestamp, @event, requestId, rawMessage, markets)
    {
        StartTime = startTime;
        EndTime = endTime;
        SupersededBy = string.IsNullOrEmpty(supersededBy) ? null : new URN(supersededBy);
    }

    public long? StartTime { get; }

    public long? EndTime { get; }

    public URN SupersededBy { get; }
}
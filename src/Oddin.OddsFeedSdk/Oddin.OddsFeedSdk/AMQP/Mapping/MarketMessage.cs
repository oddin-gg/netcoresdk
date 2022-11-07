using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping;

internal abstract class MarketMessage<T, T1> : EventMessage<T1>, IMarketMessage<T, T1>
    where T : IMarket
    where T1 : ISportEvent
{
    private readonly IReadOnlyCollection<T> _markets;

    public MarketMessage(IProducer producer, IMessageTimestamp messageTimestamp, T1 sportEvent, long? requestId,
        byte[] rawMessage, IEnumerable<T> markets)
        : base(producer, messageTimestamp, sportEvent, requestId, rawMessage) =>
        _markets = markets is null ? null : new ReadOnlyCollection<T>(markets.ToList());

    public IEnumerable<T> Markets => _markets;
}
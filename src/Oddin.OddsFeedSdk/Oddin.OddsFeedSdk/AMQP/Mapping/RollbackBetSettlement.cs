using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping;

internal class RollbackBetSettlement<T> : MarketMessage<IMarket, T>, IRollbackBetSettlement<T> where T : ISportEvent
{
    public RollbackBetSettlement(
        IMessageTimestamp timestamp,
        IProducer producer,
        T @event,
        long? requestId,
        IEnumerable<IMarket> markets,
        byte[] rawMessage)
        : base(producer, timestamp, @event, requestId, rawMessage, markets) =>
        Markets = markets is null ? null : new ReadOnlyCollection<IMarket>(markets.ToList());

    public IEnumerable<IMarket> Markets { get; }
}
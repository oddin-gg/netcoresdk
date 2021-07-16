using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using System.Collections.Generic;

namespace Oddin.OddsFeedSdk.AMQP.Mapping
{
    internal class OddsChange<T> : MarketMessage<IMarketWithOdds, T>, IOddsChange<T>
        where T : ISportEvent
    {
        public int? BetStopReason { get; }
        public int? BettingStatus { get; }

        public OddsChange(
            IProducer producer,
            IMessageTimestamp messageTimestamp,
            T sportEvent,
            long? requestId,
            byte[] rawMessage,
            IEnumerable<IMarketWithOdds> markets,
            int? betStopReason,
            int? bettingStatus)
            : base(producer, messageTimestamp, sportEvent, requestId, rawMessage, markets)
        {
            BettingStatus = bettingStatus;
            BetStopReason = betStopReason;
        }
    }
}

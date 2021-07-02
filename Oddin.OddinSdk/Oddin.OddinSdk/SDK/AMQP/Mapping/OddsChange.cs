using Oddin.OddinSdk.SDK.AMQP.Abstractions;
using Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using System;
using System.Collections.Generic;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping
{
    internal class OddsChange<T> : MarketMessage<IMarketWithOdds, T>, IOddsChange<T>
        where T : ISportEvent
    {
        private readonly int? _bettingStatus;
        private readonly int? _betStopReason;

        public INamedValue BetStopReason => throw new NotImplementedException();
        public INamedValue BettingStatus => throw new NotImplementedException();

        public OddsChange(IProducer producer, IMessageTimestamp messageTimestamp, T sportEvent, long? requestId, byte[] rawMessage, IEnumerable<IMarketWithOdds> markets, int? betStopReason, int? bettingStatus)
            : base(producer, messageTimestamp, sportEvent, requestId, rawMessage, markets)
        {
            _bettingStatus = bettingStatus;
            _betStopReason = betStopReason;
        }
    }
}

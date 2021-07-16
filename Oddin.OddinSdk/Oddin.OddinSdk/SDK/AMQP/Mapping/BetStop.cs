using Oddin.OddinSdk.SDK.AMQP.Enums;
using Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using System.Collections.Generic;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping
{
    internal class BetStop<T> : EventMessage<T>, IBetStop<T> where T : ISportEvent
    {
        public MarketStatus MarketStatus { get; }

        public IEnumerable<string> Groups { get; }

        public BetStop(MarketStatus marketStatus, IEnumerable<string> groups, IProducer producer, IMessageTimestamp messageTimestamp, T sportEvent, long? requestId, byte[] rawMessage)
            : base(producer, messageTimestamp, sportEvent, requestId, rawMessage)
        {
            MarketStatus = marketStatus;
            Groups = groups;
        }
    }
}

using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using System.Collections.Generic;

namespace Oddin.OddsFeedSdk.AMQP.Mapping
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

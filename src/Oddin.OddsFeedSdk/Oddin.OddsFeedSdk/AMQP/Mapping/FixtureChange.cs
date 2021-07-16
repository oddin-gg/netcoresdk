using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping
{
    internal class FixtureChange<T> : EventMessage<T>, IFixtureChange<T> 
        where T : ISportEvent
    {
        public FixtureChangeType? ChangeType { get; }

        public long? NextLiveTime { get; }

        public long StartTime { get; }

        public FixtureChange(
            IMessageTimestamp timestamp,
            IProducer producer,
            T @event,
            long? requestId,
            int? changeType,
            long? nextLiveTime,
            long startTime,
            byte[] rawMessage)
            : base(producer, timestamp, @event, requestId, rawMessage)
        {
            ChangeType = changeType switch 
            {
                1 => FixtureChangeType.NEW,
                2 => FixtureChangeType.DATE_TIME,
                3 => FixtureChangeType.CANCELLED,
                4 => FixtureChangeType.FORMAT,
                5 => FixtureChangeType.COVERAGE,
                106 => FixtureChangeType.STREAM_URL,
                _ => null
            };
            NextLiveTime = nextLiveTime;
            StartTime = startTime;
        }
    }
}

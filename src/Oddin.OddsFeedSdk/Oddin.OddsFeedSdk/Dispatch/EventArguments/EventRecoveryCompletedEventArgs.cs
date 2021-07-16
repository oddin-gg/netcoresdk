using Oddin.OddsFeedSdk.AMQP;
using System;

namespace Oddin.OddsFeedSdk.Dispatch.EventArguments
{
    public class EventRecoveryCompletedEventArgs
    {
        private readonly long _requestId;
        private readonly URN _eventId;

        internal EventRecoveryCompletedEventArgs(long requestId, URN eventId)
        {
            if (eventId is null)
                throw new ArgumentNullException(nameof(eventId));

            _requestId = requestId;
            _eventId = eventId;
        }

        public long GetRequestId()
        {
            return _requestId;
        }

        public URN GetEventId()
        {
            return _eventId;
        }
    }
}

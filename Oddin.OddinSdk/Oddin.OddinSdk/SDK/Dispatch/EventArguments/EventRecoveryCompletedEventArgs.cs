using Oddin.OddinSdk.SDK.AMQP;
using System;

namespace Oddin.OddinSdk.SDK.Dispatch.EventArguments
{
    /// <summary>
    /// Event arguments for the EventRecoveryCompleted events
    /// </summary>
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

        /// <summary>
        /// Gets the identifier of the recovery request
        /// </summary>
        /// <returns>Returns the identifier of the recovery request</returns>
        public long GetRequestId()
        {
            return _requestId;
        }

        /// <summary>
        /// Gets the associated event identifier
        /// </summary>
        /// <returns>Returns the associated event identifier</returns>
        public URN GetEventId()
        {
            return _eventId;
        }
    }
}

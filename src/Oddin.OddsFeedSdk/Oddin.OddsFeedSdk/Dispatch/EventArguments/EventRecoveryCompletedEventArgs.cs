using System;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.Dispatch.EventArguments;

public class EventRecoveryCompletedEventArgs
{
    private readonly URN _eventId;
    private readonly long _requestId;

    internal EventRecoveryCompletedEventArgs(long requestId, URN eventId)
    {
        if (eventId is null)
            throw new ArgumentNullException(nameof(eventId));

        _requestId = requestId;
        _eventId = eventId;
    }

    public long GetRequestId() => _requestId;

    public URN GetEventId() => _eventId;
}
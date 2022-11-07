using System;
using Oddin.OddsFeedSdk.Abstractions;
using Oddin.OddsFeedSdk.AMQP.EventArguments;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Dispatch.Abstractions;

namespace Oddin.OddsFeedSdk.Sessions.Abstractions;

public interface IOddsFeedSession : IEntityDispatcher<ISportEvent>
{
    public IOddsFeed Feed { get; }

    string Name { get; }

    event EventHandler<RawMessageEventArgs> OnRawFeedMessageReceived;
    event EventHandler<UnparsableMessageEventArgs> OnUnparsableMessageReceived;
}
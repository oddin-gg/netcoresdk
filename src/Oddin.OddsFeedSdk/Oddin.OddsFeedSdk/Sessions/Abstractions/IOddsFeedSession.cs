using Oddin.OddsFeedSdk.AMQP.EventArguments;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Dispatch.Abstractions;
using System;

namespace Oddin.OddsFeedSdk.Sessions.Abstractions
{
    public interface IOddsFeedSession : IEntityDispatcher<ISportEvent>
    { 
        string Name { get; }

        event EventHandler<UnparsableMessageEventArgs> OnUnparsableMessageReceived;
    }
}

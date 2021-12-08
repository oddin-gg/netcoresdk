using Oddin.OddsFeedSdk.AMQP.EventArguments;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Dispatch.Abstractions;
using System;
using Oddin.OddsFeedSdk.Abstractions;

namespace Oddin.OddsFeedSdk.Sessions.Abstractions
{
    public interface IOddsFeedSession : IEntityDispatcher<ISportEvent>
    {
        public IOddsFeed Feed { get; }

        string Name { get; }

        event EventHandler<UnparsableMessageEventArgs> OnUnparsableMessageReceived;
    }
}

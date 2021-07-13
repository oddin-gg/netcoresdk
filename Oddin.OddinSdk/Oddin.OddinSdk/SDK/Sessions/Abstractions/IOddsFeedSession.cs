using Oddin.OddinSdk.SDK.AMQP.EventArguments;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.Dispatch.Abstractions;
using System;

namespace Oddin.OddinSdk.SDK.Sessions.Abstractions
{
    public interface IOddsFeedSession : IEntityDispatcher<ISportEvent>
    {
        /// <summary>
        /// Gets the name of the session
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Raised when a message which cannot be parsed is received
        /// </summary>
        event EventHandler<UnparsableMessageEventArgs> OnUnparsableMessageReceived;
    }
}

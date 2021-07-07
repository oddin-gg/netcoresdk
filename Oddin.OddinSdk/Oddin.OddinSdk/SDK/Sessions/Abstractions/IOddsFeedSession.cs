using Oddin.OddinSdk.SDK.AMQP.EventArguments;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.Dispatch;
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

        /// <summary>
        /// Opens the session by opening its underlying connections
        /// </summary>
        void Open();

        /// <summary>
        /// Closes the session by closing its underlying connections
        /// </summary>
        void Close();
    }
}

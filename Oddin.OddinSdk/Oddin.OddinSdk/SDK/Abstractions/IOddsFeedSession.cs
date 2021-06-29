using Oddin.OddinSdk.SDK.AMQP.EventArguments;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.Dispatch;
using System;

namespace Oddin.OddinSdk.SDK.Abstractions
{
    /// <summary>
    /// Represents a session to the odds feed
    /// </summary>
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

        // TODO: implement or remove

        ///// <summary>
        ///// Constructs and returns a sport-specific <see cref="ISpecificEntityDispatcher{T}"/> instance allowing
        ///// processing of messages containing entity specific information
        ///// </summary>
        ///// <typeparam name="T">A <see cref="ICompetition"/> derived type specifying the entities associated with the created <see cref="IEntityDispatcher{T}"/> instance</typeparam>
        ///// <returns>The constructed <see cref="ISpecificEntityDispatcher{T}"/></returns>
        //ISpecificEntityDispatcher<T> CreateSportSpecificMessageDispatcher<T>() where T : ISportEvent;
    }
}

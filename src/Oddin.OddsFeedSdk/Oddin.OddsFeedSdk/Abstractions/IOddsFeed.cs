using Oddin.OddsFeedSdk.Dispatch.EventArguments;
using Oddin.OddsFeedSdk.Managers.Abstractions;
using Oddin.OddsFeedSdk.Sessions.Abstractions;
using System;

namespace Oddin.OddsFeedSdk.Abstractions
{
    public interface IOddsFeed : IDisposable
    {
        /// <summary>
        /// Gets a <see cref="IProducerManager"/> instance used to retrieve producer related data
        /// </summary>
        IProducerManager ProducerManager { get; }

        /// <summary>
        /// Constructs and returns a new instance of <see cref="IOddsFeedSessionBuilder"/>
        /// </summary>
        /// <returns>Constructed instance of the <see cref="IOddsFeedSessionBuilder"/></returns>
        IOddsFeedSessionBuilder CreateBuilder();

        /// <summary>
        /// Opens the current feed by opening all created sessions
        /// </summary>
        void Open();

        /// <summary>
        /// Closes the current feed by closing all created sessions and disposing of all resources associated with the current instance
        /// </summary>
        void Close();

        /// <summary>
        /// Occurs when an exception occurs in the connection loop
        /// </summary>
        event EventHandler<ConnectionExceptionEventArgs> ConnectionException;
    }
}

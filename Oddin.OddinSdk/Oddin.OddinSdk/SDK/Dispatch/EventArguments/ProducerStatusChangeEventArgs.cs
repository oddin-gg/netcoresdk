using Oddin.OddinSdk.SDK.Dispatch.Abstractions;
using System;

namespace Oddin.OddinSdk.SDK.Dispatch.EventArguments
{
    /// <summary>
    /// Event arguments for the <see cref="IOddsFeed.ProducerDown"/> and <see cref="IOddsFeed.ProducerUp"/> events
    /// </summary>
    public class ProducerStatusChangeEventArgs : EventArgs
    {
        private readonly IProducerStatusChange _statusChange;

        internal ProducerStatusChangeEventArgs(IProducerStatusChange producerStatusChange)
        {
            if (producerStatusChange is null)
                throw new ArgumentNullException(nameof(producerStatusChange));

            _statusChange = producerStatusChange;
        }

        /// <summary>
        /// Gets a <see cref="IProducerStatusChange"/> implementation containing information about the producer status change
        /// </summary>
        /// <returns>Returns a <see cref="IProducerStatusChange"/> implementation containing information about the producer status change</returns>
        public IProducerStatusChange GetProducerStatusChange()
        {
            return _statusChange;
        }
    }
}

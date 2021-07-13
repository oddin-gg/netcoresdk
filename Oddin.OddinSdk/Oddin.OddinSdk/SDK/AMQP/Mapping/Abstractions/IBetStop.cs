using Oddin.OddinSdk.SDK.AMQP.Enums;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using System.Collections.Generic;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions
{
    public interface IBetStop<out T> : IEventMessage<T> where T : ISportEvent
    {
        /// <summary>
        /// Gets a <see cref="MarketStatus"/> specifying the new status of the associated markets
        /// </summary>
        /// <value>The market status.</value>
        MarketStatus MarketStatus { get; }

        /// <summary>
        /// Get a list of strings specifying which market groups needs to be stopped
        /// </summary>
        IEnumerable<string> Groups { get; }
    }
}

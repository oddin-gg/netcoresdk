using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using System.Collections.Generic;

namespace Oddin.OddinSdk.SDK.AMQP.Abstractions
{
    /// <summary>
    /// Defines a contract implemented by all messages containing market related information
    /// </summary>
    /// <typeparam name="T">A <see cref="IMarket"/> derived type specifying the type of markets</typeparam>
    /// <typeparam name="T1">A <see cref="ISportEvent"/> derived type specifying the type of the associated sport event</typeparam>
    public interface IMarketMessage<out T, out T1> : IEventMessage<T1> where T : IMarket where T1 : ISportEvent
    {
        /// <summary>
        /// Gets a <see cref="IEnumerable{IMarket}"/> describing markets associated with the current <see cref="IMarketMessage{T, R}"/>
        /// </summary>
        IEnumerable<T> Markets { get; }
    }
}

using Oddin.OddinSdk.SDK.AMQP.Enums;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;

namespace Oddin.OddinSdk.SDK.AMQP.Abstractions
{
    /// <summary>
    /// Defines a contract implemented by odds-change messages
    /// </summary>
    /// <typeparam name="T">A <see cref="ICompetition"/> derived type specifying the type of the associated sport event</typeparam>
    public interface IOddsChange<out T> : IMarketMessage<IMarketWithOdds, T> where T : ISportEvent
    {
        /// <summary>
        /// Gets the <see cref="INamedValue"/> specifying the reason for betting being stopped, or a null reference if the reason is not known
        /// </summary>
        /// <value>The bet stop reason.</value>
        INamedValue BetStopReason { get; }

        /// <summary>
        /// Gets a <see cref="INamedValue"/> indicating the odds change was triggered by a possible event
        /// </summary>
        INamedValue BettingStatus { get; }
    }
}

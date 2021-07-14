using Oddin.OddinSdk.SDK.API.Entities.Abstractions;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions
{
    /// <summary>
    /// Defines a contract implemented by odds-change messages
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IOddsChange<out T> : IMarketMessage<IMarketWithOdds, T> where T : ISportEvent
    {
        /// <summary>
        /// Gets the <see cref="int?"/> specifying the reason for betting being stopped
        /// </summary>
        /// <value>The bet stop reason.</value>
        int? BetStopReason { get; }

        /// <summary>
        /// Gets a <see cref="int?"/> indicating the betting status
        /// </summary>
        int? BettingStatus { get; }
    }
}

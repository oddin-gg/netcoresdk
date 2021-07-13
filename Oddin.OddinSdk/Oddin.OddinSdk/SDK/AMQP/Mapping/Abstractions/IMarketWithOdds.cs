using Oddin.OddinSdk.SDK.AMQP.Enums;
using System.Collections.Generic;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions
{
    /// <summary>
    /// Defines a contract implemented by classes providing odds information for betting markets
    /// </summary>
    public interface IMarketWithOdds : IMarket
    {
        /// <summary>
        /// Gets an <see cref="IEnumerable{IOutcomeOdds}"/> where each <see cref="IOutcomeOdds"/> instance provides
        /// odds information for one outcome(selection)
        /// </summary>
        IEnumerable<IOutcomeOdds> OutcomeOdds { get; }

        /// <summary>
        /// Gets the market metadata which contains the additional market information
        /// </summary>
        /// <value>The market metadata which contains the additional market information</value>
        IMarketMetadata MarketMetadata { get; }
    }
}

using System.Collections.Generic;

namespace Oddin.OddinSdk.SDK.API.Entities.Abstractions
{
    public interface IMarketDescription
    {
        /// <summary>
        /// Gets a value uniquely identifying the current market
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets name of the current market
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets possible outcomes of this market
        /// </summary>
        IReadOnlyCollection<IOutcomeDescription> Outcomes { get; }
    }
}

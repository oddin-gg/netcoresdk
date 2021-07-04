using Oddin.OddinSdk.SDK.AMQP;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Oddin.OddinSdk.SDK.API.Abstractions
{
    public interface IApiClient
    {
        /// <summary>
        /// Gets a list of <see cref="IProducer"/> from API
        /// </summary>
        /// <returns>The list of <see cref="IProducer"/></returns>
        List<IProducer> GetProducers();

        /// <summary>
        /// Gets <see cref="IBookmakerDetails"/> from API
        /// </summary>
        /// <returns></returns>
        IBookmakerDetails GetBookmakerDetails();

        /// <summary>
        /// Gets <see cref="IMatchSummary"/> from API
        /// </summary>
        /// <param name="sportEventId">Unique identifier of the sport event</param>
        /// <param name="culture">Culture info for translations</param>
        /// <returns></returns>
        Task<IMatchSummary> GetMatchSummaryAsync(URN sportEventId, CultureInfo culture = null);
    }
}

using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using System.Collections.Generic;

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
    }
}

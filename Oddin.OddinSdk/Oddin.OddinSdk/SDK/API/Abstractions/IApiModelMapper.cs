using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.API.Models;
using Oddin.OddinSdk.Common.Exceptions;
using System.Collections.Generic;

namespace Oddin.OddinSdk.SDK.API.Abstractions
{
    internal interface IApiModelMapper
    {
        /// <exception cref="MappingException"></exception>
        public IBookmakerDetails MapBookmakerDetails(BookmakerDetailsModel model);

        /// <exception cref="MappingException"></exception>
        public List<IMarketDescription> MapMarketDescriptionsList(MarketDescriptionsModel model);

        /// <exception cref="MappingException"></exception>
        public IMatchSummary MapMatchSummary(MatchSummaryModel model);

        /// <exception cref="MappingException"></exception>
        public List<IProducer> MapProducersList(ProducersModel model);
    }
}

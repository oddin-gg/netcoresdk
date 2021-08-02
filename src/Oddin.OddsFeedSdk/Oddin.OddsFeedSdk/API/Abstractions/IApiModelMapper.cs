using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.API.Models;
using System.Collections.Generic;

namespace Oddin.OddsFeedSdk.API.Abstractions
{
    internal interface IApiModelMapper
    {
        public IBookmakerDetails MapBookmakerDetails(BookmakerDetailsModel model);

        public IMatchSummary MapMatchSummary(MatchSummaryModel model);

        public IEnumerable<IProducer> MapProducersList(ProducersModel model);
    }
}

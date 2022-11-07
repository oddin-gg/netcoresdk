using System.Collections.Generic;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.API.Models;

namespace Oddin.OddsFeedSdk.API.Abstractions;

internal interface IApiModelMapper
{
    public IBookmakerDetails MapBookmakerDetails(BookmakerDetailsModel model);

    public IMatchSummary MapMatchSummary(MatchSummaryModel model);

    public IEnumerable<IProducer> MapProducersList(ProducersModel model);
}
using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Oddin.OddsFeedSdk.AMQP.Mapping
{
    internal class MarketWithSettlement : MarketCancel, IMarketWithSettlement
    {
        public IEnumerable<IOutcomeSettlement> OutcomeSettlements { get; }

        public MarketStatus MarketStatus { get; }

        public MarketWithSettlement(
            MarketStatus marketStatus,
            int marketId,
            int refId,
            IDictionary<string, string> specifiers,
            string extentedSpecifiers,
            IEnumerable<IOutcomeSettlement> outcomes,
            IApiClient apiClient,
            ExceptionHandlingStrategy exceptionHandlingStrategy,
            int? voidReason)
            : base(marketId, refId, specifiers, extentedSpecifiers, apiClient, exceptionHandlingStrategy, voidReason)
        {
            MarketStatus = marketStatus;
            OutcomeSettlements = outcomes.ToList().AsReadOnly();
        }
    }
}
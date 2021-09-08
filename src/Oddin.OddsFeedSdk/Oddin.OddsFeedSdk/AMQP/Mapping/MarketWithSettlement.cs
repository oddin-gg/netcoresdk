using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Managers.Abstractions;
using System.Collections.Generic;
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
            IEnumerable<string> groups,
            IEnumerable<IOutcomeSettlement> outcomes,
            IMarketDescriptionFactory marketDescriptionFactory,
            ExceptionHandlingStrategy exceptionHandlingStrategy,
            int? voidReason)
            : base(marketId, refId, specifiers, extentedSpecifiers, groups, marketDescriptionFactory, exceptionHandlingStrategy, voidReason)
        {
            MarketStatus = marketStatus;
            OutcomeSettlements = outcomes
                .ToList()
                .AsReadOnly();
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Configuration.Abstractions;

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
            IReadOnlyDictionary<string, string> specifiers,
            string extendedSpecifiers,
            IEnumerable<string> groups,
            IEnumerable<IOutcomeSettlement> outcomes,
            IMarketDescriptionFactory marketDescriptionFactory,
            ISportEvent sportEvent,
            ExceptionHandlingStrategy exceptionHandlingStrategy,
            int? voidReason,
            int? voidReasonId,
            string? voidReasonParams
        )
            : base(
                marketId,
                refId,
                specifiers,
                extendedSpecifiers,
                groups,
                marketDescriptionFactory,
                sportEvent,
                exceptionHandlingStrategy,
                voidReason,
                voidReasonId,
                voidReasonParams)
        {
            MarketStatus = marketStatus;
            OutcomeSettlements = outcomes
                .ToList()
                .AsReadOnly();
        }
    }
}
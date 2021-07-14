using Oddin.OddinSdk.SDK.AMQP.Enums;
using Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions;
using Oddin.OddinSdk.SDK.API.Abstractions;
using Oddin.OddinSdk.SDK.Configuration.Abstractions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping
{
    internal class MarketWithSettlement : MarketCancel, IMarketWithSettlement
    {
        public IEnumerable<IOutcomeSettlement> OutcomeSettlements { get; }

        public MarketStatus MarketStatus { get; }

        public MarketWithSettlement(
            MarketStatus marketStatus,
            int marketId,
            IDictionary<string, string> specifiers,
            string extentedSpecifiers,
            IEnumerable<IOutcomeSettlement> outcomes,
            IApiClient apiClient,
            ExceptionHandlingStrategy exceptionHandlingStrategy,
            int? voidReason)
            : base(marketId, specifiers, extentedSpecifiers, apiClient, exceptionHandlingStrategy, voidReason)
        {
            MarketStatus = marketStatus;
            var readonlyOutcomes = outcomes as IReadOnlyCollection<IOutcomeSettlement>;
            OutcomeSettlements = readonlyOutcomes ?? new ReadOnlyCollection<IOutcomeSettlement>(outcomes.ToList());
        }
    }
}
using System.Collections.Generic;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Configuration.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping
{
    internal class MarketCancel : Market, IMarketCancel
    {
        public int? VoidReason { get; set; }
        public int? VoidReasonId { get; set; }
        public string? VoidReasonParams { get; set; }

        internal MarketCancel(
            int id,
            int refId,
            IReadOnlyDictionary<string, string> specifiers,
            string extendedSpecifiers,
            IEnumerable<string> groups,
            IMarketDescriptionFactory marketDescriptionFactory,
            ISportEvent sportEvent,
            ExceptionHandlingStrategy exceptionHandlingStrategy,
            int? voidReason,
            int? voidReasonId,
            string? voidReasonParams)
            : base(id, refId, specifiers, extendedSpecifiers, groups, marketDescriptionFactory, sportEvent, exceptionHandlingStrategy)
        {
            VoidReason = voidReason;
            VoidReasonId = voidReasonId;
            VoidReasonParams = voidReasonParams;
        }
    }
}
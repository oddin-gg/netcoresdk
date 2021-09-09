using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using System.Collections.Generic;

namespace Oddin.OddsFeedSdk.AMQP.Mapping
{
    internal class MarketCancel : Market, IMarketCancel
    {
        public int? VoidReason { get; set; }

        internal MarketCancel(
            int id,
            int refId,
            IDictionary<string, string> specifiers,
            string extentedSpecifiers,
            IEnumerable<string> groups,
            IMarketDescriptionFactory marketDescriptionFactory,
            ISportEvent sportEvent,
            ExceptionHandlingStrategy exceptionHandlingStrategy,
            int? voidReason)
            : base(id, refId, specifiers, extentedSpecifiers, groups, marketDescriptionFactory, sportEvent, exceptionHandlingStrategy)
        {
            VoidReason = voidReason;
        }
    }
}
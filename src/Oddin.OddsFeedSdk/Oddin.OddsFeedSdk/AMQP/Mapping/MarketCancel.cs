using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Managers.Abstractions;
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
            ExceptionHandlingStrategy exceptionHandlingStrategy,
            int? voidReason)
            : base(id, refId, specifiers, extentedSpecifiers, groups, marketDescriptionFactory, exceptionHandlingStrategy)
        {
            VoidReason = voidReason;
        }
    }
}
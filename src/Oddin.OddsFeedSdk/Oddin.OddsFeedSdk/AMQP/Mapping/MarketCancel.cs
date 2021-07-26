using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using System.Collections.Generic;

namespace Oddin.OddsFeedSdk.AMQP.Mapping
{
    internal class MarketCancel : Market, IMarketCancel
    {
        public int? VoidReason { get; set; }

        internal MarketCancel(
            int id,
            IDictionary<string, string> specifiers,
            string extentedSpecifiers,
            IApiClient client,
            ExceptionHandlingStrategy exceptionHandlingStrategy,
            int? voidReason)
            : base(id, specifiers, extentedSpecifiers, client, exceptionHandlingStrategy)
        {
            VoidReason = voidReason;
        }
    }
}
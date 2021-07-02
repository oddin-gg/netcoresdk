using Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions;
using Oddin.OddinSdk.SDK.AMQP.Messages;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.FeedConfiguration;
using System.Collections.Generic;
using System.Globalization;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping
{
    internal class FeedMessageMapper : IFeedMessageMapper
    {
        private readonly ExceptionHandlingStrategy _exceptionHandlingStrategy;

        public FeedMessageMapper(ExceptionHandlingStrategy exceptionHandlingStrategy)
        {
            _exceptionHandlingStrategy = exceptionHandlingStrategy;
        }

        public IOddsChange<T> MapOddsChange<T>(odds_change message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent
        {
            throw new System.NotImplementedException();
        }
    }
}

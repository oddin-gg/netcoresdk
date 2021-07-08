using Oddin.OddinSdk.SDK.AMQP.Messages;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using System.Collections.Generic;
using System.Globalization;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions
{
    internal interface IFeedMessageMapper
    {
        IOddsChange<T> MapOddsChange<T>(odds_change message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent;

        IBetStop<T> MapBetStop<T>(bet_stop message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent;
    }
}

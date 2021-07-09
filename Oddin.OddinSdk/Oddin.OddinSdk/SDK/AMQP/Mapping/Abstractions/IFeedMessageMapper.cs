using Oddin.OddinSdk.SDK.AMQP.Messages;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions
{
    internal interface IFeedMessageMapper
    {
        IOddsChange<T> MapOddsChange<T>(odds_change message, byte[] rawMessage) where T : ISportEvent;

        IBetStop<T> MapBetStop<T>(bet_stop message, byte[] rawMessage) where T : ISportEvent;
    }
}

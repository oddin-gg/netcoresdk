using Oddin.OddsFeedSdk.AMQP.Messages;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions
{
    internal interface IFeedMessageMapper
    {
        IOddsChange<T> MapOddsChange<T>(odds_change message, byte[] rawMessage) where T : ISportEvent;

        IBetStop<T> MapBetStop<T>(bet_stop message, byte[] rawMessage) where T : ISportEvent;

        IBetSettlement<T> MapBetSettlement<T>(bet_settlement message, byte[] rawMessage) where T : ISportEvent;

        IBetCancel<T> MapBetCancel<T>(bet_cancel message, byte[] rawMessage) where T : ISportEvent;

        IFixtureChange<T> MapFixtureChange<T>(fixture_change message, byte[] rawMessage) where T : ISportEvent;
    }
}

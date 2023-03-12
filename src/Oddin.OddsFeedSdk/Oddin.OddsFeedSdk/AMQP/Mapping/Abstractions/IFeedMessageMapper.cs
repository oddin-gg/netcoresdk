using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.AMQP.Messages;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions
{
    internal interface IFeedMessageMapper
    {
        IOddsChange<T> MapOddsChange<T>(odds_change message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent;

        IBetStop<T> MapBetStop<T>(bet_stop message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent;

        IBetSettlement<T> MapBetSettlement<T>(bet_settlement message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent;

        IRollbackBetSettlement<T> MapRollbackBetSettlement<T>(rollback_bet_settlement message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent;

        IRollbackBetCancel<T> MapRollbackBetCancel<T>(rollback_bet_cancel message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent;

        IBetCancel<T> MapBetCancel<T>(bet_cancel message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent;

        IFixtureChange<T> MapFixtureChange<T>(fixture_change message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent;
    }
}

using Oddin.OddsFeedSdk.AMQP.EventArguments;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using System;

namespace Oddin.OddsFeedSdk.Dispatch.Abstractions
{
    public interface IEntityDispatcher<T> where T : ISportEvent
    {
        event EventHandler<OddsChangeEventArgs<T>> OnOddsChange;

        event EventHandler<BetStopEventArgs<T>> OnBetStop;

        event EventHandler<BetSettlementEventArgs<T>> OnBetSettlement;

        event EventHandler<BetCancelEventArgs<T>> OnBetCancel;

        event EventHandler<FixtureChangeEventArgs<T>> OnFixtureChange;
    }
}

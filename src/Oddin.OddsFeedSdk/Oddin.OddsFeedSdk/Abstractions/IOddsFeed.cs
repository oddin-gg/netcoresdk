using System;
using System.Threading.Tasks;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Dispatch.EventArguments;
using Oddin.OddsFeedSdk.Managers.Abstractions;
using Oddin.OddsFeedSdk.Sessions.Abstractions;

namespace Oddin.OddsFeedSdk.Abstractions;

public interface IOddsFeed : IAsyncDisposable
{
    IProducerManager ProducerManager { get; }

    ISportDataProvider SportDataProvider { get; }

    IRecoveryManager RecoveryManager { get; }

    IBookmakerDetails BookmakerDetails { get; }

    IMarketDescriptionManager MarketDescriptionManager { get; }

    IOddsFeedSessionBuilder CreateBuilder();

    Task Open();
    Task Close();

    event EventHandler<ConnectionExceptionEventArgs> ConnectionException;
    event EventHandler<EventArgs> Disconnected;
    event EventHandler<FeedCloseEventArgs> Closed;
    event EventHandler<EventRecoveryCompletedEventArgs> EventRecoveryCompleted;
    event EventHandler<ProducerStatusChangeEventArgs> ProducerDown;
    event EventHandler<ProducerStatusChangeEventArgs> ProducerUp;
}
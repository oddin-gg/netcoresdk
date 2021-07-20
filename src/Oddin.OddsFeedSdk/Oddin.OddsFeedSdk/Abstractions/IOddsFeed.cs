using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Dispatch.EventArguments;
using Oddin.OddsFeedSdk.Managers.Abstractions;
using Oddin.OddsFeedSdk.Sessions.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Oddin.OddsFeedSdk.Abstractions
{
    public interface IOddsFeed : IDisposable
    {
        IProducerManager ProducerManager { get; }

        ISportDataProvider SportDataProvider { get; }

        IEventRecoveryRequestIssuer EventRecoveryRequestIssuer { get; }

        IBookmakerDetails BookmakerDetails { get; }

        IOddsFeedSessionBuilder CreateBuilder();

        void Open();
        void Close();

        event EventHandler<ConnectionExceptionEventArgs> ConnectionException;
        event EventHandler<EventArgs> Disconnected;
        event EventHandler<FeedCloseEventArgs> Closed;
        event EventHandler<EventRecoveryCompletedEventArgs> EventRecoveryCompleted;
        event EventHandler<ProducerStatusChangeEventArgs> ProducerDown;
        event EventHandler<ProducerStatusChangeEventArgs> ProducerUp;
    }
}

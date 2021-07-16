using Oddin.OddsFeedSdk.Dispatch.EventArguments;
using System;

namespace Oddin.OddsFeedSdk.Managers.Abstractions
{
    internal interface IEventRecoveryCompletedDispatcher
    {
        event EventHandler<EventRecoveryCompletedEventArgs> EventRecoveryCompleted;

        void Open();

        void Close();
    }
}

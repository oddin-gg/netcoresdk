using Oddin.OddinSdk.SDK.Dispatch.EventArguments;
using System;

namespace Oddin.OddinSdk.SDK.Managers.Abstractions
{
    internal interface IEventRecoveryCompletedDispatcher
    {
        event EventHandler<EventRecoveryCompletedEventArgs> EventRecoveryCompleted;

        void Open();

        void Close();
    }
}

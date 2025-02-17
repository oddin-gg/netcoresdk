using System;
using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;

namespace Oddin.OddsFeedSdk.Dispatch;

public abstract class DispatcherBase
{
    private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(DispatcherBase));

    public void Dispatch<TEventArgs>(
        EventHandler<TEventArgs> handler,
        TEventArgs eventArgs,
        string eventHandlerName,
        ExceptionHandlingStrategy exceptionHandlingStrategy
    )
    {
        if (handler is null)
        {
            return;
        }

        try
        {
            handler(this, eventArgs);
        }
        catch (Exception e)
        {
            _log.LogWarning(
                "An exception was thrown while {Type} was dispatching an event through {EventHandlerName} event handler: ${E}",
                GetType(), eventHandlerName, e);
            e.HandleAccordingToStrategy(GetType().Name, _log, exceptionHandlingStrategy);
        }
    }
}
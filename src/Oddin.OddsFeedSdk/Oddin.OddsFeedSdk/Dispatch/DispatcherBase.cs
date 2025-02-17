using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Oddin.OddsFeedSdk.Dispatch;

public abstract class DispatcherBase
{
    private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(DispatcherBase));

    public void Dispatch<TEventArgs>(EventHandler<TEventArgs> handler, TEventArgs eventArgs, string eventHandlerName)
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
                $"An exception was thrown while {GetType()} was dispatching an event through {eventHandlerName} event handler!",
                e);
        }
    }
}
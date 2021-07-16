﻿using Microsoft.Extensions.Logging;
using System;

namespace Oddin.OddsFeedSdk.Dispatch
{
    public abstract class DispatcherBase
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(DispatcherBase));

        public DispatcherBase()
        {

        }

        public void Dispatch<TEventArgs>(EventHandler<TEventArgs> handler, TEventArgs eventArgs, string eventHandlerName)
        {
            if (handler is null)
            {
                _log.LogWarning($"{GetType()} was unable to dispatch an event through {eventHandlerName} event handler, because there were no listeners subscribed to it!");
                return;
            }

            try
            {
                handler(this, eventArgs);
            }
            catch (Exception e)
            {
                _log.LogWarning($"An exception was thrown while {GetType()} was dispatching an event through {eventHandlerName} event handler!", e);
            }
        }
    }
}
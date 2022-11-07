using System;
using System.Collections.Generic;

namespace Oddin.OddsFeedSdk.Dispatch.EventArguments;

public class ConnectionExceptionEventArgs : EventArgs
{
    public ConnectionExceptionEventArgs(Exception exception, IDictionary<string, object> detail)
    {
        Exception = exception;
        Detail = detail;
    }

    public Exception Exception { get; }

    public IDictionary<string, object> Detail { get; }
}
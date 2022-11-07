using System;

namespace Oddin.OddsFeedSdk.Dispatch.EventArguments;

public class FeedCloseEventArgs : EventArgs
{
    private readonly string _reason;

    internal FeedCloseEventArgs(string reason)
    {
        if (reason is null)
            throw new ArgumentNullException(nameof(reason));

        if (reason == string.Empty)
            throw new ArgumentException($"Argument {nameof(reason)} cannot be empty!");

        _reason = reason;
    }

    public string GetReason() => _reason;
}
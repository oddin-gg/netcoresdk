using System;
using Oddin.OddsFeedSdk.Dispatch.Abstractions;

namespace Oddin.OddsFeedSdk.Dispatch.EventArguments;

public class ProducerStatusChangeEventArgs : EventArgs
{
    private readonly IProducerStatusChange _statusChange;

    internal ProducerStatusChangeEventArgs(IProducerStatusChange producerStatusChange)
    {
        if (producerStatusChange is null)
            throw new ArgumentNullException(nameof(producerStatusChange));

        _statusChange = producerStatusChange;
    }

    public IProducerStatusChange GetProducerStatusChange() => _statusChange;
}
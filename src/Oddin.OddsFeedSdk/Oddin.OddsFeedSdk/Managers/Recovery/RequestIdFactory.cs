using System.Threading;
using Oddin.OddsFeedSdk.Managers.Abstractions;

namespace Oddin.OddsFeedSdk.Managers.Recovery;

internal class RequestIdFactory : IRequestIdFactory
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private long _currentId;

    public RequestIdFactory() => _currentId = 0;

    public long GetNext()
    {
        _semaphore.Wait();
        try
        {
            var newId = _currentId < long.MaxValue
                ? _currentId + 1
                : 0;
            _currentId = newId;
            return newId;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
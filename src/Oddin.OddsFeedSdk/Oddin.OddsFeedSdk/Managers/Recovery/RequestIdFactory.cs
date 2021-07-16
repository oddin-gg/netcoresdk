using Oddin.OddsFeedSdk.Managers.Abstractions;
using System.Threading;

namespace Oddin.OddsFeedSdk.Managers.Recovery
{
    internal class RequestIdFactory : IRequestIdFactory
    {
        private long _currentId;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public RequestIdFactory()
        {
            _currentId = 0;
        }

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
}

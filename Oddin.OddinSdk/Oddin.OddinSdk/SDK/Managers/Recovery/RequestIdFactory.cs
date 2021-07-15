using Oddin.OddinSdk.SDK.Managers.Abstractions;

namespace Oddin.OddinSdk.SDK.Managers.Recovery
{
    internal class RequestIdFactory : IRequestIdFactory
    {
        private long _currentId;
        private readonly object _lockCurrentId = new object();

        public RequestIdFactory()
        {
            _currentId = 0;
        }

        public long GetNext()
        {
            lock (_lockCurrentId)
            {
                var newId = _currentId < long.MaxValue
                    ? _currentId + 1
                    : 0;
                _currentId = newId;
                return newId;
            }
        }
    }
}

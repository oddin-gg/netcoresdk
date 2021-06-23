using Microsoft.Extensions.Logging;

namespace Oddin.OddinSdk.SDK.Managers
{
    internal abstract class LoggingBase
    {
        protected ILogger _log;

        public LoggingBase(ILoggerFactory loggerFactory)
        {
            _log = loggerFactory.CreateLogger(GetType());
        }
    }
}

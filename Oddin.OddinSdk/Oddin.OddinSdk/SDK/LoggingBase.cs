using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Oddin.OddinSdk.SDK
{
    /// <summary>
    /// A base class for classes that have access to log
    /// </summary>
    public abstract class LoggingBase
    {
        protected ILogger _log;

        /// <summary>
        /// Initializes a new instance of <see cref="LoggingBase"/>
        /// </summary>
        /// <param name="loggerFactory">Logger factory used to create a logger</param>
        public LoggingBase(ILoggerFactory loggerFactory)
        {
            loggerFactory ??= new NullLoggerFactory();
            _log = loggerFactory.CreateLogger(GetType());
        }
    }
}

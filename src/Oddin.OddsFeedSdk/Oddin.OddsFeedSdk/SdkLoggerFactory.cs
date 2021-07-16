using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace Oddin.OddsFeedSdk
{
    internal static class SdkLoggerFactory
    {
        private static bool _initialized = false;
        private static ILoggerFactory _loggerFactory;

        public static ILogger GetLogger(Type type)
        {
            if (_initialized == false)
                throw new InvalidOperationException("Trying to get logger from not-initialized logger factory!");

            return _loggerFactory.CreateLogger(type);
        }

        public static void Initialize(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory is null ? new NullLoggerFactory() : loggerFactory;
            _initialized = true;
        }
    }
}

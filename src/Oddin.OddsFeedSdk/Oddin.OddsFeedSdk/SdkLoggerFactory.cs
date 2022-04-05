using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace Oddin.OddsFeedSdk
{
    internal static class SdkLoggerFactory
    {
        private static ILoggerFactory _loggerFactory;

        public static ILogger GetLogger(Type type)
        {
            if (_loggerFactory == null)
                throw new InvalidOperationException("Trying to get logger from not-initialized logger factory!");

            return _loggerFactory.CreateLogger(type);
        }

        public static void Initialize(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? new NullLoggerFactory();
        }
    }
}
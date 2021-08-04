using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using System;
using System.Runtime.Caching;

namespace Oddin.OddsFeedSdk.Common
{
    internal static class Extensions
    {
        public static long ToEpochTimeMilliseconds(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
        }

        public static DateTime FromEpochTimeMilliseconds(this long timestamp)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;
        }

        public static void HandleAccordingToStrategy(this Exception exception, string catcher, ILogger logger, ExceptionHandlingStrategy exceptionHandlingStrategy)
        {
            logger.LogWarning($"An exception was catched in {catcher}, exception {exception}");
            if (exceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                throw exception;
        }

        public static CacheItemPolicy AsCachePolicy(this TimeSpan cacheTTL)
            => new CacheItemPolicy() { AbsoluteExpiration = DateTimeOffset.Now.Add(cacheTTL) };
    }
}

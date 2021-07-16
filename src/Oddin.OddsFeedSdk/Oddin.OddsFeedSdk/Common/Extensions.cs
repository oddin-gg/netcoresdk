using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using System;

namespace Oddin.OddsFeedSdk.Common
{
    internal static class Extensions
    {
        public static long ToEpochTimeMilliseconds(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
        }

        public static void HandleAccordingToStrategy(this Exception exception, string catcher, ILogger logger, ExceptionHandlingStrategy exceptionHandlingStrategy)
        {
            logger.LogWarning($"An exception was catched in {catcher}, exception {exception}");
            if (exceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                throw exception;
        }
    }
}

using Microsoft.Extensions.Logging;
using Oddin.OddinSdk.SDK.FeedConfiguration;
using System;

namespace Oddin.OddinSdk.Common
{
    internal static class Extensions
    {
        public static long ToEpochTimeMilliseconds(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Logs the exception, and if <paramref name="exceptionHandlingStrategy"/> is <see cref="ExceptionHandlingStrategy.THROW"/>, throws it
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="catcher"></param>
        /// <param name="logger"></param>
        /// <param name="exceptionHandlingStrategy"></param>
        public static void HandleAccordingToStrategy(this Exception exception, string catcher, ILogger logger, ExceptionHandlingStrategy exceptionHandlingStrategy)
        {
            logger.LogWarning($"An exception was catched in {catcher}, exception {exception}");
            if (exceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                throw exception;
        }
    }
}

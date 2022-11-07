using System;
using System.Net;
using System.Runtime.Caching;
using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.Configuration.Abstractions;

namespace Oddin.OddsFeedSdk.Common;

internal static class Extensions
{
    public static void HandleAccordingToStrategy(this Exception exception, string catcher, ILogger logger,
        ExceptionHandlingStrategy exceptionHandlingStrategy)
    {
        logger.LogWarning($"An exception was caught in {catcher}, exception {exception}");
        if (exceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
            throw exception;
    }

    public static CacheItemPolicy AsCachePolicy(this TimeSpan cacheTtl) =>
        new() { AbsoluteExpiration = DateTimeOffset.Now.Add(cacheTtl) };

    public static bool IsSuccessStatusCode(this HttpStatusCode statusCode)
        => (int)statusCode >= 200 && (int)statusCode <= 299;
}
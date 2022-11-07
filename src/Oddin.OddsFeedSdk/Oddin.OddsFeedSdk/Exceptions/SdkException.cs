using System;

namespace Oddin.OddsFeedSdk.Exceptions;

/// <summary>
///     General SDK Exception
/// </summary>
public class SdkException : Exception
{
    public SdkException(string message)
        : base(message)
    {
    }

    public SdkException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
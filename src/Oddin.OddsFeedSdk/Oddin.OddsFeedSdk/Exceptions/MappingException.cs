using System;

namespace Oddin.OddsFeedSdk.Exceptions;

/// <summary>
///     Exception when mapping received objects
/// </summary>
public class MappingException : SdkException
{
    public MappingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
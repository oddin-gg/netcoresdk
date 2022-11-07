using System;

namespace Oddin.OddsFeedSdk.Exceptions;

public class ItemNotFoundException : SdkException
{
    public ItemNotFoundException(string id, string message)
        : base(message) =>
        Id = id;

    public ItemNotFoundException(string id, string message, Exception innerException)
        : base(message, innerException) =>
        Id = id;

    public string Id { get; }
}
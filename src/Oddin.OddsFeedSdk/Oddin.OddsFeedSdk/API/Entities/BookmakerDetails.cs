using System;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.API.Entities;

internal class BookmakerDetails : IBookmakerDetails
{
    public BookmakerDetails(DateTime expireAt, int bookmakerId, string virtualHost)
    {
        ExpireAt = expireAt;
        BookmakerId = bookmakerId;
        VirtualHost = virtualHost;
    }

    public DateTime ExpireAt { get; }

    public int BookmakerId { get; }

    public string VirtualHost { get; }
}
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using System;

namespace Oddin.OddsFeedSdk.API.Entities
{
    internal class BookmakerDetails : IBookmakerDetails
    {
        public DateTime ExpireAt { get; }

        public int BookmakerId { get; }

        public string VirtualHost { get; }

        public BookmakerDetails(DateTime expireAt, int bookmakerId, string virtualHost)
        {
            ExpireAt = expireAt;
            BookmakerId = bookmakerId;
            VirtualHost = virtualHost;
        }
    }
}

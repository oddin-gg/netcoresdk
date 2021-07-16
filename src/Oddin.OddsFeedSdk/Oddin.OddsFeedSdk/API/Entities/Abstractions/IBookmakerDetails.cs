using System;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions
{
    public interface IBookmakerDetails
    {
        DateTime ExpireAt { get; }

        int BookmakerId { get; }

        string VirtualHost { get; }
    }
}

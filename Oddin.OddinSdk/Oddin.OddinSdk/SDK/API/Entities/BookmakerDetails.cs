using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.API.Models;
using System;

namespace Oddin.OddinSdk.SDK.API.Entities
{
    internal class BookmakerDetails : IBookmakerDetails
    {
        public DateTime ExpireAt { get; }

        public int BookmakerId { get; }

        public string VirtualHost { get; }

        public BookmakerDetails(BookmakerDetailsModel model)
        {
            ExpireAt = model.expire_at;
            BookmakerId = model.bookmaker_id;
            VirtualHost = model.virtual_host;
        }
    }
}

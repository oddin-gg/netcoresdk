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

    /// <summary>
    /// Defines  a contract implemented by classes representing bookmaker information
    /// </summary>
    public interface IBookmakerDetails
    {
        /// <summary>
        /// Gets a value specifying the bookmaker's token will expire
        /// </summary>
        DateTime ExpireAt { get; }

        /// <summary>
        /// Gets the Sportradar's provided bookmaker id of the associated bookmaker
        /// </summary>
        int BookmakerId { get; }

        /// <summary>
        /// Gets the virtual host which should be used when connecting to the AMQP broker
        /// </summary>
        string VirtualHost { get; }
    }
}

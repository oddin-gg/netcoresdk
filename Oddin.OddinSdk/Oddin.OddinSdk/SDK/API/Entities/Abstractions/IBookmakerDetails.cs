using System;

namespace Oddin.OddinSdk.SDK.API.Entities.Abstractions
{
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

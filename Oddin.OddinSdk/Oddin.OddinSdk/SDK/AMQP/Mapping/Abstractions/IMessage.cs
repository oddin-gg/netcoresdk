﻿using Oddin.OddinSdk.SDK.API.Entities.Abstractions;

namespace Oddin.OddinSdk.SDK.AMQP.Abstractions
{
    /// <summary>
    /// Defines a contract followed by all top-level messages produced by the feed
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// Gets a <see cref="Producer"/> specifying the producer / service which dispatched the current <see cref="IMessage"/> message
        /// </summary>
        IProducer Producer { get; }

        /// <summary>
        /// Gets the timestamps when the message was generated, sent, received and dispatched by the sdk
        /// </summary>
        /// <value>The timestamps</value>
        IMessageTimestamp Timestamps { get; }
    }
}
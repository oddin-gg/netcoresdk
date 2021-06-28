using Microsoft.Extensions.Logging;
using Oddin.OddinSdk.Common;
using Oddin.OddinSdk.SDK.AMQP.Abstractions;
using Oddin.OddinSdk.SDK.AMQP.Messages;
using System;

namespace Oddin.OddinSdk.SDK.AMQP
{
    internal class FeedMessageDeserializer : LoggingBase, IFeedMessageDeserializer
    {
        public FeedMessageDeserializer(ILoggerFactory loggerFactory) : base(loggerFactory)
        {

        }

        public FeedMessage DeserializeMessage(string message)
        {
            if (XmlHelper.TryDeserialize(message, out AliveMessage aliveMessage))
                return aliveMessage;

            var errorMessage = $"An unknown type of feed message was encountered and could not be deserialized! Message: {message}";
            _log.LogError(errorMessage);
            throw new ArgumentException(errorMessage);
        }
    }
}

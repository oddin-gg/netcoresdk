using Oddin.OddinSdk.SDK.AMQP.Messages;

namespace Oddin.OddinSdk.SDK.AMQP.Abstractions
{
    public interface IFeedMessageDeserializer
    {
        /// <summary>
        /// Translates XML to <see cref="FeedMessage"/> object
        /// </summary>
        /// <param name="message">XML <see cref="string"/> representing serialized message</param>
        /// <returns><see cref="FeedMessage"/> representing deserialized message</returns>
        FeedMessage DeserializeMessage(string message);
    }
}

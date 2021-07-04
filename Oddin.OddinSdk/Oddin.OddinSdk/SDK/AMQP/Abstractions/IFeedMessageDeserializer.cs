using Oddin.OddinSdk.SDK.AMQP.Messages;

namespace Oddin.OddinSdk.SDK.AMQP.Abstractions
{
    public interface IFeedMessageDeserializer
    {
        /// <summary>
        /// Translates XML to <see cref="FeedMessageModel"/> object
        /// </summary>
        /// <param name="message">XML <see cref="string"/> representing serialized message</param>
        /// <returns><see cref="FeedMessageModel"/> representing deserialized message</returns>
        bool TryDeserializeMessage(string message, out FeedMessageModel feedMessage);
    }
}

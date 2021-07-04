using Microsoft.Extensions.Logging;
using Oddin.OddinSdk.Common;
using Oddin.OddinSdk.SDK.AMQP.Abstractions;
using Oddin.OddinSdk.SDK.AMQP.Messages;

namespace Oddin.OddinSdk.SDK.AMQP
{
    internal class FeedMessageDeserializer : IFeedMessageDeserializer
    {
        public FeedMessageDeserializer()
        {

        }

        public bool TryDeserializeMessage(string message, out FeedMessageModel feedMessage)
        {
            if (XmlHelper.TryDeserialize(message, out alive aliveMessage))
            {
                feedMessage = aliveMessage;
                return true;
            }

            if (XmlHelper.TryDeserialize(message, out odds_change oddsChangeMessage))
            {
                feedMessage = oddsChangeMessage;
                return true;
            }

            // ...

            feedMessage = null;
            return false;
        }
    }
}

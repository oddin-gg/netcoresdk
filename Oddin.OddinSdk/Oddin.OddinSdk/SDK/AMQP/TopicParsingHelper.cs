using Oddin.OddinSdk.SDK.AMQP.Messages;
using System;

namespace Oddin.OddinSdk.SDK.AMQP
{
    static class TopicParsingHelper
    {
        public const int TOTAL_SECTIONS_COUNT = 8;

        private static string[] GetSections(string topic)
        {
            var sections = topic.Split(".");
            if (sections.Length != TOTAL_SECTIONS_COUNT)
            {
                throw new ArgumentException($"AMQP message topic should consist of {TOTAL_SECTIONS_COUNT} sections. Only {sections.Length} were found!");
            }
            return sections;
        }

        public static MessageType GetMessageType(string topic)
        {
            var messageTypeString = GetSections(topic)[3];
            MessageType result = MessageType.UNKNOWN;
            try
            {
                result = (MessageType)Enum.Parse(typeof(MessageType), messageTypeString.ToUpper());
            }
            catch (Exception e)
            when (e is ArgumentException
                || e is OverflowException)
            {
                // TODO: finish
            }
            return result;
        }
    }
}

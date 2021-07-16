using Oddin.OddsFeedSdk.AMQP.Messages;
using System;

namespace Oddin.OddsFeedSdk.AMQP
{
    public static class TopicParsingHelper
    {
        public const int TOTAL_SECTIONS_COUNT = 8;
        public const int EVENT_ID_SECTION_INDEX = 6;
        public const int MESSAGE_TYPE_SECTION_INDEX = 3;
        public const int LIVE_SECTION_INDEX = 2;
        public const int PRE_SECTION_INDEX = 1;

        public const string PRE_PRODUCER_STRING = "pre";
        public const string LIVE_PRODUCER_STRING = "live";

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
            var messageTypeString = GetSections(topic)[MESSAGE_TYPE_SECTION_INDEX];
            MessageType result;
            try
            {
                result = (MessageType)Enum.Parse(typeof(MessageType), messageTypeString.ToUpper());
            }
            catch (Exception e)
            when (e is ArgumentException
                || e is ArgumentNullException
                || e is OverflowException)
            {
                return MessageType.UNKNOWN;
            }
            return result;
        }

        public static string GetProducer(string topic)
        {
            var sections = GetSections(topic);
            var isProducerLive = sections[LIVE_SECTION_INDEX] == LIVE_PRODUCER_STRING;
            var isProducerPre = sections[PRE_SECTION_INDEX] == PRE_PRODUCER_STRING;

            if (isProducerLive && (isProducerPre == false))
                return LIVE_PRODUCER_STRING;

            if (isProducerPre && (isProducerLive == false))
                return PRE_PRODUCER_STRING;

            return string.Empty;
        }

        public static string GetEventId(string topic)
        {
            var sections = GetSections(topic);
            // INFO: "-" in event ID section means ID is not specified
            return sections[EVENT_ID_SECTION_INDEX] == "-" ? string.Empty : sections[EVENT_ID_SECTION_INDEX];
        }
    }
}

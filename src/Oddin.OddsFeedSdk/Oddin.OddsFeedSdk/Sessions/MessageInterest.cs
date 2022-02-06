using Oddin.OddsFeedSdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Oddin.OddsFeedSdk.Sessions
{
    /// <summary>
    /// Sets what types of messages will be provided by the feed
    /// </summary>
    public class MessageInterest
    {
        public string Name { get; }

        internal MessageInterestType MessageInterestType { get; }

        public IReadOnlyCollection<string> RoutingKeys { get; }

        internal MessageInterest(string name, MessageInterestType messageInterestType, string routingKey)
        {
            Name = name;
            MessageInterestType = messageInterestType;
            RoutingKeys = new List<string>() { routingKey };
        }

        internal MessageInterest(string name, MessageInterestType messageInterestType, IEnumerable<string> routingKeys)
        {
            Name = name;
            MessageInterestType = messageInterestType;
            RoutingKeys = routingKeys.ToList().AsReadOnly();
        }

        public static readonly MessageInterest AllMessages = new MessageInterest("all", MessageInterestType.All, "*.*.*.*.*.*.*");

        public static readonly MessageInterest LiveMessagesOnly = new MessageInterest("live", MessageInterestType.Live, "*.*.live.*.*.*.*");

        public static readonly MessageInterest PrematchMessagesOnly = new MessageInterest("prematch", MessageInterestType.Prematch, "*.pre.*.*.*.*.*");

        public static readonly MessageInterest HighPriorityMessages = new MessageInterest("high_priority", MessageInterestType.HighPriority, "hi.*.*.*.*.*.*");

        public static readonly MessageInterest LowPriorityMessages = new MessageInterest("low_priority", MessageInterestType.LowPriority, "lo.*.*.*.*.*.*");

        public static readonly MessageInterest SystemAliveOnlyMessages = new("system_alive", MessageInterestType.SystemAlive, "-.-.-.alive.#");

        public static MessageInterest SpecificEventsOnly(IEnumerable<URN> eventIds)
        {
            if (eventIds is null)
                throw new ArgumentNullException($"{nameof(eventIds)} argument cannot be null!");

            if (!eventIds.Any())
                throw new ArgumentException($"{nameof(eventIds)} argument cannot be empty!");

            return new MessageInterest("specific_events", MessageInterestType.SpecificEvents, BuildRoutingKeysFromEvents(eventIds.Distinct()));
        }

        private static IEnumerable<string> BuildRoutingKeysFromEvents(IEnumerable<URN> eventIds)
            => eventIds.Select(eid => $"#.{eid.Prefix}:{eid.Type}.{eid.Id}");

        protected bool Equals(MessageInterest other) => MessageInterestType == other.MessageInterestType;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((MessageInterest) obj);
        }

        public override int GetHashCode() => (int) MessageInterestType;
    }

    internal enum MessageInterestType
    {
        All,
        Live,
        Prematch,
        HighPriority,
        LowPriority,
        SystemAlive,
        SpecificEvents
    }
}
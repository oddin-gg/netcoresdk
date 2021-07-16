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

        public IReadOnlyCollection<string> RoutingKeys { get; }

        internal MessageInterest(string name, string routingKey)
        {
            Name = name;
            RoutingKeys = new List<string>() { routingKey };
        }

        internal MessageInterest(string name, IEnumerable<string> routingKeys)
        {
            Name = name;
            RoutingKeys = routingKeys.ToList().AsReadOnly();
        }

        public static readonly MessageInterest AllMessages = new MessageInterest("all", "*.*.*.*.*.*.*.*");

        public static readonly MessageInterest LiveMessagesOnly = new MessageInterest("live", "*.*.live.*.*.*.*.*");

        public static readonly MessageInterest PrematchMessagesOnly = new MessageInterest("prematch", "*.pre.*.*.*.*.*.*");

        public static readonly MessageInterest HighPriorityMessages = new MessageInterest("high_priority", "hi.*.*.*.*.*.*.*");

        public static readonly MessageInterest LowPriorityMessages = new MessageInterest("low_priority", "lo.*.*.*.*.*.*.*");

        public static readonly MessageInterest SystemAliveOnlyMessages = new MessageInterest("system_alive", "-.-.-.alive.#");

        public static MessageInterest SpecificEventsOnly(IEnumerable<URN> eventIds)
        {
            if (eventIds is null)
                throw new ArgumentNullException($"{nameof(eventIds)} argument cannot be null!");

            if (eventIds.Count() <= 0)
                throw new ArgumentException($"{nameof(eventIds)} argument cannot be empty!");

            return new MessageInterest("specific_events", BuildRoutingKeysFromEvents(eventIds.Distinct()));
        }

        private static IEnumerable<string> BuildRoutingKeysFromEvents(IEnumerable<URN> eventIds)
            => eventIds.Select(eid => $"#.{eid.Prefix}:{eid.Type}.{eid.Id}.*");
 
    }
}

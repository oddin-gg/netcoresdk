using Oddin.OddinSdk.SDK.AMQP;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Oddin.OddinSdk.SDK.Sessions
{
    /// <summary>
    /// Defines which messages will be provided by feed
    /// </summary>
    public class MessageInterest
    {
        /// <summary>
        /// Gets the name of the message interest
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the routing keys used to filter messages
        /// </summary>
        public IReadOnlyCollection<string> RoutingKeys { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageInterest"/> class
        /// </summary>
        public MessageInterest(string name, string routingKey)
        {
            Name = name;
            RoutingKeys = new List<string>() { routingKey };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageInterest"/> class
        /// </summary>
        public MessageInterest(string name, IEnumerable<string> routingKeys)
        {
            Name = name;
            RoutingKeys = routingKeys as IReadOnlyCollection<string>;
        }

        /// <summary>
        /// Constructs a <see cref="MessageInterest"/> indicating an interest in all messages
        /// </summary>
        /// <returns>A <see cref="MessageInterest"/> indicating an interest in all messages</returns>
        public static readonly MessageInterest AllMessages = new MessageInterest("all", "*.*.*.*.*.*.*");

        /// <summary>
        /// Constructs a <see cref="MessageInterest"/> indicating an interest in live messages
        /// </summary>
        /// <returns>A <see cref="MessageInterest"/> indicating an interest in live messages</returns>
        public static readonly MessageInterest LiveMessagesOnly = new MessageInterest("live", "*.*.live.*.*.*.*");

        /// <summary>
        /// Constructs a <see cref="MessageInterest"/> indicating an interest in pre-match messages
        /// </summary>
        /// <returns>A <see cref="MessageInterest"/> indicating an interest in pre-match messages</returns>
        public static readonly MessageInterest PrematchMessagesOnly = new MessageInterest("prematch", "*.pre.*.*.*.*.*");

        /// <summary>
        /// Constructs a <see cref="MessageInterest"/> indicating an interest in hi priority messages
        /// </summary>
        /// <returns>A <see cref="MessageInterest"/> indicating an interest in high priority messages</returns>
        public static readonly MessageInterest HighPriorityMessages = new MessageInterest("high_priority", "hi.*.*.*.*.*.*");

        /// <summary>
        /// Constructs a <see cref="MessageInterest"/> indicating an interest in low priority messages
        /// </summary>
        /// <returns>A <see cref="MessageInterest"/> indicating an interest in low priority messages</returns>
        public static readonly MessageInterest LowPriorityMessages = new MessageInterest("low_priority", "lo.*.*.*.*.*.*");

        /// <summary>
        /// Constructs a <see cref="MessageInterest"/> indicating an interest in system alive messages
        /// </summary>
        /// <returns>A <see cref="MessageInterest"/> indicating an interest in system alive messages</returns>
        public static readonly MessageInterest SystemAliveOnlyMessages = new MessageInterest("system_alive", "-.-.-.alive.#");

        /// <summary>
        /// Constructs a <see cref="MessageInterest"/> indicating an interest in messages associated with specific events
        /// </summary>
        /// <param name="eventIds">A <see cref="IEnumerable{Integer}"/> specifying the target events</param>
        /// <returns>A <see cref="MessageInterest"/> indicating an interest in messages associated with specific events</returns>
        public static MessageInterest SpecificEventsOnly(IEnumerable<URN> eventIds)
        {
            if (eventIds is null)
                throw new ArgumentNullException($"{nameof(eventIds)} argument cannot be null!");

            if (eventIds.Count() <= 0)
                throw new ArgumentException($"{nameof(eventIds)} argument cannot be empty!");

            return new MessageInterest("specific_events", BuildRoutingKeysFromEvents(eventIds.Distinct()));
        }

        private static IEnumerable<string> BuildRoutingKeysFromEvents(IEnumerable<URN> eventIds)
        {
            return eventIds.Select(eid => $"#.{eid.Prefix}:{eid.Type}.{eid.Id}");
        }
    }
}

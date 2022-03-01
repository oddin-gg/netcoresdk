using Oddin.OddsFeedSdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

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

        private MessageInterest(string name, MessageInterestType messageInterestType, string routingKey)
        {
            Name = name;
            MessageInterestType = messageInterestType;
            RoutingKeys = new List<string>() { routingKey };
        }

        private MessageInterest(string name, MessageInterestType messageInterestType, IEnumerable<string> routingKeys)
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

        private bool Equals(MessageInterest other) => MessageInterestType == other.MessageInterestType;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((MessageInterest) obj);
        }

        public override int GetHashCode() => (int) MessageInterestType;

        internal bool IsProducerInScope(Producer producer)
        {
            if (MessageInterestType == LiveMessagesOnly.MessageInterestType)
            {
                return producer.ProducerScopes.Contains(ProducerScope.Live);
            }

            if (MessageInterestType == PrematchMessagesOnly.MessageInterestType)
            {
                return producer.ProducerScopes.Contains(ProducerScope.Prematch);
            }

            return true;
        }

        internal IEnumerable<int> GetPossibleSourceProducers(IEnumerable<IProducer> availableProducers)
        {
            var possibleProducers = new HashSet<int>();
            if (MessageInterestType == LiveMessagesOnly.MessageInterestType)
            {
                foreach (var producer in availableProducers)
                {
                    if (producer.ProducerScopes.Contains(ProducerScope.Live))
                    {
                        possibleProducers.Add(producer.Id);
                    }
                }
            } else  if (MessageInterestType == PrematchMessagesOnly.MessageInterestType)
            {
                foreach (var producer in availableProducers)
                {
                    if (producer.ProducerScopes.Contains(ProducerScope.Prematch))
                    {
                        possibleProducers.Add(producer.Id);
                    }
                }
            }
            else
            {
                foreach (var producer in availableProducers)
                {
                    possibleProducers.Add(producer.Id);
                }
            }

            return possibleProducers;
        }
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
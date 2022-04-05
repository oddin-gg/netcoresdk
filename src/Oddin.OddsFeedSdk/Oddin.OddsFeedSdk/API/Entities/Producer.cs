using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Entities
{
    public enum ProducerScope
    {
        Prematch = 0,
        Live = 1
    }

    class ProducerData
    {
        internal readonly int Id;
        internal readonly string Name;
        internal readonly string Description;
        internal readonly bool Active;
        internal bool Enabled;
        internal readonly string ApiUrl;
        internal readonly int StatefulRecoveryInMinutes;
        internal readonly IEnumerable<ProducerScope> ProducerScopes;

        internal long LastMessageTimestamp = 0;
        internal bool FlaggedDown = true;
        internal long LastProcessedMessageGenTimestamp = 0;
        internal long LastAliveReceivedGenTimestamp = 0;
        internal long RecoveryFromTimestamp = 0;
        internal IRecoveryInfo LastRecoveryInfo = null;

        internal ProducerData(int id, string name, string description, bool active, string apiUrl, string producerScopes, int statefulRecoveryInMinutes)
        {
            Id = id;
            Name = name;
            Description = description;
            Active = active;
            Enabled = active;
            ApiUrl = apiUrl;
            StatefulRecoveryInMinutes = statefulRecoveryInMinutes;

            ProducerScopes = producerScopes.Split("|")?.Select(s =>
            {
                return s switch
                {
                    "prematch" => ProducerScope.Prematch as ProducerScope?,
                    "live" => ProducerScope.Live,
                    _ => null
                };
            }).OfType<ProducerScope>();
        }

        public override string ToString() => $"{Id}-{Name}";
    }


    internal class Producer : IProducer
    {
        public ProducerData ProducerData;

        public int Id => ProducerData.Id;

        public string Name => ProducerData.Name;

        public string Description => ProducerData.Description;

        public long LastMessageTimestamp => ProducerData.LastMessageTimestamp;

        public bool IsAvailable => ProducerData.Active;

        public bool IsDisabled => !ProducerData.Enabled;

        public bool IsProducerDown => ProducerData.FlaggedDown;

        public string ApiUrl => ProducerData.ApiUrl;

        public IEnumerable<ProducerScope> ProducerScopes => ProducerData.ProducerScopes;

        public long LastProcessedMessageGenTimestamp => ProducerData.LastProcessedMessageGenTimestamp;

        public long ProcessingQueDelay => Timestamp.Now() -
                                          LastProcessedMessageGenTimestamp;

        public long TimestampForRecovery
        {
            get
            {
                var lastAliveReceivedGenTimestamp = ProducerData.LastAliveReceivedGenTimestamp;
                return lastAliveReceivedGenTimestamp == 0
                    ?  ProducerData.RecoveryFromTimestamp
                    : lastAliveReceivedGenTimestamp;
            }
        }

        public int StatefulRecoveryWindowInMinutes => ProducerData.StatefulRecoveryInMinutes;

        public IRecoveryInfo RecoveryInfo => ProducerData.LastRecoveryInfo;

        public Producer(ProducerData producerData) {
            ProducerData = producerData;
        }
    }
}
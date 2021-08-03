using System;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions
{
    public interface IProducer
    { 
        public int Id { get; }

        public string Name { get; }

        public string Description { get; }

        public bool IsAvailable { get; }

        public bool IsDisabled { get; }

        public bool IsProducerDown { get; }

        public DateTime LastTimestampBeforeDisconnect { get; }

        public int MaxRecoveryTime { get; }

        public int MaxInactivitySeconds { get; }

        public IRecoveryInfo RecoveryInfo { get; }
    }
}

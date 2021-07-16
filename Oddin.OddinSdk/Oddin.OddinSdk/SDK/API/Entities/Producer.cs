using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using System;
using System.Collections.Generic;

namespace Oddin.OddinSdk.SDK.API.Entities
{
    internal class Producer : IProducer
    {
        public int Id { get; }

        public string Name { get; }

        public string Description { get; }

        public bool IsAvailable { get; }

        public bool IsDisabled { get; private set; }

        public bool IsProducerDown { get; private set; }

        public DateTime LastTimestampBeforeDisconnect { get; private set; }

        public int MaxRecoveryTime { get; }

        public int MaxInactivitySeconds { get; }

        public IRecoveryInfo RecoveryInfo { get; internal set; }

        public IEnumerable<string> Scope { get; }

        public Producer(
            int id,
            string name,
            string description,
            bool active,
            string scope,
            int maxInactivitySeconds,
            int statefulRecoveryWindowInMinutes
            )
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            if (name == string.Empty)
                throw new ArgumentException($"Argument {nameof(name)} cannot be empty!");

            if (description is null)
                throw new ArgumentNullException(nameof(description));

            if (description == string.Empty)
                throw new ArgumentException($"Argument {nameof(description)} cannot be empty!");

            if (maxInactivitySeconds <= 0)
                throw new ArgumentException($"Argument {nameof(maxInactivitySeconds)} has to be positive!");

            if (statefulRecoveryWindowInMinutes <= 0)
                throw new ArgumentException($"Argument {nameof(statefulRecoveryWindowInMinutes)} has to be positive!");

            Id = id;
            Name = name;
            Description = description;
            IsAvailable = active;
            IsProducerDown = true;
            IsDisabled = false;
            LastTimestampBeforeDisconnect = DateTime.MinValue;
            Scope = scope?.Split("|");
            MaxInactivitySeconds = maxInactivitySeconds;
            MaxRecoveryTime = statefulRecoveryWindowInMinutes;
        }

        internal void SetDisabled(bool disabled)
        {
            IsDisabled = disabled;
        }

        internal void SetProducerDown(bool down)
        {
            IsProducerDown = down;
        }

        internal void SetLastTimestampBeforeDisconnect(DateTime timestamp)
        {
            LastTimestampBeforeDisconnect = timestamp;
        }
    }
}

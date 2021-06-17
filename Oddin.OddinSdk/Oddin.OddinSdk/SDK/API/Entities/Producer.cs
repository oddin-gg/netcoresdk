using System;

namespace Oddin.Oddin.SDK.API.Entities
{
    internal class Producer : IProducer
    {
        private string _name;


        public int Id => throw new NotImplementedException();

        public string Name => _name;

        public string Description => throw new NotImplementedException();

        public bool IsAvailable => throw new NotImplementedException();

        public bool IsDisabled => throw new NotImplementedException();

        public bool IsProducerDown => throw new NotImplementedException();

        public DateTime LastTimestampBeforeDisconnect => throw new NotImplementedException();

        public int MaxRecoveryTime => throw new NotImplementedException();

        public int MaxInactivitySeconds => throw new NotImplementedException();

        public IRecoveryInfo RecoveryInfo => throw new NotImplementedException();


        public Producer(string name)
        {
            _name = name;
        }
    }

    public interface IProducer
    {
        /// <summary>
        /// Gets the id of the producer
        /// </summary>
        /// <value>The id</value>
        int Id { get; }

        /// <summary>
        /// Gets the name of the producer
        /// </summary>
        /// <value>The name</value>
        string Name { get; }

        /// <summary>
        /// Gets the description of the producer
        /// </summary>
        /// <value>The description</value>
        string Description { get; }

        /// <summary>
        /// Gets a value indicating whether the producer is available on feed
        /// </summary>
        /// <value><c>true</c> if this instance is active; otherwise, <c>false</c></value>
        bool IsAvailable { get; }

        /// <summary>
        /// Gets a value indicating whether the producer is disabled
        /// </summary>
        /// <value><c>true</c> if this instance is disabled; otherwise, <c>false</c></value>
        bool IsDisabled { get; }

        /// <summary>
        /// Gets a value indicating whether the producer is marked as down
        /// </summary>
        /// <value><c>true</c> if this instance is down; otherwise, <c>false</c></value>
        bool IsProducerDown { get; }

        /// <summary>
        /// Gets the last timestamp before disconnect for this producer
        /// </summary>
        /// <value>The last timestamp before disconnect</value>
        DateTime LastTimestampBeforeDisconnect { get; }

        /// <summary>
        /// Gets the maximum recovery time
        /// </summary>
        /// <value>The maximum recovery time</value>
        int MaxRecoveryTime { get; }

        /// <summary>
        /// Gets the maximum inactivity seconds
        /// </summary>
        /// <value>The maximum inactivity seconds</value>
        int MaxInactivitySeconds { get; }

        /// <summary>
        /// Gets the recovery info about last recovery attempt
        /// </summary>
        /// <value>The recovery info about last recovery attempt</value>
        IRecoveryInfo RecoveryInfo { get; }
    }
}

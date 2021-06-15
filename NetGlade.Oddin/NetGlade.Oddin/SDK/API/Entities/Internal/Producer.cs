using System;

namespace NetGlade.Oddin.SDK.API.Entities.Internal
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
}

using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using System;
using System.Collections.Generic;

namespace Oddin.OddsFeedSdk.Managers.Abstractions
{
    public interface IProducerManager
    {
        IReadOnlyCollection<IProducer> Producers { get; }

        void DisableProducer(int id);

        IProducer Get(int id);

        IProducer Get(string name);

        bool Exists(int id);

        bool Exists(string name);

        void AddTimestampBeforeDisconnect(int id, DateTime timestamp);

        void RemoveTimestampBeforeDisconnect(int id);

        internal void Lock();
    }
}

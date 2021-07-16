using System;

namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions
{ 
    public interface IMarketMetadata
    {
        long? NextBetstop { get; }

        DateTime? NextBetstopDate => null;
    }
}

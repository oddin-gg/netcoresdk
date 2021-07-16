using System;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions
{
    /// <summary>
    /// Defines a contract for classes implementing additional market information
    /// </summary>
    public interface IMarketMetadata
    {
        /// <summary>
        /// Gets a epoch timestamp in UTC when to betstop the associated market. Typically used for outrights and typically is the start-time of the event the market refers to
        /// </summary>
        /// <value>The next betstop</value>
        long? NextBetstop { get; }

        /// <summary>
        /// Gets date/time when to betstop the associated market. Typically used for outrights and typically is the start-time of the event the market refers to
        /// </summary>
        /// <value>The next betstop</value>
        DateTime? NextBetstopDate => null;
    }
}

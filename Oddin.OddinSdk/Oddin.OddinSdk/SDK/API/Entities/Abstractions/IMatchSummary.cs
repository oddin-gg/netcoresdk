using Oddin.OddinSdk.SDK.AMQP;
using System;

namespace Oddin.OddinSdk.SDK.API.Entities.Abstractions
{
    public interface IMatchSummary
    {
        /// <summary>
        /// Name of the match
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Scheduled time of match start
        /// </summary>
        DateTime? ScheduledTime { get; }

        /// <summary>
        /// Scheduled time of match end
        /// </summary>
        DateTime? ScheduledEndTime { get; }

        /// <summary>
        /// Unique sport identifier
        /// </summary>
        URN SportId { get; }
    }
}

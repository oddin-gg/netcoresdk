using Oddin.OddsFeedSdk.AMQP;
using Oddin.OddsFeedSdk.Common;
using System;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions
{
    public interface IMatchSummary
    {
        public string Name { get; }

        public DateTime? ScheduledTime { get; }

        public DateTime? ScheduledEndTime { get; }

        public URN SportId { get; }
    }
}

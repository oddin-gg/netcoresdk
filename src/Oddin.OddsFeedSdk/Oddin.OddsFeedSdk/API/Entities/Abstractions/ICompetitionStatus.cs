using System.Collections.Generic;
using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions
{
    public interface ICompetitionStatus
    {
        URN WinnerId { get; }

        EventStatus Status { get; }

        IReadOnlyDictionary<string, object> Properties { get; }
    }
}

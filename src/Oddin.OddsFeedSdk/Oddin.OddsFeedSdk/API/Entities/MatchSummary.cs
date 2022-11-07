using System;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Entities;

internal class MatchSummary : IMatchSummary
{
    public MatchSummary(string name, DateTime? scheduledTime, DateTime? scheduledEndTime, URN sportId)
    {
        Name = name;
        ScheduledTime = scheduledTime;
        ScheduledEndTime = scheduledEndTime;
        SportId = sportId;
    }

    public string Name { get; }

    public DateTime? ScheduledTime { get; }

    public DateTime? ScheduledEndTime { get; }

    public URN SportId { get; }
}
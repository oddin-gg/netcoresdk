using System;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Entities;

internal class FixtureChange : IFixtureChange
{
    public FixtureChange(URN sportEventId, DateTime updateTime)
    {
        SportEventId = sportEventId;
        UpdateTime = updateTime;
    }

    public URN SportEventId { get; }

    public DateTime UpdateTime { get; }
}
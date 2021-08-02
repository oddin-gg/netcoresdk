using System;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions
{
    public interface IFixtureChange
    {
        URN SportEventId { get; }

        DateTime UpdateTime { get; }
    }
}

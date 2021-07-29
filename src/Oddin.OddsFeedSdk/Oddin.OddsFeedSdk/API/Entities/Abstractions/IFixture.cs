using System;
using System.Collections.Generic;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions
{
    public interface IFixture
    {
        URN Id { get; }

        DateTime? StartTime { get; }

        IReadOnlyDictionary<string, string> ExtraInfo { get; }

        IEnumerable<ITvChannel> TvChannels { get; }
    }
}

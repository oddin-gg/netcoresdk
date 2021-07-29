using System;
using System.Collections.Generic;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.API.Entities
{
    internal class LocalizedFixture
    {
        public DateTime? StartTime { get; }
        public IDictionary<string, string> ExtraInfo { get; }
        public IEnumerable<ITvChannel> TvChannels { get; }

        public LocalizedFixture(DateTime? startTime, IDictionary<string, string> extraInfo, IEnumerable<ITvChannel> tvChannels)
        {
            StartTime = startTime;
            ExtraInfo = extraInfo;
            TvChannels = tvChannels;
        }
    }
}

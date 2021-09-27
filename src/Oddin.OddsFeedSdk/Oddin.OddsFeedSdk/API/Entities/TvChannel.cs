using System;
using System.Globalization;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.API.Entities
{
    internal class TvChannel : ITvChannel
    {
        public string Name { get; }
        public string StreamUrl { get; }
        public DateTime? StartTime { get; }
        public CultureInfo Language { get; } 

        public TvChannel(string name, DateTime? startTime, string streamUrl, CultureInfo cultureInfo)
        {
            Name = name;
            StreamUrl = streamUrl;
            StartTime = startTime;
            Language = cultureInfo;
        }
    }
}

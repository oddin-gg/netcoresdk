using Oddin.OddsFeedSdk.AMQP;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions
{ 
    public interface ISportEvent
    {
        public URN Id { get; }

        public Task<string> GetNameAsync(CultureInfo culture);

        public Task<URN> GetSportIdAsync();

        public Task<DateTime?> GetScheduledTimeAsync();

        public Task<DateTime?> GetScheduledEndTimeAsync();
    }
}

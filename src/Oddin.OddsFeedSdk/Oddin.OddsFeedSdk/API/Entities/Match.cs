using System;
using System.Globalization;
using System.Threading.Tasks;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Entities
{
    internal class Match : IMatch
    {
        public URN Id => throw new NotImplementedException();

        public Task<string> GetNameAsync(CultureInfo culture) => throw new NotImplementedException();

        public Task<DateTime?> GetScheduledEndTimeAsync() => throw new NotImplementedException();

        public Task<DateTime?> GetScheduledTimeAsync() => throw new NotImplementedException();

        public Task<URN> GetSportIdAsync() => throw new NotImplementedException();
    }
}

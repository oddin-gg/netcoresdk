using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdk.API.Entities
{
    // TOOD: Implement
    internal class Tournament : ITournament
    {
        public URN Id => throw new NotImplementedException();

        public Task<string> GetNameAsync(CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public Task<DateTime?> GetScheduledEndTimeAsync()
        {
            throw new NotImplementedException();
        }

        public Task<DateTime?> GetScheduledTimeAsync()
        {
            throw new NotImplementedException();
        }

        public Task<URN> GetSportIdAsync()
        {
            throw new NotImplementedException();
        }

        public Tournament()
        {
        }
    }
}

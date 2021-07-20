using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdk.API
{
    internal class SportDataProvider : ISportDataProvider
    {
        private readonly IApiClient _apiClient;

        internal SportDataProvider(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public Task<ISport> GetSportAsync(URN id, CultureInfo culture = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ISport>> GetSportsAsync(CultureInfo culture = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<URN>> GetTournamentsAsync(URN id, CultureInfo culture = null)
        {
            throw new NotImplementedException();
        }
    }
}

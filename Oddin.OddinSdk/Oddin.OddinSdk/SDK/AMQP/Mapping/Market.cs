using Oddin.OddinSdk.SDK.AMQP.Abstractions;
using Oddin.OddinSdk.SDK.API.Abstractions;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping
{
    internal class Market : IMarket
    {
        private readonly IApiClient _apiClient;

        public int Id { get; }

        public IReadOnlyDictionary<string, string> Specifiers { get; }

        public Market(int id, IDictionary<string, string> specifiers, IApiClient apiClient)
        {
            Id = id;
            Specifiers = specifiers as IReadOnlyDictionary<string, string>;
            _apiClient = apiClient;
        }

        public Task<string> GetNameAsync(CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}

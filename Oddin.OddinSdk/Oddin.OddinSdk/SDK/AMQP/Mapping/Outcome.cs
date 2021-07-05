using Oddin.OddinSdk.SDK.AMQP.Abstractions;
using Oddin.OddinSdk.SDK.API.Abstractions;
using System.Globalization;
using System.Threading.Tasks;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping
{
    internal class Outcome : IOutcome
    {
        private readonly IApiClient _apiClient;

        public string Id { get; }

        public Outcome(string id, IApiClient apiClient)
        {
            Id = id;
            _apiClient = apiClient;
        }

        public Task<string> GetNameAsync(CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}

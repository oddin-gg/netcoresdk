using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Abstractions;
using System.Globalization;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdk.AMQP.Mapping
{
    internal class Outcome : IOutcome
    {
        private readonly IApiClient _apiClient;

        public long Id { get; }

        public long? RefId { get; }

        public Outcome(long id, long? refId, IApiClient apiClient)
        {
            Id = id;
            RefId = refId;
            _apiClient = apiClient;
        }

        public async Task<string> GetNameAsync(CultureInfo culture)
        {
            var marketDescriptions = await _apiClient.GetMarketDescriptionsAsync(culture);

            foreach (var marketDescription in marketDescriptions.market)
                foreach (var outcomeDescription in marketDescription.outcomes)
                    if (outcomeDescription.id == Id)
                        return outcomeDescription.name;

            return null;
        }
    }
}

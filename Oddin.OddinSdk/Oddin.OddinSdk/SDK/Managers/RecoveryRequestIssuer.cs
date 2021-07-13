using Oddin.OddinSdk.SDK.AMQP;
using Oddin.OddinSdk.SDK.API.Abstractions;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.FeedConfiguration;
using Oddin.OddinSdk.SDK.Managers.Abstractions;
using System;
using System.Threading.Tasks;

namespace Oddin.OddinSdk.SDK.Managers
{
    internal class RecoveryRequestIssuer : IEventRecoveryRequestIssuer
    {
        private readonly IApiClient _apiClient;
        private readonly int _nodeId;

        public RecoveryRequestIssuer(IApiClient apiClient, IOddsFeedConfiguration config)
        {
            if (apiClient is null)
                throw new ArgumentNullException(nameof(apiClient));

            if (config is null)
                throw new ArgumentNullException(nameof(config));

            _apiClient = apiClient;

            // TODO: load node id from config !!!!!!!!!!!!!!!!!!!!!!
            _nodeId = 0;
        }

        public Task<long> RecoverEventMessagesAsync(IProducer producer, URN eventId)
        {
            throw new System.NotImplementedException();
        }

        public Task<long> RecoverEventStatefulMessagesAsync(IProducer producer, URN eventId)
        {
            throw new System.NotImplementedException();
        }
    }
}

using Oddin.OddinSdk.SDK.AMQP;
using Oddin.OddinSdk.SDK.API.Abstractions;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.Configuration.Abstractions;
using Oddin.OddinSdk.SDK.Managers.Abstractions;
using System;
using System.Threading.Tasks;

namespace Oddin.OddinSdk.SDK.Managers
{
    internal class RecoveryRequestIssuer : IEventRecoveryRequestIssuer
    {
        private readonly IRequestIdFactory _requestIdFactory;
        private readonly IApiClient _apiClient;
        private readonly int _nodeId;

        public RecoveryRequestIssuer(IRequestIdFactory requestIdFactory, IApiClient apiClient, IFeedConfiguration config)
        {
            if (requestIdFactory is null)
                throw new ArgumentNullException(nameof(requestIdFactory));

            if (apiClient is null)
                throw new ArgumentNullException(nameof(apiClient));

            if (config is null)
                throw new ArgumentNullException(nameof(config));

            _requestIdFactory = requestIdFactory;
            _apiClient = apiClient;
            _nodeId = config.NodeId;
        }

        public async Task<long> RecoverEventMessagesAsync(IProducer producer, URN eventId)
        {
            return await _apiClient.PostEventRecoveryRequest(producer.Name, eventId);
        }

        public async Task<long> RecoverEventStatefulMessagesAsync(IProducer producer, URN eventId)
        {
            return await _apiClient.PostEventStatefulRecoveryRequest(producer.Name, eventId);
        }
    }
}

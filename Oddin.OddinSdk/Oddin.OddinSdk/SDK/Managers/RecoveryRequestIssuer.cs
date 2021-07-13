using Microsoft.Extensions.Logging;
using Oddin.OddinSdk.Common;
using Oddin.OddinSdk.Common.Exceptions;
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
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(RecoveryRequestIssuer));

        private readonly IRequestIdFactory _requestIdFactory;
        private readonly IApiClient _apiClient;
        private readonly int _nodeId;
        private readonly ExceptionHandlingStrategy _exceptionHandlingStrategy;

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
            _exceptionHandlingStrategy = config.ExceptionHandlingStrategy;
        }

        public async Task<long> RecoverEventMessagesAsync(IProducer producer, URN eventId)
        {
            try
            {
                return await _apiClient.PostEventRecoveryRequest(producer.Name, eventId);
            }
            catch (SdkException e)
            {
                e.HandleAccordingToStrategy(GetType().Name, _log, _exceptionHandlingStrategy);
            }
            return default;
        }

        public async Task<long> RecoverEventStatefulMessagesAsync(IProducer producer, URN eventId)
        {
            try
            {
                return await _apiClient.PostEventStatefulRecoveryRequest(producer.Name, eventId);
            }
            catch (SdkException e)
            {
                e.HandleAccordingToStrategy(GetType().Name, _log, _exceptionHandlingStrategy);
            }
            return default;
        }
    }
}

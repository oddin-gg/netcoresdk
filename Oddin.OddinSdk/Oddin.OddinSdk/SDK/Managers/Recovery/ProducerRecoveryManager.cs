﻿using Oddin.OddinSdk.SDK.AMQP.Messages;
using Oddin.OddinSdk.SDK.API.Abstractions;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.Managers.Abstractions;
using System;

namespace Oddin.OddinSdk.SDK.Managers.Recovery
{
    internal class ProducerRecoveryManager
    {
        private readonly IProducer _producer;
        private readonly IApiClient _apiClient;
        private readonly IRequestIdFactory _requestIdFactory;

        public ProducerRecoveryManager(IProducer producer, IApiClient apiClient, IRequestIdFactory requestIdFactory)
        {
            if (producer is null)
                throw new ArgumentNullException(nameof(producer));

            if (apiClient is null)
                throw new ArgumentNullException(nameof(apiClient));

            if (requestIdFactory is null)
                throw new ArgumentNullException(nameof(requestIdFactory));

            _producer = producer;
            _apiClient = apiClient;
            _requestIdFactory = requestIdFactory;
        }

        public bool DidIssueRequest(long requestId)
        {
            var id = _producer?.RecoveryInfo?.RequestId;

            if (id.HasValue)
                return id.Value == requestId;
            
            return false;
        }

        public bool MatchesProducer(int producerId)
        {
            return _producer.Id == producerId;
        }

        public void Open()
        {
            // TODO: start timer etc.
        }

        public void Close()
        {
            // ???
        }

        public void HandleSnapshotCompletedReceived(snapshot_complete message)
        {
            // TODO: implement (check the request id!)
        }

        public void HandleAliveReceived(alive message)
        {
            // TODO: implement (check the request id!)
        }


        //private async Task RequestFullOddsRecoveryAsync(IProducer producer, int nodeId)
        //{
        //    await _apiClient.PostRecoveryRequest(producer.Name, _requestIdFactory.GetNext(), nodeId);
        //}

        //private async Task RequestRecoveryAfterTimestampAsync(IProducer producer, DateTime dateAfter, int nodeId)
        //{
        //    await _apiClient.PostRecoveryRequest(producer.Name, _requestIdFactory.GetNext(), nodeId, dateAfter);
        //}
    }
}

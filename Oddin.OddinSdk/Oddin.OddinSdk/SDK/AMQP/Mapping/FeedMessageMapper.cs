using Oddin.OddinSdk.Common;
using Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions;
using Oddin.OddinSdk.SDK.AMQP.Messages;
using Oddin.OddinSdk.SDK.API.Abstractions;
using Oddin.OddinSdk.SDK.API.Entities;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.FeedConfiguration;
using Oddin.OddinSdk.SDK.Managers.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping
{
    internal class FeedMessageMapper : IFeedMessageMapper
    {
        private readonly IApiClient _apiClient;
        private readonly IProducerManager _producerManager;

        public FeedMessageMapper(IApiClient apiClient, IProducerManager producerManager)
        {
            _apiClient = apiClient;
            _producerManager = producerManager;
        }

        public IOddsChange<T> MapOddsChange<T>(odds_change message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent
        {
            throw new NotImplementedException();

            //return new OddsChange<T>(
            //    _producerManager.Get(message.product),
            //    new MessageTimestamp(message.GeneratedAt, message.SentAt, message.ReceivedAt, DateTime.UtcNow.ToEpochTimeMilliseconds()),
            //    new SportEvent(new URN(message.event_id), _apiClient),
            //    message.request_idSpecified ? (long?)message.request_id : null,
            //    rawMessage,
            //    ...
            //    )
        }
    }
}

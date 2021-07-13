using Microsoft.Extensions.Logging;
using Oddin.OddinSdk.Common;
using Oddin.OddinSdk.SDK.AMQP.Abstractions;
using Oddin.OddinSdk.SDK.AMQP.Enums;
using Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions;
using Oddin.OddinSdk.SDK.AMQP.Messages;
using Oddin.OddinSdk.SDK.API.Abstractions;
using Oddin.OddinSdk.SDK.API.Entities;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.Configuration.Abstractions;
using Oddin.OddinSdk.SDK.Managers.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping
{
    internal class FeedMessageMapper : IFeedMessageMapper
    {
        private readonly IApiClient _apiClient;
        private readonly IProducerManager _producerManager;
        private readonly ExceptionHandlingStrategy _exceptionHandlingStrategy;
        private readonly ILoggerFactory _loggerFactory;

        public const MarketStatus DEFAULT_MARKET_STATUS = MarketStatus.SUSPENDED;

        public FeedMessageMapper(IApiClient apiClient, IProducerManager producerManager, ExceptionHandlingStrategy exceptionHandlingStrategy, ILoggerFactory loggerFactory)
        {
            if (apiClient is null)
                throw new ArgumentNullException(nameof(apiClient));

            if (producerManager is null)
                throw new ArgumentNullException(nameof(producerManager));

            _apiClient = apiClient;
            _producerManager = producerManager;
            _exceptionHandlingStrategy = exceptionHandlingStrategy;
            _loggerFactory = loggerFactory;
        }

        private IDictionary<string, string> GetSpecifiers(string specifiersString)
        {
            var result = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(specifiersString))
                return new ReadOnlyDictionary<string, string>(result);

            var splitSpecifiers = specifiersString.Split("|");
            if (splitSpecifiers is null || splitSpecifiers.Length == 0)
                return new ReadOnlyDictionary<string, string>(result);

            foreach (var keyValueString in splitSpecifiers)
            {
                var splitKeyValue = keyValueString.Split("=", StringSplitOptions.RemoveEmptyEntries);
                if (splitKeyValue.Length != 2)
                    throw new FormatException($"Incorrect format of the specifier {keyValueString}! It needs to consist a key and a value separated by \"=\"");
                
                var key = splitKeyValue[0].Trim();
                if (string.IsNullOrEmpty(key))
                    throw new FormatException($"Incorrect format of the specifier {keyValueString}! The key must not be empty!");
                
                var value = splitKeyValue[1].Trim();
                if (string.IsNullOrEmpty(value))
                    throw new FormatException($"Incorrect format of the specifier {keyValueString}! The value must not be empty!");

                if (result.ContainsKey(key))
                    throw new ArgumentException($"There are two identical keys in specifiers: {specifiersString}, that should never happen!");

                result[key] = value;
            }

            return new ReadOnlyDictionary<string, string>(result);
        }

        private IAdditionalProbabilities GetAdditionalProbabilities(oddsChangeMarketOutcome outcome)
        {
            if (outcome is null)
                throw new ArgumentNullException(nameof(outcome));

            if (outcome.win_probabilitiesSpecified == false
                && outcome.lose_probabilitiesSpecified == false
                && outcome.half_win_probabilitiesSpecified == false
                && outcome.half_lose_probabilitiesSpecified == false
                && outcome.refund_probabilitiesSpecified == false)
            {
                return null;
            }

            return new AdditionalProbabilities(
                outcome.win_probabilitiesSpecified ? outcome.win_probabilities : default,
                outcome.lose_probabilitiesSpecified ? outcome.lose_probabilities : default,
                outcome.half_win_probabilitiesSpecified ? outcome.half_win_probabilities : default,
                outcome.half_lose_probabilitiesSpecified ? outcome.half_lose_probabilities : default,
                outcome.refund_probabilitiesSpecified ? outcome.refund_probabilities : default);
        }

        private IOutcomeOdds GetOutcomeOdds(oddsChangeMarketOutcome outcome)
        {
            if (outcome is null)
                throw new ArgumentNullException(nameof(outcome));

            return new OutcomeOdds(
                outcome.oddsSpecified ? outcome.odds : default,
                outcome.activeSpecified ? outcome.active == 1 : default,
                outcome.probabilitiesSpecified ? outcome.probabilities : default,
                GetAdditionalProbabilities(outcome),
                outcome.id,
                _apiClient);
        }

        private IMarketWithOdds GetMarketWithOdds(oddsChangeMarket oddsChangeMarket)
        {
            if (oddsChangeMarket is null)
                throw new ArgumentNullException(nameof(oddsChangeMarket));

            var marketStatus = oddsChangeMarket.statusSpecified
                    ? EnumParsingHelper.GetEnumFromInt<MarketStatus>(oddsChangeMarket.status)
                    : DEFAULT_MARKET_STATUS;

            return new MarketWithOdds(
                oddsChangeMarket.id,
                GetSpecifiers(oddsChangeMarket.specifiers),
                _apiClient,
                _exceptionHandlingStrategy,
                _loggerFactory,
                marketStatus,
                oddsChangeMarket.favouriteSpecified && oddsChangeMarket.favourite == 1,
                oddsChangeMarket.outcome?.Select(outcome => GetOutcomeOdds(outcome)),
                new MarketMetadata(oddsChangeMarket.market_metadata));
        }

        private int? GetBettingStatus(odds_changeOdds odds)
        {
            if (odds is null)
                return null;

            return odds.betting_statusSpecified
                ? (int?)odds.betting_status
                : null;
        }

        private int? GetBetStopReason(odds_changeOdds odds)
        {
            if (odds is null)
                return null;

            return odds.betstop_reasonSpecified
                ? (int?)odds.betstop_reason
                : null;
        }

        public IOddsChange<T> MapOddsChange<T>(odds_change message, byte[] rawMessage)
            where T : ISportEvent
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            var messageTimestamp = new MessageTimestamp(message.GeneratedAt, message.SentAt, message.ReceivedAt, DateTime.UtcNow.ToEpochTimeMilliseconds());
            ISportEvent sportEvent = new SportEvent(new URN(message.event_id), _apiClient, _exceptionHandlingStrategy, _loggerFactory);

            return new OddsChange<T>(
                _producerManager.Get(message.product),
                messageTimestamp,
                (T)sportEvent,
                message.request_idSpecified ? (long?)message.request_id : null,
                rawMessage,
                message.odds?.market?.Select(market => GetMarketWithOdds(market)),
                GetBetStopReason(message.odds),
                GetBettingStatus(message.odds));
        }

        public IBetStop<T> MapBetStop<T>(bet_stop message, byte[] rawMessage)
            where T : ISportEvent
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            var messageTimestamp = new MessageTimestamp(message.GeneratedAt, message.SentAt, message.ReceivedAt, DateTime.UtcNow.ToEpochTimeMilliseconds());
            ISportEvent sportEvent = new SportEvent(new URN(message.event_id), _apiClient, _exceptionHandlingStrategy, _loggerFactory);
            var marketStatus = message.market_statusSpecified
                    ? EnumParsingHelper.GetEnumFromInt<MarketStatus>(message.market_status)
                    : DEFAULT_MARKET_STATUS;

            return new BetStop<T>(
                marketStatus,
                message.groups?.Split("|"),
                _producerManager.Get(message.product),
                messageTimestamp,
                (T)sportEvent,
                message.request_idSpecified ? (long?)message.request_id : null,
                rawMessage);
        }
    }
}

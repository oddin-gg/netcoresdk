using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.AMQP.Messages;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Managers.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping
{
    internal class FeedMessageMapper : IFeedMessageMapper
    {
        private readonly IProducerManager _producerManager;
        private readonly IMarketDescriptionFactory _marketDescriptionFactory;
        private readonly ISportDataBuilder _sportDataBuilder;
        private readonly IFeedConfiguration _configuration;

        public const MarketStatus DEFAULT_MARKET_STATUS = MarketStatus.SUSPENDED;

        public FeedMessageMapper(
            IProducerManager producerManager,
            IFeedConfiguration configuration,
            IMarketDescriptionFactory marketDescriptionFactory,
            ISportDataBuilder sportDataBuilder)
        {
            _producerManager = producerManager;
            _marketDescriptionFactory = marketDescriptionFactory;
            _sportDataBuilder = sportDataBuilder;
            _configuration = configuration;
        }

        private IReadOnlyDictionary<string, string> GetSpecifiers(string specifiersString)
        {
            var result = new SortedDictionary<string, string>();
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

            return result;
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

        private IOutcomeOdds GetOutcomeOdds(
            oddsChangeMarketOutcome outcome,
            int marketId,
            IReadOnlyDictionary<string, string> marketSpecifiers,
            ISportEvent sportEvent)
        {
            if (outcome is null)
                throw new ArgumentNullException(nameof(outcome));

            return new OutcomeOdds(
                outcome.oddsSpecified ? outcome.odds : default,
                outcome.activeSpecified ? outcome.active == 1 : default,
                outcome.probabilitiesSpecified ? outcome.probabilities : default,
                GetAdditionalProbabilities(outcome),
                outcome.id,
                outcome.refid,
                _marketDescriptionFactory,
                _configuration,
                marketId,
                marketSpecifiers,
                sportEvent
            );
        }

        private IMarketWithOdds GetMarketWithOdds(oddsChangeMarket oddsChangeMarket, ISportEvent sportEvent)
        {
            if (oddsChangeMarket is null)
                throw new ArgumentNullException(nameof(oddsChangeMarket));

            var marketStatus = oddsChangeMarket.statusSpecified
                    ? EnumParsingHelper.GetEnumFromInt<MarketStatus>(oddsChangeMarket.status)
                    : DEFAULT_MARKET_STATUS;

            var specifiers = GetSpecifiers(oddsChangeMarket.specifiers);

            return new MarketWithOdds(
                oddsChangeMarket.id,
                oddsChangeMarket.refid,
                specifiers,
                oddsChangeMarket.extended_specifiers,
                oddsChangeMarket.groups?.Split("|"),
                _marketDescriptionFactory,
                _configuration.ExceptionHandlingStrategy,
                marketStatus,
                oddsChangeMarket.favouriteSpecified && oddsChangeMarket.favourite == 1,
                oddsChangeMarket.outcome?.Select(outcome => GetOutcomeOdds(outcome, oddsChangeMarket.id, specifiers, sportEvent)),
                sportEvent,
                new MarketMetadata(oddsChangeMarket.market_metadata));
        }

        private int? GetBettingStatus(odds_changeOdds odds)
        {
            if (odds is null)
                return null;

            return odds.betting_statusSpecified
                ? odds.betting_status
                : (int?) null;
        }

        private int? GetBetStopReason(odds_changeOdds odds)
        {
            if (odds is null)
                return null;

            return odds.betstop_reasonSpecified
                ? odds.betstop_reason
                : (int?) null;
        }

        private ISportEvent MapSportEvent(URN id, URN sport, IEnumerable<CultureInfo> cultures)
        {
            if (cultures is null || cultures.Any() == false)
                cultures = new[] { _configuration.DefaultLocale };

            switch (id?.Type)
            {
                case URN.TypeTournament:
                    if (sport is null) throw new ArgumentNullException(nameof(sport));
                    return _sportDataBuilder.BuildTournament(id, sport, cultures);
                case URN.TypeMatch:
                    return _sportDataBuilder.BuildMatch(id, cultures);
                default:
                    throw new ArgumentException($"Sport event of unknown type '{id?.Type}'");
            }
        }

        public IOddsChange<T> MapOddsChange<T>(odds_change message, IEnumerable<CultureInfo> cultures, byte[] rawMessage)
            where T : ISportEvent
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            var messageTimestamp = new MessageTimestamp(
                message.GeneratedAt,
                message.SentAt,
                message.ReceivedAt,
                Timestamp.Now());

            var sportEvent = MapSportEvent(
                string.IsNullOrEmpty(message?.event_id)
                    ? null
                    : new URN(message.event_id),
                message.GetSportURNFromRoutingKey(),
                cultures);

            return new OddsChange<T>(
                _producerManager.Get(message.product),
                messageTimestamp,
                (T)sportEvent,
                message.request_idSpecified ? (long?)message.request_id : null,
                rawMessage,
                message.odds?.market?.Select(market => GetMarketWithOdds(market, sportEvent)),
                GetBetStopReason(message.odds),
                GetBettingStatus(message.odds));
        }

        public IBetStop<T> MapBetStop<T>(bet_stop message, IEnumerable<CultureInfo> cultures, byte[] rawMessage)
            where T : ISportEvent
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            var messageTimestamp = new MessageTimestamp(
                message.GeneratedAt,
                message.SentAt,
                message.ReceivedAt,
                Timestamp.Now());

            var sportEvent = MapSportEvent(
                string.IsNullOrEmpty(message?.event_id)
                    ? null
                    : new URN(message.event_id),
                message.GetSportURNFromRoutingKey(),
                cultures);

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

        public IBetSettlement<T> MapBetSettlement<T>(bet_settlement message, IEnumerable<CultureInfo> cultures, byte[] rawMessage)
            where T : ISportEvent
        {
            if (message is null)
                throw new ArgumentNullException($"{nameof(message)}");

            var messageTimestamp = new MessageTimestamp(
                message.GeneratedAt,
                message.SentAt,
                message.ReceivedAt,
                Timestamp.Now());

            var sportEvent = MapSportEvent(
                string.IsNullOrEmpty(message?.event_id)
                    ? null
                    : new URN(message.event_id),
                message.GetSportURNFromRoutingKey(),
                cultures);

            return new BetSettlement<T>(
                messageTimestamp,
                _producerManager.Get(message.product),
                (T)sportEvent,
                message.request_idSpecified ? (long?)message.request_id : null,
                message.outcomes.Select(m => GetMarketWithResults(m, sportEvent)),
                message.certainty,
                rawMessage);
        }

        public IRollbackBetSettlement<T> MapRollbackBetSettlement<T>(rollback_bet_settlement message, IEnumerable<CultureInfo> cultures, byte[] rawMessage)
            where T : ISportEvent
        {
            if (message is null)
                throw new ArgumentNullException($"{nameof(message)}");

            var messageTimestamp = new MessageTimestamp(
                message.GeneratedAt,
                message.SentAt,
                message.ReceivedAt,
                Timestamp.Now());

            var sportEvent = MapSportEvent(
                string.IsNullOrEmpty(message?.event_id)
                    ? null
                    : new URN(message.event_id),
                message.GetSportURNFromRoutingKey(),
                cultures);

            return new RollbackBetSettlement<T>(
                messageTimestamp,
                _producerManager.Get(message.product),
                (T)sportEvent,
                message.request_idSpecified ? (long?)message.request_id : null,
                message.market.Select(m => GetRollbackBetSettlementMarket(m, sportEvent)),
                rawMessage);
        }

        private IMarket GetRollbackBetSettlementMarket(rollback_bet_settlementMarket message, ISportEvent sportEvent)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            var specifiers = GetSpecifiers(message.specifiers);

            return new Market(
                id: message.id,
                refId: 0,
                specifiers: specifiers,
                extendedSpecifiers: null,
                groups: null,
                marketDescriptionFactory: _marketDescriptionFactory,
                sportEvent: sportEvent,
                exceptionHandlingStrategy: _configuration.ExceptionHandlingStrategy
            );
        }

        public IRollbackBetCancel<T> MapRollbackBetCancel<T>(rollback_bet_cancel message, IEnumerable<CultureInfo> cultures, byte[] rawMessage)
            where T : ISportEvent
        {
            if (message is null)
                throw new ArgumentNullException($"{nameof(message)}");

            var messageTimestamp = new MessageTimestamp(
                message.GeneratedAt,
                message.SentAt,
                message.ReceivedAt,
                Timestamp.Now());

            var sportEvent = MapSportEvent(
                string.IsNullOrEmpty(message?.event_id)
                    ? null
                    : new URN(message.event_id),
                message.GetSportURNFromRoutingKey(),
                cultures);

            return new RollbackBetCancel<T>(
                messageTimestamp,
                _producerManager.Get(message.product),
                (T)sportEvent,
                message.request_idSpecified ? (long?)message.request_id : null,
                message.market.Select(m => GetRollbackBetCancelMarket(m, sportEvent)),
                rawMessage);
        }

        private IMarket GetRollbackBetCancelMarket(rollback_bet_cancelMarket message, ISportEvent sportEvent)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            var specifiers = GetSpecifiers(message.specifiers);

            return new Market(
                id: message.id,
                refId: 0,
                specifiers: specifiers,
                extendedSpecifiers: null,
                groups: null,
                marketDescriptionFactory: _marketDescriptionFactory,
                sportEvent: sportEvent,
                exceptionHandlingStrategy: _configuration.ExceptionHandlingStrategy
            );
        }

        private IMarketWithSettlement GetMarketWithResults(betSettlementMarket message, ISportEvent sportEvent)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            var marketStatus = EnumParsingHelper.GetEnumFromInt<MarketStatus>(message.status);
            var specifiers = GetSpecifiers(message.specifiers);

            return new MarketWithSettlement(
                marketStatus: marketStatus,
                marketId: message.id,
                refId: message.refid,
                specifiers: specifiers,
                extendedSpecifiers: message.extended_specifiers,
                groups: message.groups?.Split("|"),
                outcomes: GetOutcomeSettlements(message.Items, message.id, specifiers, sportEvent),
                marketDescriptionFactory: _marketDescriptionFactory,
                sportEvent: sportEvent,
                exceptionHandlingStrategy: _configuration.ExceptionHandlingStrategy,
                voidReason: message.void_reasonSpecified ? message.void_reason : default,
                voidReasonId: message.void_reason_idSpecified ? message.void_reason_id : default,
                voidReasonParams: message.void_reason_paramsSpecified ? message.void_reason_params : default
            );
        }

        private IEnumerable<IOutcomeSettlement> GetOutcomeSettlements(
            betSettlementMarketOutcome[] betSettlementMarkets,
            int marketId,
            IReadOnlyDictionary<string, string> marketSpecifiers,
            ISportEvent sportEvent)
        {
            return betSettlementMarkets.Select(b =>
                new OutcomeSettlement(
                    b.dead_heat_factorSpecified ? b.dead_heat_factor : default(double?),
                    b.id,
                    b.refid,
                    b.result,
                    b.void_factorSpecified ? b.void_factor : default(double?),
                    _marketDescriptionFactory,
                    _configuration,
                    marketId,
                    marketSpecifiers,
                    sportEvent
                )
            );
        }

        public IBetCancel<T> MapBetCancel<T>(bet_cancel message, IEnumerable<CultureInfo> cultures, byte[] rawMessage)
            where T : ISportEvent
        {
            if (message is null)
                throw new ArgumentNullException($"{nameof(message)}");

            var messageTimestamp = new MessageTimestamp(
                message.GeneratedAt,
                message.SentAt,
                message.ReceivedAt,
                Timestamp.Now());

            var sportEvent = MapSportEvent(
                string.IsNullOrEmpty(message?.event_id)
                    ? null
                    : new URN(message.event_id),
                message.GetSportURNFromRoutingKey(),
                 cultures);

            return new BetCancel<T>(
                timestamp: messageTimestamp,
                producer: _producerManager.Get(message.product),
                @event: (T)sportEvent,
                requestId: message.request_idSpecified ? (long?)message.request_id : default,
                startTime: message.start_timeSpecified ? message.start_time : default,
                endTime: message.end_timeSpecified ? message.end_time : default,
                supersededBy: message.superceded_by,
                markets: message.market.Select(m => GetMarketWithCancel(m, sportEvent)),
                rawMessage: rawMessage);
        }

        private IMarketCancel GetMarketWithCancel(bet_cancel_market message, ISportEvent sportEvent)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            return new MarketCancel(
                id: message.id,
                refId: message.refid,
                specifiers: GetSpecifiers(message.specifiers),
                extendedSpecifiers: message.extended_specifiers,
                groups: message.groups?.Split("|"),
                marketDescriptionFactory: _marketDescriptionFactory,
                sportEvent: sportEvent,
                exceptionHandlingStrategy: _configuration.ExceptionHandlingStrategy,
                voidReason: message.void_reasonSpecified ? message.void_reason : default,
                voidReasonId: message.void_reason_idSpecified ? message.void_reason_id : default,
                voidReasonParams: message.void_reason_paramsSpecified ? message.void_reason_params : default
            );
        }

        public IFixtureChange<T> MapFixtureChange<T>(fixture_change message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            var messageTimestamp = new MessageTimestamp(
                message.GeneratedAt,
                message.SentAt,
                message.ReceivedAt,
                Timestamp.Now());

            var sportEvent = MapSportEvent(
                string.IsNullOrEmpty(message?.event_id)
                    ? null
                    : new URN(message.event_id),
                message.GetSportURNFromRoutingKey(),
                cultures);

            return new FixtureChange<T>(
                timestamp: messageTimestamp,
                producer: _producerManager.Get(message.product),
                @event: (T)sportEvent,
                requestId: message.request_idSpecified ? (long?)message.request_id : null,
                changeType: message.change_typeSpecified ? message.change_type : default,
                nextLiveTime: message.next_live_timeSpecified ? (long?)message.next_live_time : null,
                startTime: message.start_time,
                rawMessage: rawMessage);
        }
    }
}
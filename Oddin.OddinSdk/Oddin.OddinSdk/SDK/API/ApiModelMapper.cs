using Oddin.OddinSdk.Common.Exceptions;
using Oddin.OddinSdk.SDK.AMQP;
using Oddin.OddinSdk.SDK.API.Entities;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.API.Models;
using System;
using System.Collections.Generic;

namespace Oddin.OddinSdk.SDK.API
{
    internal static class ApiModelMapper
    {
        
        /// <exception cref="MappingException"></exception>
        public static IBookmakerDetails MapBookmakerDetails(BookmakerDetailsModel model)
        {
            try
            {
                if (model is null)
                    throw new ArgumentNullException($"{typeof(BookmakerDetailsModel).Name} argument cannot be null!");

                return new BookmakerDetails(
                    model.expire_at,
                    model.bookmaker_id,
                    model.virtual_host);
            }
            catch (Exception e)
            {
                throw new MappingException($"An exception was thrown while mapping an object of type {model.GetType().Name} to object of type {typeof(IBookmakerDetails).Name}!", e);
            }
        }

        private static IOutcomeDescription MapOutcomeDescription(outcome_descriptionOutcome model)
        {
            if (model is null)
                throw new ArgumentNullException($"{typeof(outcome_descriptionOutcome).Name} argument cannot be null!");

            return new OutcomeDescription(
                model.id,
                model.name);
        }

        private static IMarketDescription MapMarketDescription(market_description model)
        {
            if (model is null)
                throw new ArgumentNullException($"{typeof(market_description).Name} argument cannot be null!");

            var outcomes = new List<IOutcomeDescription>();
            if (model.outcomes is null == false)
                foreach (var outcome in model.outcomes)
                    outcomes.Add(MapOutcomeDescription(outcome));

            return new MarketDescription(
                model.id,
                model.name,
                outcomes);
        }

        /// <exception cref="MappingException"></exception>
        public static List<IMarketDescription> MapMarketDescriptionsList(MarketDescriptionsModel model)
        {
            try
            {
                if (model is null)
                    throw new ArgumentNullException($"{typeof(MarketDescriptionsModel).Name} argument cannot be null!");

                if (model?.market is null)
                    throw new ArgumentException($"{typeof(MarketDescriptionsModel).Name}.{nameof(model.market)} cannot be null!");

                var result = new List<IMarketDescription>();
                foreach (var marketDescription in model.market)
                    result.Add(MapMarketDescription(marketDescription));
                return result;
            }
            catch (Exception e)
            {
                throw new MappingException($"An exception was thrown while mapping an object of type {model.GetType().Name} to object of type {typeof(List<IMarketDescription>).Name}!", e);
            }
        }

        /// <exception cref="MappingException"></exception>
        public static IMatchSummary MapMatchSummary(MatchSummaryModel model)
        {
            try
            {
                if (model is null)
                    throw new ArgumentNullException($"{typeof(MatchSummaryModel).Name} argument cannot be null!");

                if (model?.sport_event is null)
                    throw new ArgumentException($"{typeof(MatchSummaryModel).Name}.{nameof(model.sport_event)} cannot be null!");

                if (model?.sport_event?.tournament?.sport?.id is null)
                    throw new ArgumentException($"{typeof(MatchSummaryModel).Name}.{typeof(sportEvent)}.{typeof(tournament)}.{typeof(sport)}.{nameof(model.sport_event.tournament.sport.id)} or one of the classes it's contained in is null!");

                var sportEvent = model.sport_event;
                return new MatchSummary(
                    sportEvent.name,
                    sportEvent.scheduledSpecified ? (DateTime?)sportEvent.scheduled : null,
                    sportEvent.scheduled_endSpecified ? (DateTime?)sportEvent.scheduled_end : null,
                    new URN(sportEvent.tournament.sport.id));
            }
            catch (Exception e)
            {
                throw new MappingException($"An exception was thrown while mapping an object of type {model.GetType().Name} to object of type {typeof(IMatchSummary).Name}!", e);
            }
        }

        private static IProducer GetProducer(producer model)
        {
            if (model is null)
                throw new ArgumentNullException($"{typeof(producer).Name} argument cannot be null!");

            return new Producer(
                model.id,
                model.name,
                model.description,
                model.active,
                model.scope,

                // TODO: use SdkInfo (~ config) to provide maxInactivitySeconds !!!!!!!!!!!!!!!!!!
                10,

                model.stateful_recovery_window_in_minutes);
        }

        /// <exception cref="MappingException"></exception>
        public static List<IProducer> MapProducersList(ProducersModel model)
        {
            try
            {
                if (model is null)
                    throw new ArgumentNullException($"{typeof(ProducersModel).Name} argument cannot be null!");

                if (model?.producer is null)
                    throw new ArgumentException($"{typeof(ProducersModel).Name}.{nameof(model.producer)} cannot be null!");

                var result = new List<IProducer>();
                foreach (var producer in model.producer)
                    result.Add(GetProducer(producer));
                return result;
            }
            catch (Exception e)
            {
                throw new MappingException($"An exception was thrown while mapping an object of type {model.GetType().Name} to object of type {typeof(List<IProducer>).Name}!", e);
            }
        }
    }
}

using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.API.Models;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using System;
using System.Collections.Generic;
using Oddin.OddsFeedSdk.Exceptions;
using System.Linq;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API
{
    internal class ApiModelMapper : IApiModelMapper
    {
        private readonly int _maxInactivitySeconds;

        public ApiModelMapper(IFeedConfiguration config)
        {
            _maxInactivitySeconds = config.MaxInactivitySeconds;
        }
        
        public IBookmakerDetails MapBookmakerDetails(BookmakerDetailsModel model)
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

        public IMatchSummary MapMatchSummary(MatchSummaryModel model)
        {
            try
            {
                if (model is null)
                    throw new ArgumentNullException($"{typeof(MatchSummaryModel).Name} argument cannot be null!");

                if (model?.sport_event is null)
                    throw new ArgumentException($"{typeof(MatchSummaryModel).Name}.{nameof(model.sport_event)} cannot be null!");

                var sportEvent = model.sport_event;
                return new MatchSummary(
                    sportEvent.name,
                    sportEvent.scheduledSpecified ? (DateTime?)sportEvent.scheduled : null,
                    sportEvent.scheduled_endSpecified ? (DateTime?)sportEvent.scheduled_end : null,
                    string.IsNullOrEmpty(sportEvent?.tournament?.sport?.id) ? null : new URN(sportEvent.tournament.sport.id));
            }
            catch (Exception e)
            {
                throw new MappingException($"An exception was thrown while mapping an object of type {model.GetType().Name} to object of type {typeof(IMatchSummary).Name}!", e);
            }
        }

        private IProducer GetProducer(producer model)
        {
            if (model is null)
                throw new ArgumentNullException($"{typeof(producer).Name} argument cannot be null!");

            return new Producer(
                model.id,
                model.name,
                model.description,
                model.active,
                model.scope,
                _maxInactivitySeconds,
                model.stateful_recovery_window_in_minutes);
        }

        public IEnumerable<IProducer> MapProducersList(ProducersModel model)
        {
            try
            {
                if (model is null)
                    throw new ArgumentNullException($"{typeof(ProducersModel).Name} argument cannot be null!");

                if (model?.producer is null)
                    throw new ArgumentException($"{typeof(ProducersModel).Name}.{nameof(model.producer)} cannot be null!");

                return model.producer.Select(p => GetProducer(p));
            }
            catch (Exception e)
            {
                throw new MappingException($"An exception was thrown while mapping an object of type {model.GetType().Name} to object of type {typeof(List<IProducer>).Name}!", e);
            }
        }
    }
}

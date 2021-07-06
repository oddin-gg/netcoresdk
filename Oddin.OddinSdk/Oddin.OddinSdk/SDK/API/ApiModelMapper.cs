using Oddin.OddinSdk.SDK.API.Entities;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.API.Models;
using System;
using System.Collections.Generic;

namespace Oddin.OddinSdk.SDK.API
{
    internal static class ApiModelMapper
    {
        public static IBookmakerDetails MapBookmakerDetails(BookmakerDetailsModel model)
        {
            if (model is null)
                throw new ArgumentNullException($"{typeof(BookmakerDetailsModel).Name} argument cannot be null in {nameof(MapBookmakerDetails)}!");

            return new BookmakerDetails(
                model.expire_at,
                model.bookmaker_id,
                model.virtual_host);
        }

        private static IOutcomeDescription MapOutcomeDescription(outcome_descriptionOutcome model)
        {
            if (model is null)
                throw new ArgumentNullException($"{typeof(outcome_descriptionOutcome).Name} argument cannot be null in {nameof(MapOutcomeDescription)}!");

            return new OutcomeDescription(
                model.id,
                model.name);
        }

        public static IMarketDescription MapMarketDescription(market_description model)
        {
            if (model is null)
                throw new ArgumentNullException($"{typeof(market_description).Name} argument cannot be null in {nameof(MapMarketDescription)}!");

            var outcomes = new List<IOutcomeDescription>();
            if (model.outcomes is null == false)
                foreach (var outcome in model.outcomes)
                    outcomes.Add(MapOutcomeDescription(outcome));

            return new MarketDescription(
                model.id,
                model.name,
                outcomes);
        }
    }
}

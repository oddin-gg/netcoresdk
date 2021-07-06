using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.API.Models;
using System.Collections.Generic;

namespace Oddin.OddinSdk.SDK.API.Entities
{
    internal class MarketDescription : IMarketDescription
    {
        public int Id { get; }

        public string Name { get; }

        public IReadOnlyCollection<IOutcomeDescription> Outcomes { get; }

        public MarketDescription(market_description marketDescriptionsModel)
        {
            Id = marketDescriptionsModel.id;
            Name = marketDescriptionsModel.name;

            var outcomes = new List<OutcomeDescription>();
            foreach (var outcomeDescriptionModel in marketDescriptionsModel.outcomes)
            {
                outcomes.Add(new OutcomeDescription(outcomeDescriptionModel));
            }
            Outcomes = outcomes;
        }
    }
}

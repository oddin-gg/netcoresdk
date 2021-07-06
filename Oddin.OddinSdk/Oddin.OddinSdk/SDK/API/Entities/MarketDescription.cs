using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using System.Collections.Generic;

namespace Oddin.OddinSdk.SDK.API.Entities
{
    internal class MarketDescription : IMarketDescription
    {
        public int Id { get; }

        public string Name { get; }

        public IReadOnlyCollection<IOutcomeDescription> Outcomes { get; }

        public MarketDescription(int id, string name, IReadOnlyCollection<IOutcomeDescription> outcomes)
        {
            Id = id;
            Name = name;
            Outcomes = outcomes;
        }
    }
}

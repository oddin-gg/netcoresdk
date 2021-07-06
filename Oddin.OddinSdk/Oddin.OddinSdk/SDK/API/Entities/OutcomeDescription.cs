using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.API.Models;

namespace Oddin.OddinSdk.SDK.API.Entities
{
    internal class OutcomeDescription : IOutcomeDescription
    {
        public string Id { get; }

        public string Name { get; }

        public OutcomeDescription(outcome_descriptionOutcome outcomeModel)
        {
            Id = outcomeModel.id;
            Name = outcomeModel.name;
        }
    }
}

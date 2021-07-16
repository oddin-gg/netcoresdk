using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.API.Entities
{
    internal class OutcomeDescription : IOutcomeDescription
    {
        public string Id { get; }

        public string Name { get; }

        public OutcomeDescription(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}

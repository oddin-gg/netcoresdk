using System.Collections.Generic;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions
{
    public interface IMarketDescription
    {
        public int Id { get; }

        public string Name { get; }

        public IReadOnlyCollection<IOutcomeDescription> Outcomes { get; }
    }
}

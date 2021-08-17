using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.API.Entities
{
    internal class LocalizedMarketDescription : ILocalizedItem
    {
        public int RefId { get; }

        public IDictionary<long, LocalizedOutcomeDescription> Outcomes { get; }

        public IEnumerable<CultureInfo> LoadedLocals => Name.Keys;

        public IEnumerable<ISpecifier> Specifiers { get; set; }

        public IDictionary<CultureInfo, string> Name { get; } = new Dictionary<CultureInfo, string>();

        public LocalizedMarketDescription(int refId, IDictionary<long, LocalizedOutcomeDescription> outcomes)
        {
            RefId = refId;
            Outcomes = outcomes;
        }
    }
}

using System.Globalization;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.API.Entities
{
    internal class OutcomeDescription : IOutcomeDescription
    {
        private readonly LocalizedOutcomeDescription _localizedOutcomeDescription;

        public long Id { get; }

        public long? RefId => _localizedOutcomeDescription?.RefId;

        public string GetName(CultureInfo culture) => _localizedOutcomeDescription?.Name[culture];

        public string GetDescription(CultureInfo culture) => _localizedOutcomeDescription?.Description[culture];

        public OutcomeDescription(long id, LocalizedOutcomeDescription localizedOutcomeDescription)
        {
            Id = id;
            _localizedOutcomeDescription = localizedOutcomeDescription;
        }
    }
}

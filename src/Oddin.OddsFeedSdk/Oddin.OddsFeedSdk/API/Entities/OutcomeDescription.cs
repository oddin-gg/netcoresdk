using System.Globalization;
using System.Linq;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.API.Entities;

internal class OutcomeDescription : IOutcomeDescription
{
    private readonly LocalizedOutcomeDescription _localizedOutcomeDescription;

    public OutcomeDescription(string id, LocalizedOutcomeDescription localizedOutcomeDescription)
    {
        Id = id;
        _localizedOutcomeDescription = localizedOutcomeDescription;
    }

    public string Id { get; }

    public long? RefId => _localizedOutcomeDescription?.RefId;

    public string GetName(CultureInfo culture) =>
        _localizedOutcomeDescription?.Name?.FirstOrDefault(d => d.Key.Equals(culture)).Value;

    public string GetDescription(CultureInfo culture) => _localizedOutcomeDescription?.Description
        ?.FirstOrDefault(d => d.Key.Equals(culture)).Value;
}
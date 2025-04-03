using System;
using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.API.Entities;

internal class LocalizedMarketDescription : ILocalizedItem
{
    public LocalizedMarketDescription(
        int refId,
        IDictionary<string, LocalizedOutcomeDescription> outcomes,
        string includesOutcomeOfType,
        string outcomeType,
        IEnumerable<string> groups
    )
    {
        IncludesOutcomesOfType = includesOutcomeOfType;
        RefId = refId;
        Outcomes = outcomes;
        Groups = groups;

        if (outcomeType != null)
        {
            OutcomeType = outcomeType.ToUpper() switch
            {
                "PLAYER" => AMQP.Enums.OutcomeType.Player,
                "COMPETITOR" => AMQP.Enums.OutcomeType.Competitor,
                _ => null
            };
        }
    }

    [Obsolete("Do not use this field, it will be removed in future.")]
    public int RefId { get; }

    public IDictionary<string, LocalizedOutcomeDescription> Outcomes { get; }

    public IEnumerable<ISpecifier> Specifiers { get; set; }

    public string IncludesOutcomesOfType { get; set; }

    public OutcomeType? OutcomeType { get; set; }

    public IDictionary<CultureInfo, string> Name { get; } = new Dictionary<CultureInfo, string>();

    public IEnumerable<CultureInfo> LoadedLocals => Name.Keys;

    public IEnumerable<string> Groups { get; }
}

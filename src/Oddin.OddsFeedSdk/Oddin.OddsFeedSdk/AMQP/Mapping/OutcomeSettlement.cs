using System.Collections.Generic;
using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Configuration.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping;

internal class OutcomeSettlement : Outcome, IOutcomeSettlement
{
    internal OutcomeSettlement(
        double? deadHeatFactor,
        string id,
        long refId,
        int result,
        double? voidFactor,
        IMarketDescriptionFactory marketDescriptionFactory,
        IFeedConfiguration configuration,
        int marketId,
        IReadOnlyDictionary<string, string> marketSpecifiers,
        ISportEvent sportEvent)
        : base(id, refId, marketDescriptionFactory, configuration, marketId, marketSpecifiers, sportEvent)
    {
        DeadHeatFactor = deadHeatFactor;

        VoidFactor = voidFactor switch
        {
            1 => Enums.VoidFactor.One,
            0.5 => Enums.VoidFactor.Half,
            _ => null
        };

        OutcomeResult = result switch
        {
            0 => OutcomeResult.Lost,
            1 => OutcomeResult.Won,
            _ => OutcomeResult.UndecidedYet
        };
    }

    public double? DeadHeatFactor { get; }

    public VoidFactor? VoidFactor { get; }

    public OutcomeResult OutcomeResult { get; }
}
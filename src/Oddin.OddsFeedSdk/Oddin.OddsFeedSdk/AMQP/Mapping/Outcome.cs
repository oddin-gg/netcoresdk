using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Exceptions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping;

internal class Outcome : IOutcome
{
    private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(Outcome));
    private readonly IFeedConfiguration _config;

    private readonly IMarketDescriptionFactory _marketDescriptionFactory;
    private readonly int _marketId;
    private readonly IReadOnlyDictionary<string, string> _marketSpecifiers;
    private readonly ISportEvent _sportEvent;

    public Outcome(
        string id,
        long refId,
        IMarketDescriptionFactory marketDescriptionFactory,
        IFeedConfiguration config,
        int marketId,
        IReadOnlyDictionary<string, string> marketSpecifiers,
        ISportEvent sportEvent)
    {
        Id = id;
        RefId = refId;
        _marketDescriptionFactory = marketDescriptionFactory;
        _config = config;
        _marketId = marketId;
        _marketSpecifiers = marketSpecifiers;
        _sportEvent = sportEvent;
    }

    public string Id { get; }

    public long RefId { get; }

    public string GetName(CultureInfo culture)
    {
        try
        {
            var marketDescription =
                _marketDescriptionFactory.MarketDescriptionByIdAndSpecifiers(_marketId, _marketSpecifiers,
                    new[] { culture });
            var outcomeName = marketDescription?.Outcomes?.FirstOrDefault(o => o.Id == Id)?.GetName(culture);

            // market with dynamic outcomes can have also non-dynamic outcome, that's reason why outcome with outcomeID exists at first
            if (outcomeName == null && marketDescription?.OutcomeType != null)
            {
                if (marketDescription.OutcomeType == OutcomeType.Player)
                {
                    var player = _marketDescriptionFactory.PlayerCache.GetPlayer(new URN(Id), new[] { culture });
                    if (player != null && player.Name.Count > 0)
                    {
                        outcomeName = player.Name.Values.First();
                    }
                }
                else if (marketDescription.OutcomeType == OutcomeType.Competitor)
                {
                    var competitor =
                        _marketDescriptionFactory.CompetitorCache.GetCompetitor(new URN(Id), new[] { culture });
                    if (competitor != null && competitor.Name.Count > 0)
                    {
                        outcomeName = competitor.Name.Values.First();
                    }
                }
            }

            if (outcomeName is null)
                if (_config.ExceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                    throw new ItemNotFoundException(Id, "Cannot find outcome name!");
                else
                    return null;

            return MakeOutcomeName(outcomeName, culture);
        }
        catch (SdkException e)
        {
            e.HandleAccordingToStrategy(GetType().Name, _log, _config.ExceptionHandlingStrategy);
        }

        return null;
    }

    private string MakeOutcomeName(string outcomeName, CultureInfo culture) =>
        outcomeName switch
        {
            "home" => ( _sportEvent as IMatch )?.HomeCompetitor?.GetName(culture),
            "away" => ( _sportEvent as IMatch )?.AwayCompetitor?.GetName(culture),
            _ => outcomeName
        };
}
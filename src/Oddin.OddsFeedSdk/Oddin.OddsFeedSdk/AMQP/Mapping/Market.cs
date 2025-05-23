using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Exceptions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping;

internal class Market : IMarket
{
    private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(Market));
    private readonly ExceptionHandlingStrategy _exceptionHandlingStrategy;

    private readonly IMarketDescriptionFactory _marketDescriptionFactory;
    private readonly ISportEvent _sportEvent;

    public Market(
        int id,
        int refId,
        IReadOnlyDictionary<string, string> specifiers,
        string extendedSpecifiers,
        IEnumerable<string> groups,
        IMarketDescriptionFactory marketDescriptionFactory,
        ISportEvent sportEvent,
        ExceptionHandlingStrategy exceptionHandlingStrategy)
    {
        Id = id;
        RefId = refId;
        Specifiers = specifiers;
        ExtendedSpecifiers = extendedSpecifiers;
        Groups = groups;
        _marketDescriptionFactory = marketDescriptionFactory;
        _sportEvent = sportEvent;
        _exceptionHandlingStrategy = exceptionHandlingStrategy;
    }

    public int Id { get; }

    [Obsolete("Do not use this field, it will be removed in future.")]
    public int RefId { get; }

    public IReadOnlyDictionary<string, string> Specifiers { get; }

    public string ExtendedSpecifiers { get; }

    public IEnumerable<string> Groups { get; }

    public string GetName(CultureInfo culture)
    {
        try
        {
            var marketDescription =
                _marketDescriptionFactory.MarketDescriptionByIdAndSpecifiers(Id, Specifiers, new[] { culture });
            var marketName = marketDescription?.GetName(culture);

            if (marketName is null)
                if (_exceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                    throw new ItemNotFoundException(Id.ToString(), "Cannot find market name!");
                else
                    return null;

            return MakeMarketName(marketName, culture, marketDescription.Groups.ToList());
        }
        catch (SdkException e)
        {
            e.HandleAccordingToStrategy(GetType().Name, _log, _exceptionHandlingStrategy);
        }

        return null;
    }

    private string MakeMarketName(string marketName, CultureInfo culture, ICollection<string> groups)
    {
        if (Specifiers is null || Specifiers.Any() == false)
            return marketName;

        var template = marketName;
        foreach (var specifier in Specifiers)
        {
            var key = $"{{{specifier.Key}}}";
            if (template.Contains(key) == false)
                continue;

            var value = specifier.Value switch
            {
                "home" => ( _sportEvent as IMatch )?.HomeCompetitor?.GetName(culture),
                "away" => ( _sportEvent as IMatch )?.AwayCompetitor?.GetName(culture),
                _ => specifier.Value
            };
            if (IsPropsSpecifier(value, groups))
            {
                value = GetPropsSpecifierName(value, culture);
            }

            template = template.Replace(key, value);
        }

        return template;
    }

    private static bool IsPropsSpecifier(string id, ICollection<string> groups)
    {
        if (groups is null || !groups.Contains(MarketDescriptionGroups.PlayerProps))
        {
            return false;
        }

        return URN.TryParseUrn(id, out _, out _, out _);
    }

    private string GetPropsSpecifierName(string id, CultureInfo culture)
    {
        var urn = new URN(id);
        return urn.Type switch
        {
            URN.TypePlayer => _marketDescriptionFactory.PlayerCache.GetPlayer(urn, new[] { culture }).Name[culture],
            _ => id
        };
    }
}

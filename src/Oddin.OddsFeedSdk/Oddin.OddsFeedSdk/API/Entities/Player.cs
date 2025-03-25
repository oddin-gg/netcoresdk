using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Exceptions;

namespace Oddin.OddsFeedSdk.API.Entities;

internal class Player : IPlayer
{
    private readonly IEnumerable<CultureInfo> _cultures;
    private readonly ExceptionHandlingStrategy _exceptionHandling;
    private readonly IPlayerCache _playerCache;
    private readonly ISportDataBuilder _sportDataBuilder;

    public Player(
        URN id,
        IPlayerCache playerCache,
        ISportDataBuilder sportDataBuilder,
        ExceptionHandlingStrategy exceptionHandling,
        IEnumerable<CultureInfo> cultures)
    {
        Id = id;
        _playerCache = playerCache;
        _sportDataBuilder = sportDataBuilder;
        _exceptionHandling = exceptionHandling;
        _cultures = cultures;
    }

    public URN Id { get; }

    public IReadOnlyDictionary<CultureInfo, string> Names
    {
        get
        {
            var names = FetchPlayer(_cultures)?.Name;
            if (names is not null)
                return new ReadOnlyDictionary<CultureInfo, string>(names);

            return new ReadOnlyDictionary<CultureInfo, string>(new Dictionary<CultureInfo, string>());
        }
    }

    public IReadOnlyDictionary<CultureInfo, string> FullNames
    {
        get
        {
            var countries = FetchPlayer(_cultures)?.FullName;
            if (countries is not null)
                return new ReadOnlyDictionary<CultureInfo, string>(countries);

            return new ReadOnlyDictionary<CultureInfo, string>(new Dictionary<CultureInfo, string>());
        }
    }

    public IReadOnlyDictionary<CultureInfo, string> SportIDs
    {
        get
        {
            var sportIDs = FetchPlayer(_cultures)?.SportID;
            if (sportIDs is not null)
                return new ReadOnlyDictionary<CultureInfo, string>(sportIDs);

            return new ReadOnlyDictionary<CultureInfo, string>(new Dictionary<CultureInfo, string>());
        }
    }

    public string GetName(CultureInfo culture) =>
        FetchPlayer(new[] { culture })
            ?.Name
            ?.FirstOrDefault(d => d.Key.Equals(culture))
            .Value;

    public string GetFullName(CultureInfo culture) =>
        FetchPlayer(new[] { culture })
            ?.FullName
            ?.FirstOrDefault(d => d.Key.Equals(culture))
            .Value;

    public string GetSportID(CultureInfo culture) =>
        FetchPlayer(new[] { culture })
            ?.SportID
            ?.FirstOrDefault(d => d.Key.Equals(culture))
            .Value;

    private LocalizedPlayer FetchPlayer(IEnumerable<CultureInfo> cultures)
    {
        var item = _playerCache.GetPlayer(Id, cultures);

        if (item == null && _exceptionHandling == ExceptionHandlingStrategy.THROW)
            throw new ItemNotFoundException(Id.ToString(), $"Competitor {Id} not found");
        return item;
    }
}
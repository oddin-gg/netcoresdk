using System;
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

internal class Fixture : IFixture
{
    private readonly IEnumerable<CultureInfo> _cultures;
    private readonly IFixtureCache _fixtureCache;
    private readonly ExceptionHandlingStrategy _handlingStrategy;

    public Fixture(URN id, IFixtureCache fixtureCache, ExceptionHandlingStrategy handlingStrategy,
        IEnumerable<CultureInfo> cultures)
    {
        Id = id;
        _fixtureCache = fixtureCache;
        _handlingStrategy = handlingStrategy;
        _cultures = cultures;
    }

    public URN Id { get; }

    public DateTime? StartTime => FetchFixture()?.StartTime;

    public IReadOnlyDictionary<string, string> ExtraInfo
    {
        get
        {
            var fixtures = FetchFixture()?.ExtraInfo;
            if (fixtures is not null)
                return new ReadOnlyDictionary<string, string>(fixtures);

            return new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
        }
    }

    public IEnumerable<ITvChannel> TvChannels => FetchFixture()?.TvChannels;

    private LocalizedFixture FetchFixture()
    {
        var item = _fixtureCache.GetFixture(Id, _cultures.First());

        if (item is null && _handlingStrategy == ExceptionHandlingStrategy.THROW)
            throw new ItemNotFoundException(Id.ToString(), $"Fixture with id {Id} not found");

        return item;
    }
}
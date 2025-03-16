using System;
using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Entities;

internal class LocalizedMatch : ILocalizedItem
{
    internal struct Competitor
    {
        public URN Id { get; init; }
        public string Qualifier { get; init; }
    }

    public LocalizedMatch(URN id) => Id = id;

    internal URN Id { get; }

    [Obsolete("Do not use this field, it will be removed in future.")]
    internal URN RefId { get; set; }
    internal DateTime? ScheduledTime { get; set; }
    internal DateTime? ScheduledEndTime { get; set; }
    internal URN SportId { get; set; }
    internal URN TournamentId { get; set; }
    internal IEnumerable<Competitor> Competitors { get; set; }
    internal SportFormat SportFormat { get; set; }
    internal IDictionary<CultureInfo, string> Name { get; set; } = new Dictionary<CultureInfo, string>();
    internal LiveOddsAvailability LiveOddsAvailability { get; set; }
    public IEnumerable<CultureInfo> LoadedLocals => Name.Keys;
    public IDictionary<string, string> ExtraInfo { get; set;  }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Entities
{
    internal class LocalizedMatch : ILocalizedItem
    {
        internal URN Id { get; }
        internal URN RefId { get; set; }

        internal DateTime? ScheduledTime { get; set; }
        internal DateTime? ScheduledEndTime { get; set; }
        internal URN SportId { get; set; }
        internal URN TournamentId { get; set; }
        internal URN HomeTeamId { get; set; }
        internal URN AwayTeamId { get; set; }
        internal string HomeTeamQualifier { get; set; }
        internal string AwayTeamQualifier { get; set; }

        internal IDictionary<CultureInfo, string> Name { get; set; } = new Dictionary<CultureInfo, string>();
        public IEnumerable<CultureInfo> LoadedLocals => Name.Keys;

        internal LiveOddsAvailability LiveOddsAvailability { get; set; }

        public LocalizedMatch(URN id)
        {
            Id = id;
        }
    }
}

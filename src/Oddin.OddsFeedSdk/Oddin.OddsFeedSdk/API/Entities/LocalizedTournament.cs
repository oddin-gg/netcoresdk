using System;
using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Entities
{
    internal class LocalizedTournament : ILocalizedItem
    {
        public URN Id { get; }

        public URN RefId { get; set; }

        internal IDictionary<CultureInfo, string> Name { get; set; } = new Dictionary<CultureInfo, string>();

        public IEnumerable<CultureInfo> LoadedLocals => Name.Keys;
    
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        public URN SportId { get; set; }
        
        public DateTime? ScheduledTime { get; set; }
        
        public DateTime? ScheduledEndTime { get; set; }

        public IEnumerable<URN> CompetitorIds { get; set; } = null;
        
        public LocalizedTournament(URN id)
        {
            Id = id;
        }
    }
}

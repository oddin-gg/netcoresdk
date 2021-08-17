using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Entities
{
    internal class LocalizedCompetitor : ILocalizedItem
    {
        public URN Id { get; }

        public URN RefId { get; set; }

        public URN SportId { get; set; }

        internal IDictionary<CultureInfo, string> Name { get; set; } = new Dictionary<CultureInfo, string>();

        internal IDictionary<CultureInfo, string> Abbreviation { get; set; } = new Dictionary<CultureInfo, string>();

        internal IDictionary<CultureInfo, string> Country { get; set; } = new Dictionary<CultureInfo, string>();

        public bool? IsVirtual { get; set; }

        public string CountryCode { get; set; }

        public int? Underage { get; set; }

        public IEnumerable<CultureInfo> LoadedLocals => GetLoadedLocals();

        private IEnumerable<CultureInfo> GetLoadedLocals()
        {
            var allCultures = new HashSet<CultureInfo>();

            foreach (var name in Name)
                allCultures.Add(name.Key);
            
            foreach (var abbreviation in Abbreviation)
                allCultures.Add(abbreviation.Key);
            
            foreach (var country in Country)
                allCultures.Add(country.Key);

            return allCultures;
        }

        public LocalizedCompetitor(URN id)
        {
            Id = id;
        }
    }
}

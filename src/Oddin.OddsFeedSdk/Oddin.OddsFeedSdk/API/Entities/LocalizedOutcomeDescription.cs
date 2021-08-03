using System.Collections.Generic;
using System.Globalization;

namespace Oddin.OddsFeedSdk.API.Entities
{
    internal class LocalizedOutcomeDescription
    {
        public long? RefId { get; set; }

        internal IDictionary<CultureInfo, string> Name { get; set; } = new Dictionary<CultureInfo, string>();

        internal IDictionary<CultureInfo, string> Description { get; set; } = new Dictionary<CultureInfo, string>();
    }
}

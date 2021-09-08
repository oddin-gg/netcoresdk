using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.Managers.Abstractions;
using System.Globalization;
using System.Linq;

namespace Oddin.OddsFeedSdk.AMQP.Mapping
{
    internal class Outcome : IOutcome
    {
        private readonly IMarketDescriptionManager _marketDescriptionManager;

        public long Id { get; }

        public long RefId { get; }

        public Outcome(long id, long refId, IMarketDescriptionManager marketDescriptionManager)
        {
            Id = id;
            RefId = refId;
            _marketDescriptionManager = marketDescriptionManager;
        }

        public string GetName(CultureInfo culture)
        {
            var marketDescriptions = _marketDescriptionManager.GetMarketDescriptions(culture);
            return marketDescriptions
                .FirstOrDefault(m => m.Outcomes.Any(o => o.Id == Id))
                ?.GetName(culture);    
        }
    }
}

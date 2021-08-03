using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Exceptions;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Oddin.OddsFeedSdk.API.Entities
{
    internal class MarketDescription : IMarketDescription
    {
        private readonly string _variant;
        private readonly IMarketDescriptionCache _marketDescriptionCache;
        private readonly ExceptionHandlingStrategy _handlingStrategy;
        private readonly IEnumerable<CultureInfo> _cultures;

        public int Id { get; }

        public int? RefId => FetchMarketDescription(_cultures)?.RefId;

        public IEnumerable<IOutcomeDescription> Outcomes
            => FetchMarketDescription(_cultures)?.Outcomes?
                .Select(o => new OutcomeDescription(o.Key, o.Value))
                ?? new List<OutcomeDescription>();

        public IEnumerable<ISpecifier> Specifiers => FetchMarketDescription(_cultures)?.Specifiers;

        public string OutcomeType => _variant;

        public MarketDescription(
            int id,
            string variant,
            IMarketDescriptionCache marketDescriptionCache,
            ExceptionHandlingStrategy handlingStrategy,
            IEnumerable<CultureInfo> cultures)
        {
            Id = id;
            _variant = variant;
            _marketDescriptionCache = marketDescriptionCache;
            _handlingStrategy = handlingStrategy;
            _cultures = cultures;
        }

        public string GetName(CultureInfo culture)
            => FetchMarketDescription(new[] { culture })?.Name?.FirstOrDefault(d => d.Key == culture).Value;

        private LocalizedMarketDescription FetchMarketDescription(IEnumerable<CultureInfo> cultures)
        {
            var item = _marketDescriptionCache.GetMarketDescription(Id, _variant, cultures);

            if (item == null && _handlingStrategy == ExceptionHandlingStrategy.THROW)
                throw new ItemNotFoundException(Id.ToString(), "Market description not found");
            else
                return item;
        }
    }
}

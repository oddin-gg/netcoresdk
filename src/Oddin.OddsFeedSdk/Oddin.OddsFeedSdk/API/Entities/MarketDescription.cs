using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Exceptions;
using System.Collections.Generic;
using System.Globalization;

namespace Oddin.OddsFeedSdk.API.Entities
{
    internal class MarketDescription : IMarketDescription
    {
        private readonly string _variant;
        private readonly IMarketDescriptionCache _marketDescriptionCache;
        private readonly ExceptionHandlingStrategy _handlingStrategy;
        private readonly IEnumerable<CultureInfo> _cultures;

        public int Id { get; }

        public int? RefId { get; }

        public IEnumerable<IOutcomeDescription> Outcomes { get; }

        public IEnumerable<ISpecifier> Specifiers => throw new System.NotImplementedException(); // TODO: Implement

        public string OutcomeType => throw new System.NotImplementedException();

        public IEnumerable<string> Groups => throw new System.NotImplementedException();

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

        public string GetName(CultureInfo culture) => throw new System.NotImplementedException();

        private LocalizedMarketDescription FetchMarketDescription( IEnumerable<CultureInfo> cultures)
        {
            var item = _marketDescriptionCache.GetMarketDescription(Id, _variant, cultures);

            if (item == null && _handlingStrategy == ExceptionHandlingStrategy.THROW)
                throw new ItemNotFoundException(Id.ToString(), "Market description not found");
            else
                return item;

        }
    }
}

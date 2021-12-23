using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Oddin.OddsFeedSdk.Exceptions;
using Oddin.OddsFeedSdk.API.Abstractions;
using System.Collections.ObjectModel;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping
{
    internal class Market : IMarket
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(Market));

        private readonly IMarketDescriptionFactory _marketDescriptionFactory;
        private readonly ISportEvent _sportEvent;
        private readonly ExceptionHandlingStrategy _exceptionHandlingStrategy;

        public int Id { get; }

        public int RefId { get; }

        public IReadOnlyDictionary<string, string> Specifiers { get; }

        public string ExtendedSpecifiers { get; }

        public IEnumerable<string> Groups { get; }

        public Market(
            int id,
            int refId,
            IReadOnlyDictionary<string, string> specifiers,
            string extendedSpecifiers,
            IEnumerable<string> groups,
            IMarketDescriptionFactory marketDescriptionFactory,
            ISportEvent sportEvent,
            ExceptionHandlingStrategy exceptionHandlingStrategy)
        {
            Id = id;
            RefId = refId;
            Specifiers = specifiers;
            ExtendedSpecifiers = extendedSpecifiers;
            Groups = groups;
            _marketDescriptionFactory = marketDescriptionFactory;
            _sportEvent = sportEvent;
            _exceptionHandlingStrategy = exceptionHandlingStrategy;
        }

        public string GetName(CultureInfo culture)
        {
            try
            {
                var marketDescription = _marketDescriptionFactory.GetMarketDescription(Id, Specifiers, new[] { culture });
                var marketName = marketDescription?.GetName(culture);

                if (marketName is null)
                    if (_exceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                        throw new ItemNotFoundException(Id.ToString(), "Cannot find market name!");
                    else
                        return null;

                return MakeMarketName(marketName, culture);
            }
            catch (SdkException e)
            {
                e.HandleAccordingToStrategy(GetType().Name, _log, _exceptionHandlingStrategy);
            }
            return null;
        }

        private string MakeMarketName(string marketName, CultureInfo culture)
        {
            if (Specifiers is null || Specifiers.Any() == false)
                return marketName;

            var template = marketName;
            foreach(var specifier in Specifiers)
            {
                var key = $"{{{specifier.Key}}}";
                if (template.Contains(key) == false)
                    continue;

                var value = specifier.Value switch
                {
                    "home" => (_sportEvent as IMatch)?.HomeCompetitor?.GetName(culture),
                    "away" => (_sportEvent as IMatch)?.AwayCompetitor?.GetName(culture),
                    _ => specifier.Value
                };

                template = template.Replace(key, value);
            }

            return template;
        }
    }
}
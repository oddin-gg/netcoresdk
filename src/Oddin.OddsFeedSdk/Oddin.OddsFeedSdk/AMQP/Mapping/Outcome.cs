using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Exceptions;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Oddin.OddsFeedSdk.AMQP.Mapping
{
    internal class Outcome : IOutcome
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(Outcome));

        private readonly IMarketDescriptionFactory _marketDescriptionFactory;
        private readonly IFeedConfiguration _configuration;
        private readonly int _marketId;
        private readonly IDictionary<string, string> _marketSpecifiers;
        private readonly ISportEvent _sportEvent;

        public long Id { get; }

        public long RefId { get; }

        public Outcome(
            long id,
            long refId,
            IMarketDescriptionFactory marketDescriptionFactory,
            IFeedConfiguration configuration,
            int marketId,
            IDictionary<string, string> marketSpecifiers,
            ISportEvent sportEvent)  
        {
            Id = id;
            RefId = refId;
            _marketDescriptionFactory = marketDescriptionFactory;
            _configuration = configuration;
            _marketId = marketId;
            _marketSpecifiers = marketSpecifiers;
            _sportEvent = sportEvent;
        }

        public string GetName(CultureInfo culture)
        {
            try
            {
                var marketDescription = _marketDescriptionFactory.GetMarketDescription(_marketId, _marketSpecifiers, new[] { culture });
                var outcomeName = marketDescription?.Outcomes?.FirstOrDefault(o => o.Id == Id)?.GetName(culture);

                if (outcomeName is null)
                    if (_configuration.ExceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                        throw new ItemNotFoundException(Id.ToString(), "Cannot find outcome name!");
                    else
                        return null;

                return MakeOutcomeName(outcomeName, culture);
            }
            catch (SdkException e)
            {
                e.HandleAccordingToStrategy(GetType().Name, _log, _configuration.ExceptionHandlingStrategy);
            }
            return null;
        }

        private string MakeOutcomeName(string outcomeName, CultureInfo culture)
        {
            return outcomeName switch
            {
                "home" => (_sportEvent as IMatch)?.HomeCompetitor?.GetName(culture),
                "away" => (_sportEvent as IMatch)?.AwayCompetitor?.GetName(culture),
                _ => outcomeName
            };
        }
    }
}

using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Oddin.OddsFeedSdk.Exceptions;
using Oddin.OddsFeedSdk.Managers.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping
{
    internal class Market : IMarket
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(Market));

        private readonly ExceptionHandlingStrategy _exceptionHandlingStrategy;
        private readonly IMarketDescriptionManager _marketDescriptionManager;

        public int Id { get; }

        public int RefId { get; }

        public IReadOnlyDictionary<string, string> Specifiers { get; }

        public string ExtendedSpecifiers { get; }

        public IEnumerable<string> Groups { get; }

        public Market(
            int id,
            int refId,
            IDictionary<string, string> specifiers,
            string extendedSpecifiers,
            IEnumerable<string> groups,
            IMarketDescriptionManager marketDescriptionManager,
            ExceptionHandlingStrategy exceptionHandlingStrategy)
        {
            if (specifiers is null)
                throw new ArgumentNullException(nameof(specifiers));

            if (marketDescriptionManager is null)
                throw new ArgumentNullException(nameof(marketDescriptionManager));

            Id = id;
            RefId = refId;
            Specifiers = specifiers as IReadOnlyDictionary<string, string>;
            ExtendedSpecifiers = extendedSpecifiers;
            Groups = groups;

            _exceptionHandlingStrategy = exceptionHandlingStrategy;
            _marketDescriptionManager = marketDescriptionManager;
        }

        public string GetName(CultureInfo culture)
        {
            try
            {
                var marketDescriptions =  _marketDescriptionManager.GetMarketDescriptions(culture);
                return marketDescriptions
                    .FirstOrDefault(m => m.Id == Id)?.GetName(culture);
            }
            catch (SdkException e)
            {
                e.HandleAccordingToStrategy(GetType().Name, _log, _exceptionHandlingStrategy);
            }
            return null;
        }
    }
}

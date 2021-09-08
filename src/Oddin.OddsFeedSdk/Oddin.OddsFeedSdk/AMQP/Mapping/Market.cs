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
using Oddin.OddsFeedSdk.API.Abstractions;
using System.Collections.ObjectModel;

namespace Oddin.OddsFeedSdk.AMQP.Mapping
{
    internal class Market : IMarket
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(Market));
        private readonly IMarketDescriptionFactory _marketDescriptionFactory;
        private readonly ExceptionHandlingStrategy _exceptionHandlingStrategy;

        public int Id { get; }

        public int RefId { get; }

        private readonly IDictionary<string, string> _specifiers;
        public IReadOnlyDictionary<string, string> Specifiers
            => _specifiers is null
                ? null
                : new ReadOnlyDictionary<string, string>(_specifiers);

        public string ExtendedSpecifiers { get; }

        public IEnumerable<string> Groups { get; }

        public Market(
            int id,
            int refId,
            IDictionary<string, string> specifiers,
            string extendedSpecifiers,
            IEnumerable<string> groups,
            IMarketDescriptionFactory marketDescriptionFactory,
            ExceptionHandlingStrategy exceptionHandlingStrategy)
        {
            Id = id;
            RefId = refId;
            _specifiers = specifiers;
            ExtendedSpecifiers = extendedSpecifiers;
            Groups = groups;
            _marketDescriptionFactory = marketDescriptionFactory;
            _exceptionHandlingStrategy = exceptionHandlingStrategy;
        }

        public string GetName(CultureInfo culture)
        {
            try
            {
                var marketDescription = _marketDescriptionFactory.GetMarketDescription(Id, _specifiers, new[] { culture });
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
            return marketName;

            if (Specifiers is null || Specifiers.Any() == false)
                return marketName;

            var template = marketName;
            foreach(var specifier in Specifiers)
            {

            }
        }
    }
}

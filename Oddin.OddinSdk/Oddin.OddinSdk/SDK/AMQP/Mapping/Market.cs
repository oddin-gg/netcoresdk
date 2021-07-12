﻿using Microsoft.Extensions.Logging;
using Oddin.OddinSdk.Common;
using Oddin.OddinSdk.Common.Exceptions;
using Oddin.OddinSdk.SDK.AMQP.Abstractions;
using Oddin.OddinSdk.SDK.API.Abstractions;
using Oddin.OddinSdk.SDK.FeedConfiguration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping
{
    internal class Market : LoggingBase, IMarket
    {
        private readonly IApiClient _apiClient;
        private readonly ExceptionHandlingStrategy _exceptionHandlingStrategy;

        public int Id { get; }

        public IReadOnlyDictionary<string, string> Specifiers { get; }

        public Market(int id, IDictionary<string, string> specifiers, IApiClient apiClient, ExceptionHandlingStrategy exceptionHandlingStrategy, ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
            if (specifiers is null)
                throw new ArgumentNullException($"{nameof(specifiers)}");

            if (apiClient is null)
                throw new ArgumentNullException($"{nameof(apiClient)}");

            Id = id;
            Specifiers = specifiers as IReadOnlyDictionary<string, string>;
            _apiClient = apiClient;
            _exceptionHandlingStrategy = exceptionHandlingStrategy;
        }

        public async Task<string> GetNameAsync(CultureInfo culture)
        {
            try
            {
                var marketDescriptions = await _apiClient.GetMarketDescriptionsAsync(culture);
                return marketDescriptions
                    .Where(m => m.Id == Id)
                    .First()
                    .Name;
            }
            catch (Exception e)
            when (e is CommunicationException
                || e is MappingException)
            {
                e.HandleAccordingToStrategy(GetType().Name, _log, _exceptionHandlingStrategy);
            }
            return null;
        }
    }
}

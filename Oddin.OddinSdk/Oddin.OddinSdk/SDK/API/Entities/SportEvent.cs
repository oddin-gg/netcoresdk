using Microsoft.Extensions.Logging;
using Oddin.OddinSdk.Common;
using Oddin.OddinSdk.Common.Exceptions;
using Oddin.OddinSdk.SDK.AMQP;
using Oddin.OddinSdk.SDK.API.Abstractions;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.FeedConfiguration;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Oddin.OddinSdk.SDK.API.Entities
{
    internal class SportEvent : LoggingBase, ISportEvent
    {
        private readonly URN _id;
        private readonly IApiClient _apiClient;
        private readonly ExceptionHandlingStrategy _exceptionHandlingStrategy;

        public URN Id => _id;

        public SportEvent(URN urn, IApiClient apiClient, ExceptionHandlingStrategy exceptionHandlingStrategy, ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
            if (apiClient is null)
                throw new ArgumentNullException($"{nameof(apiClient)}");

            _id = urn;
            _apiClient = apiClient;
            _exceptionHandlingStrategy = exceptionHandlingStrategy;
        }

        public async Task<string> GetNameAsync(CultureInfo culture)
        {
            try
            {
                return (await _apiClient.GetMatchSummaryAsync(_id, culture)).Name;
            }
            catch (Exception e)
            when (e is CommunicationException
                || e is MappingException)
            {
                e.HandleAccordingToStrategy(GetType().Name, _log, _exceptionHandlingStrategy);
            }
            return null;
        }

        public async Task<DateTime?> GetScheduledTimeAsync()
        {
            try
            {
                return (await _apiClient.GetMatchSummaryAsync(_id)).ScheduledTime;
            }
            catch (Exception e)
            when (e is CommunicationException
                || e is MappingException)
            {
                e.HandleAccordingToStrategy(GetType().Name, _log, _exceptionHandlingStrategy);
            }
            return null;
        }

        public async Task<DateTime?> GetScheduledEndTimeAsync()
        {
            try
            {
                return (await _apiClient.GetMatchSummaryAsync(_id)).ScheduledEndTime;
            }
            catch (Exception e)
            when (e is CommunicationException
                || e is MappingException)
            {
                e.HandleAccordingToStrategy(GetType().Name, _log, _exceptionHandlingStrategy);
            }
            return null;
        }

        public async Task<URN> GetSportIdAsync()
        {
            try
            {
                return (await _apiClient.GetMatchSummaryAsync(_id)).SportId;
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

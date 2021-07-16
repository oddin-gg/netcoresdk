using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.AMQP;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Oddin.OddsFeedSdk.Exceptions;

namespace Oddin.OddsFeedSdk.API.Entities
{
    internal class SportEvent : ISportEvent
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(SportEvent));

        private readonly URN _id;
        private readonly IApiClient _apiClient;
        private readonly ExceptionHandlingStrategy _exceptionHandlingStrategy;

        public URN Id => _id;

        public SportEvent(URN urn, IApiClient apiClient, ExceptionHandlingStrategy exceptionHandlingStrategy)
        {
            if (apiClient is null)
                throw new ArgumentNullException(nameof(apiClient));

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
            catch (SdkException e)
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
            catch (SdkException e)
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
            catch (SdkException e)
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
            catch (SdkException e)
            {
                e.HandleAccordingToStrategy(GetType().Name, _log, _exceptionHandlingStrategy);
            }
            return null;
        }
    }
}

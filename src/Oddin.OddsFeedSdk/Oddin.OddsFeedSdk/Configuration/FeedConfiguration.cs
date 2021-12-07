using Oddin.OddsFeedSdk.Configuration.Abstractions;
using System;
using System.Globalization;

namespace Oddin.OddsFeedSdk.Configuration
{
    internal class FeedConfiguration : IFeedConfiguration
    {
        // General properties
        public ExceptionHandlingStrategy ExceptionHandlingStrategy { get; }
        public CultureInfo DefaultLocale { get; }

        // Hosting propeties
        public string AccessToken { get; }
        public int? NodeId { get; }
        public string ApiHost { get; }
        public string Host { get; }
        public int Port { get; }
        public bool UseSsl { get; }
        public bool UseApiSsl { get; }

        // Recovery properties
        public int MaxRecoveryTime { get; }
        public int MaxInactivitySeconds { get; }
        public int HttpClientTimeout { get; }
        public bool IgnoreRecovery { get; }

        internal AppConfigurationSection Section { get; }

        public FeedConfiguration(
            string accessToken,
            CultureInfo defaultLocale,
            string host,
            int port,
            string apiHost,
            bool useSsl,
            bool useApiSsl,
            int maxInactivitySeconds,
            int maxRecoveryExecutionInSeconds,
            int? nodeId,
            ExceptionHandlingStrategy exceptionHandlingStrategy,
            int httpClientTimeout,
            AppConfigurationSection section,
            bool ignoreRecovery)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentException(nameof(accessToken));

            if (maxInactivitySeconds < SdkDefaults.MinInactivitySeconds || maxInactivitySeconds > SdkDefaults.MaxInactivitySeconds)
                throw new ArgumentOutOfRangeException(nameof(maxInactivitySeconds));

            if (maxRecoveryExecutionInSeconds < SdkDefaults.MinRecoveryExecutionInSeconds)
                throw new ArgumentOutOfRangeException(nameof(maxRecoveryExecutionInSeconds));

            if (httpClientTimeout < SdkDefaults.MinHttpClientTimeout || httpClientTimeout > SdkDefaults.MaxHttpClientTimeout)
                throw new ArgumentOutOfRangeException(nameof(httpClientTimeout));

            if (nodeId < 0)
                throw new ArgumentException($"Sdk Node Id is {nodeId}: only positive numbers are allowed");

            AccessToken = accessToken;
            DefaultLocale = defaultLocale ?? throw new ArgumentNullException(nameof(defaultLocale));
            Host = host;
            Port = port;
            UseSsl = useSsl;
            ApiHost = apiHost;
            UseApiSsl = useApiSsl;
            MaxInactivitySeconds = maxInactivitySeconds;
            MaxRecoveryTime = maxRecoveryExecutionInSeconds;
            NodeId = nodeId;
            ExceptionHandlingStrategy = exceptionHandlingStrategy;
            HttpClientTimeout = httpClientTimeout;
            Section = section;
            IgnoreRecovery = ignoreRecovery;
        }
    }
}

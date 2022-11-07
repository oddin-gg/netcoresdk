using System;
using System.Globalization;
using Oddin.OddsFeedSdk.Configuration.Abstractions;

namespace Oddin.OddsFeedSdk.Configuration;

internal class FeedConfiguration : IFeedConfiguration
{
    public FeedConfiguration(
        string accessToken,
        CultureInfo defaultLocale,
        string host,
        int port,
        string apiHost,
        bool useSsl,
        bool useApiSsl,
        int maxInactivitySeconds,
        int maxRecoveryExecutionMinutes,
        int? nodeId,
        ExceptionHandlingStrategy exceptionHandlingStrategy,
        int httpClientTimeout,
        int initialSnapshotTimeInMinutes,
        AppConfigurationSection section)
    {
        if (string.IsNullOrEmpty(accessToken))
            throw new ArgumentException(nameof(accessToken));

        if (httpClientTimeout is < SdkDefaults.MinHttpClientTimeout or > SdkDefaults.MaxHttpClientTimeout)
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
        MaxRecoveryExecutionMinutes = maxRecoveryExecutionMinutes;
        NodeId = nodeId;
        ExceptionHandlingStrategy = exceptionHandlingStrategy;
        HttpClientTimeout = httpClientTimeout;
        InitialSnapshotTimeInMinutes = initialSnapshotTimeInMinutes;
        Section = section;
    }

    internal AppConfigurationSection Section { get; }

    // General properties
    public ExceptionHandlingStrategy ExceptionHandlingStrategy { get; }
    public CultureInfo DefaultLocale { get; }

    // Hosting properties
    public string AccessToken { get; }
    public int? NodeId { get; }
    public string ApiHost { get; }
    public string Host { get; }
    public int Port { get; }
    public bool UseSsl { get; }
    public bool UseApiSsl { get; }

    // Recovery properties
    public int MaxRecoveryExecutionMinutes { get; }
    public int MaxInactivitySeconds { get; }
    public int HttpClientTimeout { get; }
    public int InitialSnapshotTimeInMinutes { get; }
}
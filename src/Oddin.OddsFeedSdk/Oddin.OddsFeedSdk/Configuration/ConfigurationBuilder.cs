using Oddin.OddsFeedSdk.Configuration.Abstractions;

namespace Oddin.OddsFeedSdk.Configuration;

internal class ConfigurationBuilder : RecoveryConfigurationBuilder<IConfigurationBuilder>, IConfigurationBuilder
{
    private readonly string _apiHost;
    private readonly string _host;
    private readonly int _port;

    public ConfigurationBuilder(string accessToken, IAppConfigurationSectionProvider sectionProvider, string host,
        string apiHost, int port)
        : base(accessToken, sectionProvider)
    {
        _host = host;
        _apiHost = apiHost;
        _port = port;
    }

    public override IFeedConfiguration Build()
    {
        PreBuildCheck();

        return new FeedConfiguration(
            AccessToken,
            DefaultLocale,
            _host,
            _port,
            _apiHost,
            true,
            true,
            MaxInactivitySeconds ?? SdkDefaults.MaxInactivitySeconds,
            MaxRecoveryExecutionMinutes ?? SdkDefaults.MaxRecoveryExecutionMinutes,
            SdkNodeId,
            ExceptionHandlingStrategy,
            HttpClientTimeout ?? SdkDefaults.DefaultHttpClientTimeout,
            InitialSnapshotTimeInMinutes ?? default,
            Section);
    }
}
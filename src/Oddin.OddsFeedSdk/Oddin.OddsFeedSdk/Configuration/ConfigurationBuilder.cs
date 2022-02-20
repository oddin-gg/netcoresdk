using Oddin.OddsFeedSdk.Configuration.Abstractions;

namespace Oddin.OddsFeedSdk.Configuration
{
    internal class ConfigurationBuilder : RecoveryConfigurationBuilder<IConfigurationBuilder>, IConfigurationBuilder
    {
        private readonly string _host;
        private readonly string _apiHost;
        private readonly int _port;

        public ConfigurationBuilder(string accessToken, IAppConfigurationSectionProvider sectionProvider, string host, string apiHost, int port)
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
                accessToken: AccessToken,
                defaultLocale: DefaultLocale,
                host: _host,
                port: _port,
                apiHost: _apiHost,
                useSsl: true,
                useApiSsl: true,
                maxInactivitySeconds: MaxInactivitySeconds ?? SdkDefaults.MaxInactivitySeconds,
                maxRecoveryExecutionMinutes: MaxRecoveryExecutionMinutes ?? SdkDefaults.MaxRecoveryExecutionMinutes,
                nodeId: SdkNodeId,
                exceptionHandlingStrategy: ExceptionHandlingStrategy,
                httpClientTimeout: HttpClientTimeout ?? SdkDefaults.DefaultHttpClientTimeout,
                InitialSnapshotTimeInMinutes ?? default,
                section: Section);
        }
    }
}
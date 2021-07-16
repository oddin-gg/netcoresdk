using Oddin.OddsFeedSdk.Configuration.Abstractions;

namespace Oddin.OddsFeedSdk.Configuration
{
    internal class ReplayConfigurationBuilder : ConfigurationBuilderBase<IReplayConfigurationBuilder>, IReplayConfigurationBuilder
    {
        public ReplayConfigurationBuilder(string accessToken, IAppConfigurationSectionProvider sectionProvider)
            : base(accessToken, sectionProvider)
        {
        }

        public override IFeedConfiguration Build()
        {
            PreBuildCheck();

            return new FeedConfiguration(
                accessToken: AccessToken,
                environment: SdkEnvironment.Replay,
                defaultLocale: DefaultLocale,
                host: SdkDefaults.IntegrationHost,
                port: SdkDefaults.DefaultPort,
                apiHost: SdkDefaults.IntegrationApiHost,
                useSsl: true,
                useApiSsl: true,
                maxInactivitySeconds: SdkDefaults.MaxInactivitySeconds,
                maxRecoveryExecutionInSeconds: SdkDefaults.MaxRecoveryExecutionInSeconds,
                nodeId: SdkNodeId,
                exceptionHandlingStrategy: ExceptionHandlingStrategy,
                httpClientTimeout: HttpClientTimeout ?? SdkDefaults.DefaultHttpClientTimeout,
                section: Section);
        }
    }
}
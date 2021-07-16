using Oddin.OddsFeedSdk.Configuration.Abstractions;

namespace Oddin.OddsFeedSdk.Configuration
{
    internal class ConfigurationBuilder : RecoveryConfigurationBuilder<IConfigurationBuilder>, IConfigurationBuilder
    {
        private readonly SdkEnvironment _environment;

        public ConfigurationBuilder(string accessToken, IAppConfigurationSectionProvider sectionProvider, SdkEnvironment environment)
            : base(accessToken, sectionProvider)
        {
            _environment = environment;
        }

        public override IFeedConfiguration Build()
        {
            PreBuildCheck();

            return new FeedConfiguration(
                accessToken: AccessToken,
                environment: _environment,
                defaultLocale: DefaultLocale,
                host: _environment == SdkEnvironment.Production ? SdkDefaults.ProductionHost : SdkDefaults.IntegrationHost,
                port: SdkDefaults.DefaultPort,
                apiHost: _environment == SdkEnvironment.Production ? SdkDefaults.ProductionApiHost : SdkDefaults.IntegrationApiHost,
                useSsl: true,
                useApiSsl: true,
                maxInactivitySeconds: MaxInactivitySeconds ?? SdkDefaults.MinInactivitySeconds,
                maxRecoveryExecutionInSeconds: MaxRecoveryTimeInSeconds ?? SdkDefaults.MaxRecoveryExecutionInSeconds,
                nodeId: SdkNodeId,
                exceptionHandlingStrategy: ExceptionHandlingStrategy,
                httpClientTimeout: HttpClientTimeout ?? SdkDefaults.DefaultHttpClientTimeout,
                section: Section);
        }
    }
}
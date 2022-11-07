using Oddin.OddsFeedSdk.Configuration.Abstractions;

namespace Oddin.OddsFeedSdk.Configuration;

internal class ReplayConfigurationBuilder : ConfigurationBuilderBase<IReplayConfigurationBuilder>,
    IReplayConfigurationBuilder
{
    public ReplayConfigurationBuilder(string accessToken, IAppConfigurationSectionProvider sectionProvider)
        : base(accessToken, sectionProvider)
    {
    }

    public override IFeedConfiguration Build()
    {
        PreBuildCheck();

        return new FeedConfiguration(
            AccessToken,
            DefaultLocale,
            SdkDefaults.GetIntegrationHost(),
            SdkDefaults.DefaultPort,
            SdkDefaults.GetIntegrationApiHost(),
            true,
            true,
            SdkDefaults.MaxInactivitySeconds,
            SdkDefaults.MaxRecoveryExecutionMinutes,
            SdkNodeId,
            ExceptionHandlingStrategy,
            HttpClientTimeout ?? SdkDefaults.DefaultHttpClientTimeout,
            InitialSnapshotTimeInMinutes ?? default,
            Section);
    }
}
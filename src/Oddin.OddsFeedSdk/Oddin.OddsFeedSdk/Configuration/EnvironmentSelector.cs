using System;
using Oddin.OddsFeedSdk.Configuration.Abstractions;

namespace Oddin.OddsFeedSdk.Configuration;

internal class EnvironmentSelector : IEnvironmentSelector
{
    private readonly string _accessToken;

    private readonly IAppConfigurationSectionProvider _sectionProvider;

    internal EnvironmentSelector(string accessToken, IAppConfigurationSectionProvider sectionProvider)
    {
        if (string.IsNullOrEmpty(accessToken))
            throw new ArgumentException(nameof(accessToken));

        _accessToken = accessToken;
        _sectionProvider = sectionProvider ?? throw new ArgumentNullException(nameof(sectionProvider));
    }

    public IConfigurationBuilder SelectIntegration(string region = Region.DEFAULT) => new ConfigurationBuilder(
        _accessToken, _sectionProvider, SdkDefaults.GetIntegrationHost(region),
        SdkDefaults.GetIntegrationApiHost(region), SdkDefaults.DefaultPort);

    public IConfigurationBuilder SelectProduction(string region = Region.DEFAULT) => new ConfigurationBuilder(
        _accessToken, _sectionProvider, SdkDefaults.GetProductionHost(region), SdkDefaults.GetProductionApiHost(region),
        SdkDefaults.DefaultPort);

    public IConfigurationBuilder SelectTest(string region = Region.DEFAULT) => new ConfigurationBuilder(_accessToken,
        _sectionProvider, SdkDefaults.GetTestHost(region), SdkDefaults.GetTestApiHost(region), SdkDefaults.DefaultPort);

    public IReplayConfigurationBuilder SelectReplay() => new ReplayConfigurationBuilder(_accessToken, _sectionProvider);

    public IConfigurationBuilder SelectEnvironment(string host, string apiHost, int port) =>
        new ConfigurationBuilder(_accessToken, _sectionProvider, host, apiHost, port);
}
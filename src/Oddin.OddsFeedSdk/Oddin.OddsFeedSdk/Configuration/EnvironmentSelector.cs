using Oddin.OddsFeedSdk.Configuration.Abstractions;
using System;

namespace Oddin.OddsFeedSdk.Configuration
{
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

        public IConfigurationBuilder SelectIntegration(string region = Region.DEFAULT)
        {
            return new ConfigurationBuilder(_accessToken, _sectionProvider, SdkDefaults.GetIntegrationHost(region), SdkDefaults.GetIntegrationApiHost(region), SdkDefaults.DefaultPort);
        }

        public IConfigurationBuilder SelectProduction(string region = Region.DEFAULT)
        {
            return new ConfigurationBuilder(_accessToken, _sectionProvider, SdkDefaults.GetProductionHost(region), SdkDefaults.GetProductionApiHost(region), SdkDefaults.DefaultPort);
        }

        public IConfigurationBuilder SelectTest(string region = Region.DEFAULT)
        {
            return new ConfigurationBuilder(_accessToken, _sectionProvider, SdkDefaults.GetTestHost(region), SdkDefaults.GetTestApiHost(region), SdkDefaults.DefaultPort);
        }

        public IReplayConfigurationBuilder SelectReplay()
        {
            return new ReplayConfigurationBuilder(_accessToken, _sectionProvider);
        }

        public IConfigurationBuilder SelectEnvironment(string host, string apiHost, int port)
        {
            return new ConfigurationBuilder(_accessToken, _sectionProvider, host, apiHost, port);
        }

    }
}
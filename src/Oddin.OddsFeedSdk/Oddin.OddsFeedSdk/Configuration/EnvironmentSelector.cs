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
            
            if (sectionProvider is null)
                throw new ArgumentNullException(nameof(sectionProvider));

            _accessToken = accessToken;
            _sectionProvider = sectionProvider;
        }

        public IConfigurationBuilder SelectIntegration()
        {
            return new ConfigurationBuilder(_accessToken, _sectionProvider, SdkEnvironment.Integration);
        }

        public IConfigurationBuilder SelectProduction()
        {
            return new ConfigurationBuilder(_accessToken, _sectionProvider, SdkEnvironment.Production);
        }

        public IReplayConfigurationBuilder SelectReplay()
        {
            return new ReplayConfigurationBuilder(_accessToken, _sectionProvider);
        }
    }
}
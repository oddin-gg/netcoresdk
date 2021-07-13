using Oddin.OddinSdk.SDK.Configuration.Abstractions;
using System;

namespace Oddin.OddinSdk.SDK.Configuration
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

        /// <summary>
        /// Use integration server and api endpoints
        /// </summary>
        public IConfigurationBuilder SelectIntegration()
        {
            return new ConfigurationBuilder(_accessToken, _sectionProvider, SdkEnvironment.Integration);
        }

        /// <summary>
        /// Use production server and api endpoints
        /// </summary>
        public IConfigurationBuilder SelectProduction()
        {
            return new ConfigurationBuilder(_accessToken, _sectionProvider, SdkEnvironment.Production);
        }

        /// <summary>
        /// Use integration server and api endpoints but replay binding will be used
        /// </summary>
        public IReplayConfigurationBuilder SelectReplay()
        {
            return new ReplayConfigurationBuilder(_accessToken, _sectionProvider);
        }
    }
}
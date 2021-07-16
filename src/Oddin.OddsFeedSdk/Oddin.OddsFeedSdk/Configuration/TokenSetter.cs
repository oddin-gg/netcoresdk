using Oddin.OddsFeedSdk.Configuration.Abstractions;
using System;

namespace Oddin.OddsFeedSdk.Configuration
{
    internal class TokenSetter : ITokenSetter
    {
        private readonly IAppConfigurationSectionProvider _appConfigurationSectionProvider;

        internal TokenSetter(IAppConfigurationSectionProvider configurationProvider)
        {
            if (configurationProvider is null)
                throw new ArgumentNullException(nameof(configurationProvider));

            _appConfigurationSectionProvider = configurationProvider;
        }

        public IEnvironmentSelector SetAccessToken(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentException("Access Token cannot be a null or empty", nameof(accessToken));

            return new EnvironmentSelector(accessToken, _appConfigurationSectionProvider);
        }

        public IEnvironmentSelector SetAccessTokenFromConfigFile()
        {
            return new EnvironmentSelector(_appConfigurationSectionProvider.Get().AccessToken, _appConfigurationSectionProvider);
        }
    }
}
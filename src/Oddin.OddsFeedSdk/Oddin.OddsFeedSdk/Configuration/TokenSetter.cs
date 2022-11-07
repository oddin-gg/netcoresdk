using System;
using Oddin.OddsFeedSdk.Configuration.Abstractions;

namespace Oddin.OddsFeedSdk.Configuration;

internal class TokenSetter : ITokenSetter
{
    private readonly IAppConfigurationSectionProvider _appConfigurationSectionProvider;

    internal TokenSetter(IAppConfigurationSectionProvider configurationProvider) => _appConfigurationSectionProvider =
        configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));

    public IEnvironmentSelector SetAccessToken(string accessToken)
    {
        if (string.IsNullOrEmpty(accessToken))
            throw new ArgumentException("Access Token cannot be a null or empty", nameof(accessToken));

        return new EnvironmentSelector(accessToken, _appConfigurationSectionProvider);
    }

    public IEnvironmentSelector SetAccessTokenFromConfigFile() =>
        new EnvironmentSelector(_appConfigurationSectionProvider.Get().AccessToken, _appConfigurationSectionProvider);
}
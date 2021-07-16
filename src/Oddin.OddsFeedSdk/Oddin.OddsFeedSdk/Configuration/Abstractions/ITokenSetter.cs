namespace Oddin.OddsFeedSdk.Configuration.Abstractions
{
    public interface ITokenSetter
    {
        IEnvironmentSelector SetAccessToken(string accessToken);

        IEnvironmentSelector SetAccessTokenFromConfigFile();
    }
}

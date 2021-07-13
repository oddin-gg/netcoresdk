namespace Oddin.OddinSdk.SDK.Configuration.Abstractions
{
    /// <summary>
    /// Defines a contract implemented by classes taking care of the 1st step when building configuration - setting the token
    /// </summary>
    public interface ITokenSetter
    {
        /// <summary>
        /// Sets the access token used to the access resources
        /// </summary>
        /// <param name="accessToken">The access token used to access resources</param>
        /// <returns>The <see cref="IEnvironmentSelector"/> instance allowing the selection of target environment</returns>
        IEnvironmentSelector SetAccessToken(string accessToken);

        /// <summary>
        /// Sets the access token used to the access resources from value read from configuration file
        /// </summary>
        /// <returns>The <see cref="IEnvironmentSelector"/> instance allowing the selection of target environment</returns>
        IEnvironmentSelector SetAccessTokenFromConfigFile();
    }
}

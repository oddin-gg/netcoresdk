namespace Oddin.OddinSdk.SDK
{
    /// <summary>
    /// Defines a contract implemented by classes representing odds feed configuration / settings
    /// </summary>
    public interface IOddsFeedConfiguration
    {
        /// <summary>
        /// Gets the access token used when accessing feed's REST interface
        /// </summary>
        string AccessToken { get; }

        /// <summary>
        /// Gets a host name of the Sports API
        /// </summary>
        string ApiHost { get; }

        /// <summary>
        /// Gets a value indicating whether the connection to Sports API should use SSL
        /// </summary>
        bool UseApiSsl { get; }

        /// <summary>
        /// Gets a value specifying timeout set for HTTP responses
        /// </summary>
        int HttpClientTimeout { get; }
    }
}

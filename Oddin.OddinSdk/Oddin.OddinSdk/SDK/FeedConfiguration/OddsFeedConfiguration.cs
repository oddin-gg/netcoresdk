namespace Oddin.OddinSdk.SDK.FeedConfiguration
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

        /// <summary>
        /// Gets a value specifying the host name of the AQMP broker
        /// </summary>
        string Host { get; }

        /// <summary>
        /// Gets the port used for connecting to the AQMP broker
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Gets the exception handling strategy
        /// </summary>
        /// <value>The exception handling strategy</value>
        public ExceptionHandlingStrategy ExceptionHandlingStrategy { get; }
    }
}

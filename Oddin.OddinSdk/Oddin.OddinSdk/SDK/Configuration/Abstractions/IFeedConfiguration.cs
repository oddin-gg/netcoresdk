using System.Globalization;

namespace Oddin.OddinSdk.SDK.Configuration.Abstractions
{
    /// <summary>
    /// Defines the main configuration of the Feed
    /// </summary>
    public interface IFeedConfiguration
    {
        /// <summary>
        /// Gets the access token used when accessing feed's REST interface
        /// </summary>
        public string AccessToken { get; }

        /// <summary>
        /// Gets the maximum allowed timeout in seconds, between consecutive AMQP messages associated with the same producer.
        /// If this value is exceeded, the producer is considered to be down
        /// </summary>
        public int MaxInactivitySeconds { get; }

        /// <summary>
        /// Gets a <see cref="CultureInfo" /> specifying default locale (only 'en' is supported at this moment)
        /// </summary>
        public CultureInfo DefaultLocale { get; }

        /// <summary>
        /// Gets the maximum recovery time in seconds
        /// </summary>
        public int MaxRecoveryTime { get; }

        /// <summary>
        /// Gets the SDK node identifier
        /// </summary>
        public int NodeId { get; }

        /// <summary>
        /// Gets the <see cref="SdkEnvironment"/> value specifying the environment to which to connect.
        /// </summary>
        public SdkEnvironment Environment { get; }

        /// <summary>
        /// Gets the exception handling strategy
        /// </summary>
        /// <value>The exception handling strategy</value>
        public ExceptionHandlingStrategy ExceptionHandlingStrategy { get; }

        /// <summary>
        /// Gets a value specifying the host name of the AQMP server
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// Gets the port used for connecting to the AQMP server
        /// </summary>
        /// <value>The port</value>
        public int Port { get; }

        /// <summary>
        /// Gets a value indicating whether a ssl should be used to AQMP
        /// </summary>
        public bool UseSsl { get; }

        /// <summary>
        /// Gets a host name of the Rest Api
        /// </summary>
        public string ApiHost { get; }

        /// <summary>
        /// Value indicating if ssl should be used when communicating with Rest Api
        /// </summary>
        public bool UseApiSsl { get; }

        /// <summary>
        /// Gets a value specifying timeout set for HTTP responses
        /// </summary>
        public int HttpClientTimeout { get; }
    }
}

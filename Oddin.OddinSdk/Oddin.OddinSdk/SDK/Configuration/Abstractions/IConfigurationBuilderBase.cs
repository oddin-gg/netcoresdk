using System.Globalization;

namespace Oddin.OddinSdk.SDK.Configuration.Abstractions
{
    /// <summary>
    /// A general configuration builder
    /// </summary>
    public interface IConfigurationBuilderBase<out T>
    {
        /// <summary>
        /// Sets the general configuration from file
        /// </summary>
        T LoadFromConfigFile();

        /// <summary>
        /// Sets the default locale
        /// </summary>
        T SetDefaultLocale(CultureInfo culture);

        /// <summary>
        /// How are exceptions thrown handled in the SDK.
        /// </summary>
        T SetExceptionHandlingStrategy(ExceptionHandlingStrategy strategy);

        /// <summary>
        /// Sets the node id used by SDK
        /// </summary>
        T SetNodeId(int nodeId);

        /// <summary>
        /// Sets the timeout for HTTP responses from REST endpoints
        /// </summary>
        T SetHttpClientTimeout(int httpClientTimeout);

        /// <summary>
        /// Builds and returns a <see cref="IFeedConfiguration"/>
        /// </summary>
        IFeedConfiguration Build();
    }

    /// <summary>
    /// A recovery part of the configuration builder
    /// </summary>
    public interface IRecoveryConfigurationBuilder<out T> : IConfigurationBuilderBase<T>
    {
        /// <summary>
        /// Sets the max time window between two consecutive alive messages before the associated producer is marked as down
        /// </summary>
        T SetMaxInactivitySeconds(int inactivitySeconds);

        /// <summary>
        /// Sets the maximum time in seconds in which recovery must be completed (minimum 900 seconds)
        /// </summary>
        T SetMaxRecoveryTime(int maxRecoveryTimeInSeconds);
    }

    /// <summary>
    /// Defines a general configuration properties
    /// </summary>
    public interface IConfigurationBuilder : IRecoveryConfigurationBuilder<IConfigurationBuilder>
    {
    }

    /// <summary>
    /// Defines a replay configuration properties
    /// </summary>
    public interface IReplayConfigurationBuilder : IConfigurationBuilderBase<IReplayConfigurationBuilder>
    {
    }
}

using System.Globalization;

namespace Oddin.OddsFeedSdk.Configuration.Abstractions
{
    public interface IConfigurationBuilderBase<out T>
    {
        T LoadFromConfigFile();

        T SetDefaultLocale(CultureInfo culture);

        T SetExceptionHandlingStrategy(ExceptionHandlingStrategy strategy);

        T SetNodeId(int nodeId);

        T SetHttpClientTimeout(int httpClientTimeout);

        T SetInitialSnapshotTimeInMinutes(int initialSnapshotTimeInMinutes);

        IFeedConfiguration Build();
    }

    public interface IRecoveryConfigurationBuilder<out T> : IConfigurationBuilderBase<T>
    {
        T SetMaxInactivitySeconds(int inactivitySeconds);

        T SetMaxRecoveryTime(int maxRecoveryTimeInSeconds);
    }

    public interface IConfigurationBuilder : IRecoveryConfigurationBuilder<IConfigurationBuilder>
    {
    }

    public interface IReplayConfigurationBuilder : IConfigurationBuilderBase<IReplayConfigurationBuilder>
    {
    }
}

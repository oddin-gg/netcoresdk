using Oddin.OddsFeedSdk.Configuration.Abstractions;

namespace Oddin.OddsFeedSdk.Configuration;

internal abstract class RecoveryConfigurationBuilder<T> : ConfigurationBuilderBase<T>, IRecoveryConfigurationBuilder<T>
    where T : class
{
    protected int? MaxInactivitySeconds;
    protected int? MaxRecoveryExecutionMinutes;

    internal RecoveryConfigurationBuilder(string accessToken, IAppConfigurationSectionProvider sectionProvider)
        : base(accessToken, sectionProvider)
    {
    }

    public T SetMaxInactivitySeconds(int inactivitySeconds)
    {
        MaxInactivitySeconds = inactivitySeconds;
        return this as T;
    }

    public T SetMaxRecoveryExecutionMinutes(int maxRecoveryExecutionMinutes)
    {
        MaxRecoveryExecutionMinutes = maxRecoveryExecutionMinutes;
        return this as T;
    }

    internal override void LoadFromConfigFile(AppConfigurationSection section)
    {
        base.LoadFromConfigFile(section);

        MaxInactivitySeconds = section.MaxInactivitySeconds;
        MaxRecoveryExecutionMinutes = section.MaxRecoveryExecutionMinutes;
    }
}
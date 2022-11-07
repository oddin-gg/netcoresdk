using System;
using System.Configuration;
using Oddin.OddsFeedSdk.Configuration.Abstractions;

namespace Oddin.OddsFeedSdk.Configuration;

internal class AppConfigurationSection : ConfigurationSection
{
    private const string SECTION_NAME = "Oddin";

    [ConfigurationProperty("AccessToken", IsRequired = true)]
    internal string AccessToken
    {
        get => (string)this["AccessToken"];
        set => this["AccessToken"] = value;
    }

    [ConfigurationProperty("DefaultLocale", DefaultValue = "en", IsRequired = false)]
    internal string DefaultLocale
    {
        get => (string)this["DefaultLocale"];
        set => this["DefaultLocale"] = value;
    }

    [ConfigurationProperty("ExceptionHandlingStrategy", DefaultValue = ExceptionHandlingStrategy.THROW,
        IsRequired = false)]
    internal ExceptionHandlingStrategy ExceptionHandlingStrategy
    {
        get => (ExceptionHandlingStrategy)this["ExceptionHandlingStrategy"];
        set => this["ExceptionHandlingStrategy"] = value;
    }

    [ConfigurationProperty("SdkNodeId", DefaultValue = null, IsRequired = false)]
    internal int? SdkNodeId
    {
        get => (int?)this["SdkNodeId"];
        set => this["SdkNodeId"] = value;
    }

    [ConfigurationProperty("HttpClientTimeout", DefaultValue = null, IsRequired = false)]
    internal int? HttpClientTimeout
    {
        get => (int?)this["HttpClientTimeout"];
        set => this["HttpClientTimeout"] = value;
    }

    [ConfigurationProperty("MaxInactivitySeconds", DefaultValue = null, IsRequired = false)]
    internal int? MaxInactivitySeconds
    {
        get => (int?)this["MaxInactivitySeconds"];
        set => this["MaxInactivitySeconds"] = value;
    }

    [ConfigurationProperty("MaxRecoveryExecutionMinutes", DefaultValue = null, IsRequired = false)]
    internal int? MaxRecoveryExecutionMinutes
    {
        get => (int?)this["MaxRecoveryExecutionMinutes"];
        set => this["MaxRecoveryExecutionMinutes"] = value;
    }

    [ConfigurationProperty("InitialSnapshotTimeInMinutes", DefaultValue = null, IsRequired = false)]
    internal int? InitialSnapshotTimeInMinutes
    {
        get => (int?)this["InitialSnapshotTimeInMinutes"];
        set => this["InitialSnapshotTimeInMinutes"] = value;
    }

    internal static AppConfigurationSection LoadFromFile()
    {
        var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        if (config == null)
            throw new InvalidOperationException("Unable to load application configuration");

        var section = (AppConfigurationSection)config.GetSection(SECTION_NAME);
        if (section == null)
            throw new InvalidOperationException($"Unable to load section {SECTION_NAME} from configuration");

        return section;
    }
}
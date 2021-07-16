using Oddin.OddsFeedSdk.Configuration.Abstractions;
using System;
using System.Configuration;

namespace Oddin.OddsFeedSdk.Configuration
{
    internal class AppConfigurationSection : ConfigurationSection
    {
        private const string SECTION_NAME = "Oddin";

        [ConfigurationProperty("AccessToken", IsRequired = true)]
        internal string AccessToken
        {
            get { return this["AccessToken"] as string; }
            set { this["AccessToken"] = value; }
        }

        [ConfigurationProperty("DefaultLocale", DefaultValue = "en", IsRequired = false)]
        internal string DefaultLocale
        {
            get { return this["DefaultLocale"] as string; }
            set { this["DefaultLocale"] = value; }
        }
        
        [ConfigurationProperty("ExceptionHandlingStrategy", DefaultValue = ExceptionHandlingStrategy.THROW, IsRequired = false)]
        internal ExceptionHandlingStrategy ExceptionHandlingStrategy
        {
            get { return (ExceptionHandlingStrategy)this["ExceptionHandlingStrategy"]; }
            set { this["ExceptionHandlingStrategy"] = value; }
        }

        [ConfigurationProperty("SdkNodeId", DefaultValue = null, IsRequired = false)]
        internal int? SdkNodeId
        {
            get { return this["SdkNodeId"] as int?; }
            set { this["SdkNodeId"] = value; }
        }     
        
        [ConfigurationProperty("HttpClientTimeout", DefaultValue = null, IsRequired = false)]
        internal int? HttpClientTimeout
        {
            get { return this["HttpClientTimeout"] as int?; }
            set { this["HttpClientTimeout"] = value; }
        }
        
        [ConfigurationProperty("MaxInactivitySeconds", DefaultValue = null, IsRequired = false)]
        internal int? MaxInactivitySeconds
        {
            get { return this["MaxInactivitySeconds"] as int?; }
            set { this["MaxInactivitySeconds"] = value; }
        }
        
        [ConfigurationProperty("MaxRecoveryTimeInSeconds", DefaultValue = null, IsRequired = false)]
        internal int? MaxRecoveryTimeInSeconds
        {
            get { return this["MaxRecoveryTimeInSeconds"] as int?; }
            set { this["MaxRecoveryTimeInSeconds"] = value; }
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
}

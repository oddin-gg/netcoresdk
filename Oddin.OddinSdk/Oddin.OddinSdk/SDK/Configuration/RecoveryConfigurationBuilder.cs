using Oddin.OddinSdk.SDK.Configuration.Abstractions;
using System;

namespace Oddin.OddinSdk.SDK.Configuration
{
    internal abstract class RecoveryConfigurationBuilder<T> : ConfigurationBuilderBase<T>, IRecoveryConfigurationBuilder<T> where T : class
    {
        protected int? MaxInactivitySeconds;
        protected int? MaxRecoveryTimeInSeconds;

        internal RecoveryConfigurationBuilder(string accessToken, IAppConfigurationSectionProvider sectionProvider)
            : base(accessToken, sectionProvider)
        {
        }

        internal override void LoadFromConfigFile(AppConfigurationSection section)
        {
            base.LoadFromConfigFile(section);

            MaxInactivitySeconds = section.MaxInactivitySeconds;
            MaxRecoveryTimeInSeconds = section.MaxRecoveryTimeInSeconds;
        }

        /// <summary>
        /// Max time window between two consecutive alive messages before the producer is marked as down
        /// </summary>
        public T SetMaxInactivitySeconds(int inactivitySeconds)
        {
            if (inactivitySeconds < SdkDefaults.MinInactivitySeconds)
                throw new ArgumentException($"Value must be at least {SdkDefaults.MinInactivitySeconds}.");

            if (inactivitySeconds > SdkDefaults.MaxInactivitySeconds)
                throw new ArgumentException($"Value must be less then {SdkDefaults.MaxInactivitySeconds}.");

            MaxInactivitySeconds = inactivitySeconds;
            return this as T;
        }

        /// <summary>
        /// Maximum time in seconds in which recovery must be completed
        /// </summary>
        public T SetMaxRecoveryTime(int maxRecoveryTimeInSeconds)
        {
            if (maxRecoveryTimeInSeconds < SdkDefaults.MinRecoveryExecutionInSeconds)
                throw new ArgumentException($"Value must be at least {SdkDefaults.MinRecoveryExecutionInSeconds}.");

            if (maxRecoveryTimeInSeconds > SdkDefaults.MaxRecoveryExecutionInSeconds)
                throw new ArgumentException($"Value must be less then {SdkDefaults.MaxRecoveryExecutionInSeconds}.");

            MaxRecoveryTimeInSeconds = maxRecoveryTimeInSeconds;
            return this as T;
        }
    }
}

using Oddin.OddsFeedSdk.Configuration.Abstractions;
using System;
using System.Globalization;
using System.Linq;

namespace Oddin.OddsFeedSdk.Configuration
{
    internal abstract class ConfigurationBuilderBase<T> : IConfigurationBuilderBase<T>
        where T : class
    {
        internal readonly IAppConfigurationSectionProvider SectionProvider;

        internal readonly string AccessToken;

        internal CultureInfo DefaultLocale;

        internal ExceptionHandlingStrategy ExceptionHandlingStrategy;

        internal int? SdkNodeId;

        internal int? HttpClientTimeout;

        internal int? InitialSnapshotTimeInMinutes;

        internal AppConfigurationSection Section { get; private set; }

        internal ConfigurationBuilderBase(string accessToken, IAppConfigurationSectionProvider sectionProvider)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentException(nameof(accessToken));

            AccessToken = accessToken;
            SectionProvider = sectionProvider ?? throw new ArgumentNullException(nameof(sectionProvider));
            ExceptionHandlingStrategy = ExceptionHandlingStrategy.CATCH;
            SdkNodeId = null;
            DefaultLocale = Feed.AvailableLanguages().FirstOrDefault();
        }

        internal virtual void LoadFromConfigFile(AppConfigurationSection section)
        {
            Section = section ?? throw new ArgumentNullException(nameof(section));

            if (string.IsNullOrEmpty(section.DefaultLocale) == false)
                SetDefaultLocale(new CultureInfo(section.DefaultLocale.Trim()));

            ExceptionHandlingStrategy = section.ExceptionHandlingStrategy;
            SdkNodeId = section.SdkNodeId;

            HttpClientTimeout = section.HttpClientTimeout;

            InitialSnapshotTimeInMinutes = section.InitialSnapshotTimeInMinutes;
        }

        public T LoadFromConfigFile()
        {
            LoadFromConfigFile(SectionProvider.Get());
            return this as T;
        }

        public T SetDefaultLocale(CultureInfo culture)
        {
            DefaultLocale = culture ?? throw new ArgumentNullException(nameof(culture));
            return this as T;
        }

        public T SetExceptionHandlingStrategy(ExceptionHandlingStrategy strategy)
        {
            ExceptionHandlingStrategy = strategy;
            return this as T;
        }

        public T SetNodeId(int nodeId)
        {
            SdkNodeId = nodeId;
            return this as T;
        }

        public T SetHttpClientTimeout(int httpClientTimeout)
        {
            HttpClientTimeout = httpClientTimeout;
            return this as T;
        }

        public T SetInitialSnapshotTimeInMinutes(int initialSnapshotTimeInMinutes)
        {
            InitialSnapshotTimeInMinutes = initialSnapshotTimeInMinutes;
            return this as T;
        }

        public abstract IFeedConfiguration Build();

        protected virtual void PreBuildCheck()
        {
            DefaultLocale ??= Feed.AvailableLanguages().First();

            if (string.IsNullOrEmpty(AccessToken))
                throw new InvalidOperationException("Missing access token");
        }
    }
}
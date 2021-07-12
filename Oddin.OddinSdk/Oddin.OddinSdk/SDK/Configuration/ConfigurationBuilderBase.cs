using Oddin.OddinSdk.SDK.Configuration.Abstractions;
using System;
using System.Globalization;
using System.Linq;

namespace Oddin.OddinSdk.SDK.Configuration
{
    internal abstract class ConfigurationBuilderBase<T> : IConfigurationBuilderBase<T> 
        where T : class
    {
        internal readonly IAppConfigurationSectionProvider SectionProvider;

        internal readonly string AccessToken;

        internal CultureInfo DefaultLocale;

        internal ExceptionHandlingStrategy ExceptionHandlingStrategy;

        internal int SdkNodeId;

        internal int? HttpClientTimeout;

        internal AppConfigurationSection Section { get; private set; }

        internal ConfigurationBuilderBase(string accessToken, IAppConfigurationSectionProvider sectionProvider)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentNullException(nameof(accessToken));

            if (sectionProvider is null)
                throw new ArgumentNullException(nameof(sectionProvider));

            AccessToken = accessToken;
            SectionProvider = sectionProvider;
            ExceptionHandlingStrategy = ExceptionHandlingStrategy.CATCH;
            SdkNodeId = 0;
            DefaultLocale = Feed.AvailableLanguages().FirstOrDefault();
        }

        internal virtual void LoadFromConfigFile(AppConfigurationSection section)
        {
            if (section is null)
                throw new ArgumentNullException(nameof(section));

            Section = section;

            if (string.IsNullOrEmpty(section.DefaultLocale) == false)
                SetDefaultLocale(new CultureInfo(section.DefaultLocale.Trim()));

            ExceptionHandlingStrategy = section.ExceptionHandlingStrategy;
            SdkNodeId = section.SdkNodeId.Value;

            HttpClientTimeout = section.HttpClientTimeout;
        }

        public T LoadFromConfigFile()
        {
            LoadFromConfigFile(SectionProvider.Get());
            return this as T;
        }

        public T SetDefaultLocale(CultureInfo culture)
        {
            if (culture is null)
                throw new ArgumentNullException(nameof(culture));

            DefaultLocale = culture;
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

        public abstract IFeedConfiguration Build();

        protected virtual void PreBuildCheck()
        {
            if (DefaultLocale == null)
                DefaultLocale = Feed.AvailableLanguages().First();
            
            if (string.IsNullOrEmpty(AccessToken))
                throw new InvalidOperationException("Missing access token");
        }
    }
}
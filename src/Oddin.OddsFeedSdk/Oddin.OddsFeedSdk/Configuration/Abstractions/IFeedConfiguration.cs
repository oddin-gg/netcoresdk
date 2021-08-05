using System.Globalization;

namespace Oddin.OddsFeedSdk.Configuration.Abstractions
{
    public interface IFeedConfiguration
    {
        public string AccessToken { get; }

        public int MaxInactivitySeconds { get; }

        public CultureInfo DefaultLocale { get; }

        public int MaxRecoveryTime { get; }

        public int? NodeId { get; }

        public SdkEnvironment Environment { get; }

        public ExceptionHandlingStrategy ExceptionHandlingStrategy { get; }

        public string Host { get; }

        public int Port { get; }

        public bool UseSsl { get; }

        public string ApiHost { get; }

        public bool UseApiSsl { get; }

        public int HttpClientTimeout { get; }
    }
}

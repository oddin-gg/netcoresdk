using Oddin.OddinSdk.SDK.FeedConfiguration;

namespace Oddin.OddinSdk.SampleIntegration
{
    // TODO: replace with configuration implementation builder included in sdk (properties validation!)

    class CustomOddsFeedConfiguration : IOddsFeedConfiguration
    {
        public string AccessToken { get; }
        public string ApiHost { get; }
        public bool UseApiSsl { get; }
        public int HttpClientTimeout { get; }
        public string Host { get; }
        public int Port { get; }
        public ExceptionHandlingStrategy ExceptionHandlingStrategy { get; }

        public CustomOddsFeedConfiguration(
            string accessToken,
            string apiHost,
            bool useApiSsl,
            int httpClientTimeout,
            string host,
            int port,
            ExceptionHandlingStrategy strategy)
        {
            AccessToken = accessToken;
            ApiHost = apiHost;
            UseApiSsl = useApiSsl;
            HttpClientTimeout = httpClientTimeout;
            Host = host;
            Port = port;
            ExceptionHandlingStrategy = strategy;
        }
    }
}

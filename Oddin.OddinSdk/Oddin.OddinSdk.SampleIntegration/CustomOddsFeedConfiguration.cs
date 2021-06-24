using Oddin.OddinSdk.SDK;

namespace Oddin.OddinSdk.SampleIntegration
{
    // TODO: replace with configuration implementation builder included in sdk (properties validation!)

    class CustomOddsFeedConfiguration : IOddsFeedConfiguration
    {
        public string AccessToken { get; }
        public string ApiHost { get; }
        public bool UseApiSsl { get; }
        public int HttpClientTimeout { get; }

        public CustomOddsFeedConfiguration(
            string accessToken,
            string apiHost,
            bool useApiSsl,
            int httpClientTimeout)
        {
            AccessToken = accessToken;
            ApiHost = apiHost;
            UseApiSsl = useApiSsl;
            HttpClientTimeout = httpClientTimeout;
        }
    }
}

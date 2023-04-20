namespace Oddin.OddsFeedSdk
{
    public static class SdkDefaults
    {
        public const string ProductionHost = "mq.oddin.gg";
        public const string ProductionApiHost = "api-mq.oddin.gg";

        public const string IntegrationHost = "mq.integration.oddin.gg";
        public const string IntegrationApiHost = "api-mq.integration.oddin.gg";

        public const string TestHost = "mq-test.integration.oddin.gg";
        public const string TestApiHost = "api-mq-test.integration.oddin.gg";

        public const int DefaultPort = 5672;

        public const int UnknownProducerId = 99;

        public const int StatefulRecoveryWindowInMinutes = 4320;

        public const int MinHttpClientTimeout = 10;
        public const int MaxHttpClientTimeout = 60;
        public const int DefaultHttpClientTimeout = 30;
        public static int MaxRecoveryExecutionMinutes = 360;
        public static int MaxInactivitySeconds = 20;
    }
}
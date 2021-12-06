namespace Oddin.OddsFeedSdk
{
    internal static class SdkDefaults
    {
        internal const string ProductionHost = "mq.oddin.gg";
        internal const string ProductionApiHost = "api-mq.oddin.gg";

        internal const string IntegrationHost = "mq.integration.oddin.gg";
        internal const string IntegrationApiHost = "api-mq.integration.oddin.gg";

        internal const int DefaultPort = 5672;

        internal const int UnknownProducerId = 99;

        internal const int StatefulRecoveryWindowInMinutes = 4320;

        internal const int MinInactivitySeconds = 11;
        internal const int DefaultInactivitySeconds = 15;
        internal const int MaxInactivitySeconds = 30;

        internal const int MinRecoveryExecutionInSeconds = 600;
        internal const int MaxRecoveryExecutionInSeconds = 21600;

        internal const int MinHttpClientTimeout = 10;
        internal const int MaxHttpClientTimeout = 60;
        internal const int DefaultHttpClientTimeout = 30;
    }
}

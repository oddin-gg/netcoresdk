using System;
using Oddin.OddsFeedSdk.Configuration.Abstractions;

namespace Oddin.OddsFeedSdk
{
    public static class SdkDefaults
    {

        [Obsolete("ProductionHost is deprecated, please use GetProductionHost() instead.")]
        public static readonly string ProductionHost = GetProductionHost();

        [Obsolete("ProductionApiHost is deprecated, please use GetProductionApiHost() instead.")]
        public static readonly string ProductionApiHost = GetProductionApiHost();

        [Obsolete("IntegrationHost is deprecated, please use GetIntegrationHost() instead.")]
        public static readonly string IntegrationHost = GetIntegrationHost();

        [Obsolete("IntegrationApiHost is deprecated, please use GetIntegrationApiHost() instead.")]
        public static readonly string IntegrationApiHost = GetIntegrationApiHost();

        [Obsolete("TestHost is deprecated, please use GetTestHost() instead.")]
        public static readonly string TestHost = GetTestHost();

        [Obsolete("TestApiHost is deprecated, please use GetTestApiHost() instead.")]
        public static readonly string TestApiHost = GetTestApiHost();

        public const int DefaultPort = 5672;

        public const int UnknownProducerId = 99;

        public const int StatefulRecoveryWindowInMinutes = 4320;

        public const int MinHttpClientTimeout = 10;
        public const int MaxHttpClientTimeout = 60;
        public const int DefaultHttpClientTimeout = 30;
        public static int MaxRecoveryExecutionMinutes = 360;
        public static int MaxInactivitySeconds = 20;

        public static string GetProductionHost(string region = Region.DEFAULT)
        {
            return "mq." + region + "oddin.gg";
        }

        public static string GetProductionApiHost(string region = Region.DEFAULT)
        {
            return "api-mq." + region + "oddin.gg";
        }

        public static string GetIntegrationHost(string region = Region.DEFAULT)
        {
            return "mq.integration." + region + "oddin.gg";
        }

        public static string GetIntegrationApiHost(string region = Region.DEFAULT)
        {
            return "api-mq.integration." + region + "oddin.gg";
        }

        public static string GetTestHost(string region = Region.DEFAULT)
        {
            return "mq-test.integration." + region + "oddin.gg";
        }

        public static string GetTestApiHost(string region = Region.DEFAULT)
        {
            return "api-mq-test.integration." + region + "oddin.gg";
        }

    }
}
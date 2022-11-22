using System;
using System.Threading.Tasks;
using Oddin.OddsFeedSdk;

namespace Oddin.OddsFeedSdkDemoIntegration
{
    public static class Program
    {
        // Put you token here:
        private const bool REPLAY = false;
        private const string TOKEN = "95d7d5b7-4b31-407a-9320-4aa5bdec8bc5";
        private const string MQHOST = "mq-test.integration.oddin.gg";
        private const string APIHOST = "api-mq-test.integration.oddin.gg";

        static async Task Main(string[] _)
        {
            if (!REPLAY)
            {
                await GeneralExample.Run(TOKEN, Feed
                    .GetConfigurationBuilder()
                    .SetAccessToken(TOKEN)
                    .SelectEnvironment(MQHOST, APIHOST)
                    .SetInitialSnapshotTimeInMinutes(1)
                    .Build());
            }
            else
            {
                await ReplayExample.Run(TOKEN, "od:match:61695");
            }
        }
    }
}

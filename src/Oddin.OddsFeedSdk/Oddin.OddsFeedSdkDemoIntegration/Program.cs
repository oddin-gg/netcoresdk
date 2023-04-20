using System;
using System.Threading.Tasks;
using Oddin.OddsFeedSdk;
using Oddin.OddsFeedSdk.Configuration.Abstractions;

namespace Oddin.OddsFeedSdkDemoIntegration
{
    public static class Program
    {
        static async Task Main(string[] _)
        {
            // Put you token here:
            var token = "your-token";

            // Or put token and running mode into env variables
            var envToken = Environment.GetEnvironmentVariable("TOKEN");
            
            // FEED | REPLAY
            var envMode = Environment.GetEnvironmentVariable("MODE");
            
            // TEST | INTEGRATION | PRODUCTION
            var envEnvironment = Environment.GetEnvironmentVariable("ENVIRONMENT");

            if (!string.IsNullOrEmpty(envToken))
            {
                token = envToken;
            }

            if (envMode == "FEED")
            {
                IFeedConfiguration config = envEnvironment switch
                {
                    "PRODUCTION" => Feed.GetConfigurationBuilder()
                        .SetAccessToken(token)
                        .SelectProduction()
                        .SetInitialSnapshotTimeInMinutes(15)
                        .Build(),
                    "TEST" => Feed.GetConfigurationBuilder()
                        .SetAccessToken(token)
                        .SelectTest()
                        .SetInitialSnapshotTimeInMinutes(5)
                        .Build(),
                    _ => Feed.GetConfigurationBuilder()
                        .SetAccessToken(token)
                        .SelectIntegration()
                        .SetInitialSnapshotTimeInMinutes(5)
                        .Build()
                };

                await GeneralExample.Run(token, config);
            }

            if (envMode == "REPLAY")
            {
                await ReplayExample.Run(token, "od:match:61695");
            } else {
                // if no environment is set, run interactive prompt
                Console.WriteLine("Select example:");
                Console.WriteLine("G = General");
                Console.WriteLine("R = Replay");
                Console.Write("Enter letter: ");
                var key = Console.ReadKey().KeyChar;
                Console.WriteLine();

                switch (char.ToUpper(key))
                {
                    case 'R':
                    {
                        await ReplayExample.Run(token, "od:match:61695");
                        break;
                    }
                    default:
                    {
                        await GeneralExample.Run(token);
                        break;
                    }
                }
            }
        }
    }
}
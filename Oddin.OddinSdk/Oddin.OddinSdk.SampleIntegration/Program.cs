using Microsoft.Extensions.Logging;
using Oddin.OddinSdk.SDK;
using Oddin.OddinSdk.SDK.AMQP.EventArguments;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.Sessions;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Oddin.OddinSdk.SampleIntegration
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serilogLogger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo
                .Console()
                .CreateLogger();

            var loggerFactory = new LoggerFactory().AddSerilog(serilogLogger);

            var config = Feed
                .GetConfigurationBuilder()
                .SetAccessToken("1a0c5a30-74ed-416d-b120-8c05f92e382f")
                .SelectIntegration()
                .Build();

            var feed = new Feed(config, loggerFactory);

            var session = feed
                .CreateBuilder()
                .SetMessageInterest(MessageInterest.AllMessages)
                .Build();

            session.OnOddsChange += OnOddsChangeReceived;
            session.OnBetStop += OnBetStopReceived;
            session.OnBetSettlement += OnBetSettlement;
            session.OnUnparsableMessageReceived += OnUnparsableMessageReceived;
            session.OnBetCancel += Session_OnBetCancel;

            feed.Open();
            Console.ReadLine();
            feed.Close();

            session.OnOddsChange -= OnOddsChangeReceived;
            session.OnBetStop -= OnBetStopReceived;
            session.OnBetSettlement -= OnBetSettlement;
            session.OnUnparsableMessageReceived -= OnUnparsableMessageReceived;
            session.OnBetCancel -= Session_OnBetCancel;
        }

        private static void Session_OnBetCancel(object sender, BetCancelEventArgs<ISportEvent> e)
        {
            Console.WriteLine($"On Bet Cancel Message Received in {e.GetBetCancel().Event.GetNameAsync(Feed.AvailableLanguages().First())}");
        }

        private static async void OnBetSettlement(object sender, BetSettlementEventArgs<ISportEvent> eventArgs)
        {
            Console.WriteLine($"On Bet Settlement in {await eventArgs.GetBetSettlement().Event.GetNameAsync(Feed.AvailableLanguages().First())}");
        }

        private static async void OnOddsChangeReceived(object sender, OddsChangeEventArgs<ISportEvent> eventArgs)
        {
            Console.WriteLine($"Odds changed in {await eventArgs.GetOddsChange().Event.GetNameAsync(Feed.AvailableLanguages().First())}");
        }

        private static async void OnBetStopReceived(object sender, BetStopEventArgs<ISportEvent> eventArgs)
        {
            Console.WriteLine($"Bet stop in {await eventArgs.GetBetStop().Event.GetNameAsync(Feed.AvailableLanguages().First())}");
        }
        private static void OnUnparsableMessageReceived(object sender, UnparsableMessageEventArgs e)
        {
            Console.WriteLine($"On Unparsable Message Received in {e.MessageType}");
        }
    }
}

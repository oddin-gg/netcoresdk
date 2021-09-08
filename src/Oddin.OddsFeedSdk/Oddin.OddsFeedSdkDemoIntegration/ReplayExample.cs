using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk;
using Oddin.OddsFeedSdk.AMQP.EventArguments;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Dispatch.EventArguments;
using Oddin.OddsFeedSdk.Sessions;
using Oddin.OddsFeedSdk.Sessions.Abstractions;
using Serilog;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdkDemoIntegration
{
    internal static class ReplayExample
    {
        private static readonly CultureInfo CultureEn = CultureInfo.GetCultureInfoByIetfLanguageTag("en");

        internal static async Task Run(string token)
        {
            var loggerFactory = CreateLoggerFactory();

            var config = Feed
                .GetConfigurationBuilder()
                .SetAccessToken(token)
                .SelectReplay()
                .SetNodeId(1)
                .Build();

            var feed = new ReplayFeed(config, loggerFactory);

            var session = feed
                .CreateBuilder()
                .SetMessageInterest(MessageInterest.AllMessages)
                .Build();

            AttachEvents(feed);
            AttachEvents(session);

            feed.Open();

            var replayManager = feed.ReplayManager;

            var match1 = new URN("od:match:32496");
            var match2 = new URN("od:match:32497");
            
            // Stop replay
            await replayManager.StopReplay();

            // Get URNs of events already in queue
            var eventsInQueue = await replayManager.GetEventsInQueue();

            //Get list of events as ISportEvents 
            var replayList = await replayManager.GetReplayList();

            // If match1 is not in queue add it
            if (eventsInQueue.Any(q => q == match1) == false)
                await replayManager.AddMessagesToReplayQueue(match1);

            // If match2 is not in queue add it
            if (eventsInQueue.Any(q => q == match2) == false)
                await replayManager.AddMessagesToReplayQueue(match2);

            // Get URNs of events already in queue
            var eventsInQueueAfter = await replayManager.GetEventsInQueue();

            // Start the replay
            var result = await replayManager.StartReplay(30, 500);

            // Check status
            var status = await replayManager.GetStatusOfReplay();
            Console.WriteLine($"Start status: {result} Replay status: {status}");
            // Wait for 30 minutes
            await Task.Delay(TimeSpan.FromMinutes(1));

            await replayManager.StopAndClearReplay();
            
            feed.Close();

            DetachEvents(feed);
            DetachEvents(session);
        }

        private static ILoggerFactory CreateLoggerFactory()
        {
            var serilogLogger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            return new LoggerFactory().AddSerilog(serilogLogger);
        }

        private static void AttachEvents(Feed feed)
        {
            feed.EventRecoveryCompleted += OnEventRecoveryComplete;
        }

        private static void DetachEvents(Feed feed)
        {
            feed.EventRecoveryCompleted -= OnEventRecoveryComplete;
        }

        private static void AttachEvents(IOddsFeedSession session)
        {
            session.OnOddsChange += OnOddsChangeReceived;
            session.OnBetStop += OnBetStopReceived;
            session.OnBetSettlement += OnBetSettlement;
            session.OnUnparsableMessageReceived += OnUnparsableMessageReceived;
            session.OnBetCancel += Session_OnBetCancel;
            session.OnFixtureChange += Session_OnFixtureChange;
        }

        private static void DetachEvents(IOddsFeedSession session)
        {
            session.OnOddsChange -= OnOddsChangeReceived;
            session.OnBetStop -= OnBetStopReceived;
            session.OnBetSettlement -= OnBetSettlement;
            session.OnUnparsableMessageReceived -= OnUnparsableMessageReceived;
            session.OnBetCancel -= Session_OnBetCancel;
            session.OnFixtureChange -= Session_OnFixtureChange;
        }

        private static async void Session_OnFixtureChange(object sender, FixtureChangeEventArgs<ISportEvent> eventArgs)
        {
            Console.WriteLine($"On Bet Cancel Message Received in {eventArgs.GetFixtureChange().Event.Id} {await eventArgs.GetFixtureChange().Event.GetNameAsync(Feed.AvailableLanguages().First())}");
        }

        private static async void Session_OnBetCancel(object sender, BetCancelEventArgs<ISportEvent> eventArgs)
        {
            Console.WriteLine($"On Bet Cancel Message Received in {eventArgs.GetBetCancel().Event.Id} {await eventArgs.GetBetCancel().Event.GetNameAsync(Feed.AvailableLanguages().First())}");
        }

        private static async void OnBetSettlement(object sender, BetSettlementEventArgs<ISportEvent> eventArgs)
        {
            Console.WriteLine($"On Bet Settlement in {eventArgs.GetBetSettlement().Event.Id} {await eventArgs.GetBetSettlement().Event.GetNameAsync(Feed.AvailableLanguages().First())}");
        }

        private static async void OnOddsChangeReceived(object sender, OddsChangeEventArgs<ISportEvent> eventArgs)
        {
            Console.WriteLine($"Odds changed in {eventArgs.GetOddsChange().Event.Id} {await eventArgs.GetOddsChange().Event.GetNameAsync(Feed.AvailableLanguages().First())}");
        }

        private static async void OnBetStopReceived(object sender, BetStopEventArgs<ISportEvent> eventArgs)
        {
            Console.WriteLine($"Bet stop in {eventArgs.GetBetStop().Event.Id} {await eventArgs.GetBetStop().Event.GetNameAsync(Feed.AvailableLanguages().First())}");
        }

        private static void OnUnparsableMessageReceived(object sender, UnparsableMessageEventArgs e)
        {
            Console.WriteLine($"On Unparsable Message Received in {e.MessageType}");
        }

        private static void OnEventRecoveryComplete(object sender, EventRecoveryCompletedEventArgs eventArgs)
        {
            Console.WriteLine($"Event recovery completed [event id: {eventArgs.GetEventId()}, request id: {eventArgs.GetRequestId()}]");
        }
    }
}

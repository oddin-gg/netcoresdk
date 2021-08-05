using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk;
using Oddin.OddsFeedSdk.AMQP.EventArguments;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Dispatch.EventArguments;
using Oddin.OddsFeedSdk.Sessions;
using Oddin.OddsFeedSdk.Sessions.Abstractions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdkDemoIntegration
{
    internal static class GeneralExample
    {
        private static readonly CultureInfo CultureEn = CultureInfo.GetCultureInfoByIetfLanguageTag("en");
        private static readonly CultureInfo CultureRu = CultureInfo.GetCultureInfoByIetfLanguageTag("ru");

        internal static async Task Run()
        {
            var loggerFactory = CreateLoggerFactory();

            var config = Feed
                .GetConfigurationBuilder()
                .SetAccessToken(Program.TOKEN)
                .SelectIntegration()
                .Build();

            var feed = new Feed(config, loggerFactory);

            var session = feed
                .CreateBuilder()
                .SetMessageInterest(MessageInterest.AllMessages)
                .Build();

            AttachEvents(feed);
            AttachEvents(session);

            feed.Open();

            var ctrlCPressed = new TaskCompletionSource<bool>();
            Console.CancelKeyPress += (s, e) => { e.Cancel = true; ctrlCPressed.TrySetResult(true); };

            var tasks = new List<Task>
            {
                WorkWithRecovery(feed),
                WorkWithSportDataProvider(feed),
                WorkWithMarketDesctiptionManager(feed),
                ctrlCPressed.Task
            };
            await Task.WhenAll(tasks);

            feed.Close();

            DetachEvents(feed);
            DetachEvents(session);
        }

        private async static Task WorkWithRecovery(Feed feed)
        {
            //var producer = feed.ProducerManager.Get("live");
            //var urn = new URN("od:match:35671");
            //Console.WriteLine($"Event recovery request response: {await feed.EventRecoveryRequestIssuer.RecoverEventMessagesAsync(producer, urn)}");

            //Console.ReadLine();
        }

        private async static Task WorkWithSportDataProvider(Feed feed)
        {
            var provider = feed.SportDataProvider;

            var sportsEn = await provider.GetSportsAsync(CultureEn);
            var sportsRu = await provider.GetSportsAsync(CultureRu);

            Console.WriteLine($"{sportsEn.FirstOrDefault()?.Id}");
            Console.WriteLine($"{sportsRu.FirstOrDefault()?.Id}");
        }

        private async static Task WorkWithMarketDesctiptionManager(Feed feed)
        {
            try
            {
                var manager = feed.MarketDescriptionManager;

                var marketDescriptionsEn = manager.GetMarketDescriptions(CultureEn);
                var marketDescriptionsRu = manager.GetMarketDescriptions(CultureRu);

                var description = marketDescriptionsEn.First();
                var specifiers = string.Join(", ", description.Specifiers.Select(s => $"Name:{s.Name} Type:{s.Type}"));
                var outcomes = string.Join(", ", description.Outcomes.Select(o => $"Id:{o.Id}/{o.RefId} Name:{o.GetName(CultureEn)} Description:{o.GetDescription(CultureEn)}"));

                Console.WriteLine($"Market Description - Id:{description.Id} RefId:{description.RefId} OutcomeType/Variant:{description.OutcomeType}");
                Console.WriteLine($"Specifiers:{specifiers}");
                Console.WriteLine($"Outcomes:{outcomes}");
            }
            catch (Exception e)
            {

            }
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

        private static async void Session_OnFixtureChange(object sender, FixtureChangeEventArgs<ISportEvent> e)
        {
            Console.WriteLine($"On Bet Cancel Message Received in {await e.GetFixtureChange().Event.GetNameAsync(Feed.AvailableLanguages().First())}");
        }

        private static async void Session_OnBetCancel(object sender, BetCancelEventArgs<ISportEvent> e)
        {
            Console.WriteLine($"On Bet Cancel Message Received in {await e.GetBetCancel().Event.GetNameAsync(Feed.AvailableLanguages().First())}");
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

        private static void OnEventRecoveryComplete(object sender, EventRecoveryCompletedEventArgs eventArgs)
        {
            Console.WriteLine($"Event recovery completed [event id: {eventArgs.GetEventId()}, request id: {eventArgs.GetRequestId()}]");
        }
    }
}

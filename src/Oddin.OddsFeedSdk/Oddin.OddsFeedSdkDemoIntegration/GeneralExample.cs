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
                WorkWithProducers(feed),
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
            var matchUrn = "od:match:36856";
            var producerName = "live";

            var producer = feed.ProducerManager.Get(producerName);
            var urn = new URN(matchUrn);

            // if the match is too old, 404 will be returned
            Console.WriteLine($"Event recovery request response: {await feed.EventRecoveryRequestIssuer.RecoverEventMessagesAsync(producer, urn)}");
            
            Console.WriteLine($"Event stateful recovery request response: {await feed.EventRecoveryRequestIssuer.RecoverEventStatefulMessagesAsync(producer, urn)}");
        }

        private async static Task WorkWithProducers(Feed feed)
        {
            foreach (var producer in feed.ProducerManager.Producers)
                Console.WriteLine($"Producer name: {producer.Name}, id: {producer.Id}");
        }

        private async static Task WorkWithSportDataProvider(Feed feed)
        {
            var provider = feed.SportDataProvider;

            var competitorUrn = new URN("od:competitor:300");
            var matchUrn = new URN("od:match:36856");
            var tournamentUrn = new URN("od:tournament:1524");
            var sportUrn = new URN("od:sport:1");

            provider.DeleteCompetitorFromCache(competitorUrn);
            provider.DeleteMatchFromCache(matchUrn);
            provider.DeleteTournamentFromCache(tournamentUrn);

            var activeTournaments = provider.GetActiveTournaments(CultureEn);
            var act = activeTournaments.First();
            //Console.WriteLine($"Competitor: {act.GetCompetitors().First().GetName(CultureEn)}");
            Console.WriteLine($"End date: {act.GetEndDate()}");
            Console.WriteLine($"Name: {await act.GetNameAsync(CultureEn)}");
            Console.WriteLine($"Scheduled end time: {await act.GetScheduledEndTimeAsync()}");
            Console.WriteLine($"Scheduled time: {await act.GetScheduledTimeAsync()}");
            Console.WriteLine($"Sport ID: {await act.GetSportIdAsync()}");
            Console.WriteLine($"Start date: {act.GetStartDate()}");
            Console.WriteLine($"ID: {act.Id}");

            var availableTournaments = provider.GetAvailableTournaments(sportUrn, CultureEn);

            var competitor = provider.GetCompetitor(competitorUrn, CultureEn);
            var fixtureChanges = provider.GetFixtureChanges(CultureEn);
            var listOfMatches = provider.GetListOfMatches(0, 2, CultureEn);
            var liveMatches = provider.GetLiveMatches(CultureEn);
            var match = provider.GetMatch(matchUrn, CultureEn);
            var matchesForNow = provider.GetMatchesFor(DateTime.UtcNow, CultureEn);
            var sport = await provider.GetSportAsync(sportUrn, CultureEn);
            var sports = await provider.GetSportsAsync(CultureEn);

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

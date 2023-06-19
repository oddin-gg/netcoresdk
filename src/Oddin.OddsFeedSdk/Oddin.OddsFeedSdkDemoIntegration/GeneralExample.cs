using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk;
using Oddin.OddsFeedSdk.Abstractions;
using Oddin.OddsFeedSdk.AMQP.EventArguments;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Dispatch.EventArguments;
using Oddin.OddsFeedSdk.Sessions;
using Oddin.OddsFeedSdk.Sessions.Abstractions;
using Serilog;

namespace Oddin.OddsFeedSdkDemoIntegration
{
    internal static class GeneralExample
    {
        private static readonly CultureInfo CultureEn = CultureInfo.GetCultureInfoByIetfLanguageTag("en");

        internal static async Task Run(string token, IFeedConfiguration config = null)
        {
            var loggerFactory = CreateLoggerFactory();

            // Build configuration
            config ??= Feed
                .GetConfigurationBuilder()
                .SetAccessToken(token)
                .SelectIntegration(Region.DEFAULT)
                .SetInitialSnapshotTimeInMinutes(60)
                // or .LoadFromConfigFile()
                .Build();

            // Create Feed
            var feed = new Feed(config, loggerFactory);

            // Subscribe for session
            var session = feed
                .CreateBuilder()
                .SetMessageInterest(MessageInterest.AllMessages)
                .Build();

            AttachEvents(feed);
            AttachEvents(session);

            feed.Open();

            var ctrlCPressed = new TaskCompletionSource<bool>();
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                ctrlCPressed.TrySetResult(true);
            };

            var tasks = new List<Task>
            {
                WorkWithRecovery(feed),
                WorkWithProducers(feed),
                WorkWithSportDataProvider(feed),
                WorkWithMarketDescriptionManager(feed),
                WorkWithBookmakerDetails(feed),
                ctrlCPressed.Task
            };
            await Task.WhenAll(tasks);

            feed.Close();

            DetachEvents(feed);
            DetachEvents(session);
        }

        private static Task WorkWithRecovery(IOddsFeed feed)
        {
            const string matchUrn = "od:match:32109";
            const string producerName = "live";

            var producer = feed.ProducerManager.Get(producerName);
            var urn = new URN(matchUrn);

            // if the match is too old, 404 will be returned
            // you can use HttpStatusCode.IsSuccessStatusCode to check validity of response
            Console.WriteLine(
                $"Event recovery request: {feed.RecoveryManager.InitiateEventOddsMessagesRecovery(producer, urn)}");
            Console.WriteLine(
                $"Event stateful recovery request: {feed.RecoveryManager.InitiateEventStatefulMessagesRecovery(producer, urn)}");

            return Task.CompletedTask;
        }

        private static Task WorkWithProducers(IOddsFeed feed)
        {
            foreach (var producer in feed.ProducerManager.Producers)
                Console.WriteLine($"Producer name: {producer.Name}, id: {producer.Id}");

            return Task.CompletedTask;
        }

        private static async Task WorkWithSportDataProvider(IOddsFeed feed)
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
            Console.WriteLine($"Competitor: {act.GetCompetitors().First().GetName(CultureEn)}");
            Console.WriteLine($"End date: {act.GetEndDate()}");
            Console.WriteLine($"Name: {await act.GetNameAsync(CultureEn)}");
            Console.WriteLine($"Scheduled end time: {await act.GetScheduledEndTimeAsync()}");
            Console.WriteLine($"Scheduled time: {await act.GetScheduledTimeAsync()}");
            Console.WriteLine($"Sport ID: {await act.GetSportIdAsync()}");
            Console.WriteLine($"Start date: {act.GetStartDate()}");
            Console.WriteLine($"ID: {act.Id}");
            Console.WriteLine($"Risk Tier: {act.RiskTier()}");

            var competitor = provider.GetCompetitor(competitorUrn, CultureEn);
            Console.WriteLine($"Abbreviation: {competitor.Abbreviations.First()}");
            Console.WriteLine($"Country: {competitor.Countries.FirstOrDefault()}");
            Console.WriteLine($"Country code: {competitor.CountryCode}");
            Console.WriteLine($"Abbreviation: {competitor.GetAbbreviation(CultureEn)}");
            Console.WriteLine($"Country: {competitor.GetCountry(CultureEn)}");
            Console.WriteLine($"Name: {competitor.GetName(CultureEn)}");
            Console.WriteLine($"Sport: {competitor.GetSports().First().GetName(CultureEn)}");
            Console.WriteLine($"ID: {competitor.Id}");
            Console.WriteLine($"Is virtual: {competitor.IsVirtual}");
            Console.WriteLine($"Name: {competitor.Names[CultureEn]}");
            Console.WriteLine($"Ref ID: {competitor.RefId}");
            Console.WriteLine($"Short name: {competitor.ShortName}");
            Console.WriteLine($"Icon Path: {competitor.IconPath}");

            var fixtureChanges = provider.GetFixtureChanges(CultureEn);
            var fc = fixtureChanges.First();
            Console.WriteLine($"Sport event ID: {fc.SportEventId}");
            Console.WriteLine($"Update time: {fc.UpdateTime}");

            var listOfMatches = provider.GetListOfMatches(0, 2, CultureEn);
            var m = listOfMatches.First();
            Console.WriteLine($"Away competitor name: {m.AwayCompetitor.GetName(CultureEn)}");
            Console.WriteLine($"Fixture ID: {m.Fixture.Id}");
            Console.WriteLine($"Fixture extra info: {string.Join(", ", m.Fixture.ExtraInfo)}");
            Console.WriteLine($"Name: {await m.GetNameAsync(CultureEn)}");
            Console.WriteLine($"Properties: {string.Join(", ", m.Status.Properties)}");
            Console.WriteLine($"Scheduled end time: {await m.GetScheduledEndTimeAsync()}");
            Console.WriteLine($"Scheduled time: {await m.GetScheduledTimeAsync()}");
            Console.WriteLine($"Sport ID: {await m.GetSportIdAsync()}");
            Console.WriteLine($"ID: {m.Id}");
            Console.WriteLine($"Live odds availability: {m.LiveOddsAvailability}");
            Console.WriteLine($"Status: {m.Status}");

            var sport = await provider.GetSportAsync(sportUrn, CultureEn);
            Console.WriteLine($"Name: {sport.GetName(CultureEn)}");
            Console.WriteLine($"Names: {string.Join(", ", sport.Names)}");
            Console.WriteLine($"ID: {sport.Id}");
            Console.WriteLine($"Icon Path: {sport.IconPath}");
            Console.WriteLine($"Name: {sport.Names.FirstOrDefault()}");
            Console.WriteLine($"Ref ID: {sport.RefId}");
            Console.WriteLine($"Tournament ID: {sport.Tournaments.FirstOrDefault()?.Id}");
        }

        private static Task WorkWithMarketDescriptionManager(IOddsFeed feed)
        {
            var manager = feed.MarketDescriptionManager;
            var marketDescriptionsEn = manager.GetMarketDescriptions(CultureEn);

            var description = marketDescriptionsEn.First();
            var specifiers = string.Join(", ", description.Specifiers.Select(s => $"Name:{s.Name} Type:{s.Type}"));
            var outcomes = string.Join(", ",
                description.Outcomes.Select(o =>
                    $"Id:{o.Id}/{o.RefId} Name:{o.GetName(CultureEn)} Description:{o.GetDescription(CultureEn)}"));

            Console.WriteLine(
                $"Market Description - Name: {description.GetName(CultureEn)} Id:{description.Id} RefId:{description.RefId} OutcomeType/Variant:{description.OutcomeType}");
            Console.WriteLine($"Specifiers:{specifiers}");
            Console.WriteLine($"Outcomes:{outcomes}");

            var marketVoidReasons = manager.GetMarketVoidReasons();
            foreach (var voidReason in marketVoidReasons)
            {
                Console.WriteLine(
                    $"Void reason: [id={voidReason.Id}; name='{voidReason.Name}'; description='{voidReason.Description}'; template='{voidReason.Template}'; params='{string.Join(",", voidReason.Params)}']");
            }

            return Task.CompletedTask;
        }

        private static Task WorkWithBookmakerDetails(IOddsFeed feed)
        {
            var bookmakerDetails = feed.BookmakerDetails;
            Console.WriteLine($"Bookmaker ID: {bookmakerDetails?.BookmakerId}");
            Console.WriteLine($"Expire at: {bookmakerDetails?.ExpireAt}");
            Console.WriteLine($"Virtual host: {bookmakerDetails?.VirtualHost}");

            return Task.CompletedTask;
        }

        private static ILoggerFactory CreateLoggerFactory()
        {
            var serilogLogger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            return new LoggerFactory().AddSerilog(serilogLogger);
        }

        private static void AttachEvents(IOddsFeed feed)
        {
            feed.EventRecoveryCompleted += OnEventRecoveryComplete;
            feed.ProducerDown += OnProducerDown;
            feed.ProducerUp += OnProducerUp;
            feed.ConnectionException += OnConnectionException;
            feed.Disconnected += OnDisconnected;
            feed.Closed += OnClosed;
        }

        private static void DetachEvents(IOddsFeed feed)
        {
            feed.EventRecoveryCompleted -= OnEventRecoveryComplete;
            feed.ProducerDown -= OnProducerDown;
            feed.ProducerUp -= OnProducerUp;
            feed.ConnectionException -= OnConnectionException;
            feed.Disconnected -= OnDisconnected;
            feed.Closed -= OnClosed;
        }

        private static void AttachEvents(IOddsFeedSession session)
        {
            session.OnRawFeedMessageReceived += OnRawFeedMessageReceived;
            session.OnOddsChange += OnOddsChangeReceived;
            session.OnBetStop += OnBetStopReceived;
            session.OnBetSettlement += OnBetSettlement;
            session.OnRollbackBetSettlement += OnRollbackBetSettlement;
            session.OnRollbackBetCancel += OnRollbackBetCancel;
            session.OnUnparsableMessageReceived += OnUnparsableMessageReceived;
            session.OnBetCancel += Session_OnBetCancel;
            session.OnFixtureChange += Session_OnFixtureChange;
        }

        private static void DetachEvents(IOddsFeedSession session)
        {
            session.OnRawFeedMessageReceived -= OnRawFeedMessageReceived;
            session.OnOddsChange -= OnOddsChangeReceived;
            session.OnBetStop -= OnBetStopReceived;
            session.OnBetSettlement -= OnBetSettlement;
            session.OnRollbackBetSettlement -= OnRollbackBetSettlement;
            session.OnRollbackBetCancel -= OnRollbackBetCancel;
            session.OnUnparsableMessageReceived -= OnUnparsableMessageReceived;
            session.OnBetCancel -= Session_OnBetCancel;
            session.OnFixtureChange -= Session_OnFixtureChange;
        }

        private static async void OnOddsChangeReceived(object sender, OddsChangeEventArgs<ISportEvent> eventArgs)
        {
            var oddsChange = eventArgs.GetOddsChange();
            var oddsChangeOther = eventArgs.GetOddsChange(Feed.AvailableLanguages().Last());

            var e = eventArgs.GetOddsChange().Event;
            var match = (IMatch)e;
            Console.WriteLine($"Odds changed in {match.Status}");
            Console.WriteLine($"Raw message: {Encoding.UTF8.GetString(oddsChange.RawMessage.Take(40).ToArray())}...");
            Console.WriteLine($"{string.Join(", ", match.HomeCompetitor.Abbreviations)}");
            Console.WriteLine($"{match.LiveOddsAvailability}");
            Console.WriteLine($"{match.Fixture.Id}");
            Console.WriteLine($"{await match.GetNameAsync(CultureEn)}");
            Console.WriteLine($"{await match.GetScheduledTimeAsync()}");
            Console.WriteLine($"Sport ID: {await e.GetSportIdAsync()}");
            Console.WriteLine($"Scheduled time: {await e.GetScheduledTimeAsync()}");

            // Market
            var market = oddsChange.Markets?.FirstOrDefault();
            Console.WriteLine($"Odds changed market: {market?.GetName(CultureEn)}");
            Console.WriteLine($"Odds changed market: {market?.Status}");

            // Outcome
            var outcome = oddsChangeOther.Markets?.FirstOrDefault()?.OutcomeOdds.FirstOrDefault();
            Console.WriteLine($"Odds changed market outcome: {outcome?.GetName(CultureEn)}");
            Console.WriteLine($"Odds changed market outcome: {outcome?.Id} {outcome?.RefId} {outcome?.Probabilities}");

            var competitor = match.Competitors.FirstOrDefault();
            Console.WriteLine($"Odds change competitor icon path: {competitor?.IconPath}");
            Console.WriteLine(
                $"Odds change competitor sports: {string.Join(", ", competitor?.GetSports().Select(s => s.Id) ?? Enumerable.Empty<URN>())}");

            // Tournament
            var tournament = match.Tournament;
            Console.WriteLine($"Tournament Id: {tournament.Id}");
            Console.WriteLine($"Risk Tier: {tournament.RiskTier()}");

            // Scoreboard
            if (match.Status.IsScoreboardAvailable)
            {
                var scoreboard = match.Status.Scoreboard;
                if (scoreboard != null)
                {
                    Console.WriteLine($"Home Goals: {scoreboard.HomeGoals}");
                    Console.WriteLine($"Away Goals: {scoreboard.AwayGoals}");
                    Console.WriteLine($"Scoreboard Time: {scoreboard.Time}");
                    Console.WriteLine($"Scoreboard GameTime: {scoreboard.GameTime}");
                }
            }
        }

        private static async void Session_OnFixtureChange(object sender, FixtureChangeEventArgs<ISportEvent> eventArgs)
        {
            var e = eventArgs.GetFixtureChange().Event;
            Console.WriteLine(
                $"On Bet Cancel Message Received in {await e.GetNameAsync(Feed.AvailableLanguages().First())}");
            Console.WriteLine($"Sport ID: {await e.GetSportIdAsync()}");
            Console.WriteLine($"Scheduled time: {await e.GetScheduledTimeAsync()}");
        }

        private static async void Session_OnBetCancel(object sender, BetCancelEventArgs<ISportEvent> eventArgs)
        {
            var e = eventArgs.GetBetCancel().Event;
            Console.WriteLine(
                $"On Bet Cancel Message Received in {await e.GetNameAsync(Feed.AvailableLanguages().First())}");
            Console.WriteLine($"Sport ID: {await e.GetSportIdAsync()}");
            Console.WriteLine($"Scheduled time: {await e.GetScheduledTimeAsync()}");
            foreach (var market in eventArgs.GetBetCancel(CultureEn).Markets)
            {
                Console.WriteLine(
                    $"Market: '{market.GetName(CultureEn)}'; voidReason: {market.VoidReason}; voidReasonId: {market.VoidReasonId}; voidReasonParams: '{market.VoidReasonParams}'");
            }
        }

        private static async void OnBetSettlement(object sender, BetSettlementEventArgs<ISportEvent> eventArgs)
        {
            var sportEvent = eventArgs.GetBetSettlement().Event;

            // Match
            var match = (IMatch)sportEvent;
            Console.WriteLine($"Match Id: {match.Id}");

            // Tournament
            var tournament = match.Tournament;
            Console.WriteLine($"Tournament Id: {tournament.Id}");
            Console.WriteLine($"Risk Tier: {tournament.RiskTier()}");

            // Get Sport from Match
            var sport1 = await match.GetSportAsync();
            var sportId = sport1.Id;
            Console.WriteLine($"Sport Id: {sportId}");

            // Get Sport from Tournament
            var sport2 = await tournament.GetSportAsync();
            var sportName = sport2.GetName(CultureEn);
            Console.WriteLine($"Sport Name: {sportName}");

            // Get Sport Using SportDataProvider
            var session = (IOddsFeedSession)sender;
            var provider = session.Feed.SportDataProvider;
            var sport3 = await provider.GetSportAsync(sportId);
            Console.WriteLine($"Icon Path: {sport3.IconPath}");

            foreach (var m in eventArgs.GetBetSettlement().Markets)
            {
                foreach (var outcome in m.OutcomeSettlements)
                {
                    if (outcome.VoidFactor != null)
                    {
                        Console.WriteLine($"Outcome with void factor: {outcome.VoidFactor}");
                    }
                }
            }
        }

        private static async void OnRollbackBetSettlement(object sender, RollbackBetSettlementEventArgs<ISportEvent> eventArgs)
        {
            var sportEvent = eventArgs.GetRollbackBetSettlement(CultureEn).Event;

            // Match
            var match = (IMatch)sportEvent;
            var name = await match.GetNameAsync(CultureEn);
            Console.WriteLine($"Match Id: {match.Id}; name: {name}");

            foreach (var market in eventArgs.GetRollbackBetSettlement(CultureEn).Markets)
            {
                var specifiers = string.Join(", ", market.Specifiers.Select(s => $"{s.Key}={s.Value}"));

                Console.WriteLine(
                    $"Rollback Bet Settlement: {market.Id}: '{market.GetName(CultureEn)}'; specifiers: {specifiers}");
            }
        }

        private static async void OnRollbackBetCancel(object sender, RollbackBetCancelEventArgs<ISportEvent> eventArgs)
        {
            var sportEvent = eventArgs.GetRollbackBetCancel(CultureEn).Event;

            // Match
            var match = (IMatch)sportEvent;
            var name = await match.GetNameAsync(CultureEn);
            Console.WriteLine($"Match Id: {match.Id}; name: {name}");

            foreach (var market in eventArgs.GetRollbackBetCancel(CultureEn).Markets)
            {
                var specifiers = string.Join(", ", market.Specifiers.Select(s => $"{s.Key}={s.Value}"));

                Console.WriteLine(
                    $"Rollback Bet Cancel: {market.Id}: '{market.GetName(CultureEn)}'; specifiers: {specifiers}");
            }
        }

        private static async void OnBetStopReceived(object sender, BetStopEventArgs<ISportEvent> eventArgs)
        {
            Console.WriteLine(
                $"Bet stop in {await eventArgs.GetBetStop().Event.GetNameAsync(Feed.AvailableLanguages().First())}");
        }

        private static void OnUnparsableMessageReceived(object sender, UnparsableMessageEventArgs e)
        {
            Console.WriteLine($"On Unparsable Message Received in {e.MessageType}");
        }

        private static void OnRawFeedMessageReceived(object sender, RawMessageEventArgs e)
        {
            var producer = e.Producer;
            if (producer.Length > 0)
            {
                Console.WriteLine($"Received a RAW message: {e.MessageType} from: {producer}");
            }
            else
            {
                // not all messages (ALIVE) have a producer
                Console.WriteLine($"Received a RAW message: {e.MessageType}");
            }
        }

        private static void OnEventRecoveryComplete(object sender, EventRecoveryCompletedEventArgs eventArgs)
        {
            Console.WriteLine(
                $"Event recovery completed [event id: {eventArgs.GetEventId()}, request id: {eventArgs.GetRequestId()}]");
        }

        private static void OnProducerDown(object sender, ProducerStatusChangeEventArgs eventArgs)
        {
            Console.WriteLine($"OnProducerDown: {eventArgs.GetProducerStatusChange().Producer.Name}");
        }

        private static void OnProducerUp(object sender, ProducerStatusChangeEventArgs eventArgs)
        {
            Console.WriteLine($"OnProducerUp: {eventArgs.GetProducerStatusChange().Producer.Name}");
        }

        private static void OnConnectionException(object sender, ConnectionExceptionEventArgs eventArgs)
        {
            Console.WriteLine($"OnConnectionException: {eventArgs.Exception}");
        }

        private static void OnDisconnected(object sender, EventArgs eventArgs)
        {
            Console.WriteLine("OnDisconnected");
        }

        private static void OnClosed(object sender, FeedCloseEventArgs eventArgs)
        {
            Console.WriteLine($"OnClosed: {eventArgs.GetReason()}");
        }
    }
}

﻿using Microsoft.Extensions.Logging;
using Oddin.OddinSdk.SDK;
using Oddin.OddinSdk.SDK.Configuration.Abstractions;
using Oddin.OddinSdk.SDK.Configuration;
using Oddin.OddinSdk.SDK.AMQP.EventArguments;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.FeedConfiguration;
using Oddin.OddinSdk.SDK.Sessions;
using Serilog;
using System;
using System.Globalization;

namespace Oddin.OddinSdk.SampleIntegration
{
    class Program
    {
        private static CultureInfo EnglishCulture => new CultureInfo("en-US");

        static void Main(string[] args)
        {
            var serilogLogger = new LoggerConfiguration()
                .MinimumLevel.Warning()
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

            var session = feed.CreateBuilder()
                .SetMessageInterest(MessageInterest.AllMessages)
                .Build();

            session.OnOddsChange += OnOddsChangeReceived;
            session.OnBetStop += OnBetStopReceived;

            feed.Open();
            Console.ReadLine();
            feed.Close();

            session.OnOddsChange -= OnOddsChangeReceived;
            session.OnBetStop -= OnBetStopReceived;
        }

        private static async void OnOddsChangeReceived(object sender, OddsChangeEventArgs<ISportEvent> eventArgs)
        {
            Console.WriteLine($"Odds changed in {await eventArgs.GetOddsChange().Event.GetNameAsync(EnglishCulture)}");
        }

        private static async void OnBetStopReceived(object sender, BetStopEventArgs<ISportEvent> eventArgs)
        {
            Console.WriteLine($"Bet stop in {await eventArgs.GetBetStop().Event.GetNameAsync(EnglishCulture)}");
        }
    }
}

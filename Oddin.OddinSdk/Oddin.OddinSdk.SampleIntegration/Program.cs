using Microsoft.Extensions.Logging;
using Oddin.OddinSdk.SDK;
using Oddin.OddinSdk.SDK.Configuration.Abstractions;
using Oddin.OddinSdk.SDK.Configuration;
using Serilog;
using System;

namespace Oddin.OddinSdk.SampleIntegration
{
    class Program
    {
        static void Main(string[] args)
        {
            var serilogLogger = new LoggerConfiguration()
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
            //foreach (var producer in feed.ProducerManager.Producers)
            //    Console.WriteLine(producer.Name);

            feed.Open();
            Console.ReadLine();
            feed.Close();
        }

        private static void OnDummyFeedMessageReceived(object _, string message)
        {
            Console.WriteLine(message);
        }
    }
}

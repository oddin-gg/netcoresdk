using Microsoft.Extensions.Logging;
using Oddin.OddinSdk.SDK;
using Oddin.OddinSdk.SampleIntegration;
using Oddin.OddinSdk.SDK.FeedConfiguration;
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

            var config = new CustomOddsFeedConfiguration(
                accessToken: "1a0c5a30-74ed-416d-b120-8c05f92e382f",
                apiHost: "api-mq.integration.oddin.gg",
                useApiSsl: true,
                httpClientTimeout: 10,
                host: "mq.integration.oddin.gg",
                port: 5672,
                ExceptionHandlingStrategy.THROW);

            var feed = new Feed(config, loggerFactory);
            //foreach (var producer in feed.ProducerManager.Producers)
            //    Console.WriteLine(producer.Name);

            feed.DummyFeedMessageReceived += OnDummyFeedMessageReceived;
            feed.Open();
            Console.ReadLine();
            feed.Close();
            feed.DummyFeedMessageReceived -= OnDummyFeedMessageReceived;
        }

        private static void OnDummyFeedMessageReceived(object _, string message)
        {
            Console.WriteLine(message);
        }
    }
}

using Microsoft.Extensions.Logging;
using Oddin.OddinSdk.SDK;
using Oddin.OddinSdk.SampleIntegration;
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
                httpClientTimeout: 10);

            var feed = new Feed(config, loggerFactory);
            foreach (var producer in feed.ProducerManager.Producers)
                Console.WriteLine(producer.Name);
        }
    }
}

using Microsoft.Extensions.Logging;
using Oddin.Oddin.SDK;
using Serilog;
using System;

namespace Oddin.Oddin.SampleIntegration
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

            var feed = new Feed(loggerFactory);
            foreach (var producer in feed.ProducerManager.Producers)
                Console.WriteLine(producer.Name);
        }
    }
}

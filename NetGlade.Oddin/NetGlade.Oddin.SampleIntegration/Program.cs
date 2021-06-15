
using Oddin.Oddin.SDK;
using System;

namespace Oddin.Oddin.SampleIntegration
{
    class Program
    {
        static void Main(string[] args)
        {
            var feed = new Feed();
            foreach (var producer in feed.ProducerManager.Producers)
                Console.WriteLine(producer.Name);
        }
    }
}

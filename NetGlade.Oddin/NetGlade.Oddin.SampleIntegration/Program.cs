
using NetGlade.Oddin.SDK;
using System;

namespace NetGlade.Oddin.SampleIntegration
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

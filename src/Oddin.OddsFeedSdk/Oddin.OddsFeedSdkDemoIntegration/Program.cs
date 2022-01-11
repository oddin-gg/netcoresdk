using System;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdkDemoIntegration
{
    public static class Program
    {
        // Put you token here:
        private const string TOKEN = "your-token";

        static async Task Main(string[] _)
        {
            Console.WriteLine("Select example:");
            Console.WriteLine("G = General");
            Console.WriteLine("R = Replay");
            Console.Write("Enter letter: ");
            var key = Console.ReadKey().KeyChar;
            Console.WriteLine();

            switch (char.ToUpper(key))
            {
                case 'R':
                {
                    await ReplayExample.Run(TOKEN);
                    break;
                }
                default:
                {
                    await GeneralExample.Run(TOKEN);
                    break;
                }
            }
        }
    }
}

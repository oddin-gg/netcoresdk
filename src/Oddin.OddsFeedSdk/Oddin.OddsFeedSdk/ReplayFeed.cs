using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.Managers.Abstractions;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Oddin.OddsFeedSdk
{
    public class ReplayFeed : Feed, IReplayOddsFeed
    {
        public IReplayManager ReplayManager
            => Services.GetService<IReplayManager>();

        public ReplayFeed(IFeedConfiguration config, ILoggerFactory loggerFactory = null)
            : base(config, true, loggerFactory)
        {
        }
    }
}

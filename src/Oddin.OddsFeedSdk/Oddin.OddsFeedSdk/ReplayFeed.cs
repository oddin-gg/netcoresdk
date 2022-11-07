using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.Abstractions;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Managers.Abstractions;

namespace Oddin.OddsFeedSdk;

public class ReplayFeed : Feed, IReplayOddsFeed
{
    public ReplayFeed(IFeedConfiguration config, ILoggerFactory loggerFactory = null)
        : base(config, true, loggerFactory)
    {
    }

    public IReplayManager ReplayManager => Services.GetService<IReplayManager>();
}
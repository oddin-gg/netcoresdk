using Oddin.OddsFeedSdk.Managers.Abstractions;

namespace Oddin.OddsFeedSdk.Abstractions;

public interface IReplayOddsFeed : IOddsFeed
{
    public IReplayManager ReplayManager { get; }
}
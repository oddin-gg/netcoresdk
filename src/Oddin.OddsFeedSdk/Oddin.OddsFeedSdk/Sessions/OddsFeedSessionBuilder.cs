using System;
using Oddin.OddsFeedSdk.Sessions.Abstractions;

namespace Oddin.OddsFeedSdk.Sessions;

internal class OddsFeedSessionBuilder : IOddsFeedSessionBuilder, ISessionBuilder
{
    private readonly Feed _feed;
    private bool _hasBeenSessionBuild;
    private MessageInterest _msgInterest;

    public OddsFeedSessionBuilder(Feed feed)
    {
        if (feed is null)
            throw new ArgumentNullException(nameof(feed));

        _feed = feed;
    }

    public ISessionBuilder SetMessageInterest(MessageInterest msgInterest)
    {
        _msgInterest = msgInterest;
        return this;
    }

    public IOddsFeedSession Build()
    {
        if (_hasBeenSessionBuild)
        {
            throw new InvalidOperationException(
                $"The {nameof(IOddsFeedSession)} instance has already been built by the current instance of {nameof(ISessionBuilder)}!");
        }

        _hasBeenSessionBuild = true;
        return _feed.BuildSession(_msgInterest);
    }
}
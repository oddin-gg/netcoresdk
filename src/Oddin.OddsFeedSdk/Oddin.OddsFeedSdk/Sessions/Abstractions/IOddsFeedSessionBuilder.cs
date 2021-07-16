namespace Oddin.OddsFeedSdk.Sessions.Abstractions
{
    public interface IOddsFeedSessionBuilder
    {
        ISessionBuilder SetMessageInterest(MessageInterest messageInterest);
    }
}

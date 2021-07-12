namespace Oddin.OddinSdk.SDK.Sessions.Abstractions
{
    /// <summary>
    /// Represents a first step when building an <see cref="IOddsFeedSession"/> instance
    /// </summary>
    public interface IOddsFeedSessionBuilder
    {
        /// <summary>
        /// Sets a <see cref="MessageInterest"/> specifying which type of messages should be received from the feed
        /// </summary>
        /// <param name="messageInterest">a <see cref="MessageInterest"/> specifying which type of messages should be received from the feed</param>
        /// <returns>A <see cref="ISessionBuilder"/> representing the second step when building a <see cref="IOddsFeedSession"/> instance</returns>
        ISessionBuilder SetMessageInterest(MessageInterest messageInterest);
    }
}

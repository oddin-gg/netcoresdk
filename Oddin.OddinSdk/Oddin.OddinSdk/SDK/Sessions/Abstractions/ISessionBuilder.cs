namespace Oddin.OddinSdk.SDK.Sessions.Abstractions
{
    /// <summary>
    /// Represents a second step when building an <see cref="IOddsFeedSession"/> instance
    /// </summary>
    public interface ISessionBuilder
    {
        /// <summary>
        /// Builds and returns a <see cref="IOddsFeedSession"/> instance
        /// </summary>
        /// <returns>the built <see cref="IOddsFeedSession"/> instance</returns>
        IOddsFeedSession Build();
    }
}

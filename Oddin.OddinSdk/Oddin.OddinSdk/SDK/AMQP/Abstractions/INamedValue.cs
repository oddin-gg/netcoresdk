namespace Oddin.OddinSdk.SDK.AMQP.Abstractions
{
    /// <summary>
    /// Specifies a contract implemented by classes representing values with names / descriptions
    /// </summary>
    public interface INamedValue
    {
        /// <summary>
        /// Gets the value associated with the current instance
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets the description associated with the current instance
        /// </summary>
        string Description { get; }
    }
}

namespace Oddin.OddinSdk.SDK.API.Entities.Abstractions
{
    public interface IOutcomeDescription
    {
        /// <summary>
        /// Gets a value uniquely identifying the current outcome
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets name of the current outcome
        /// </summary>
        string Name { get; }
    }
}

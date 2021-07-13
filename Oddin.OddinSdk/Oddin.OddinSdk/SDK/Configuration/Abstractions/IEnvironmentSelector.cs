namespace Oddin.OddinSdk.SDK.Configuration.Abstractions
{
    /// <summary>
    /// Defines a contract implemented by classes taking care of the 2nd step when building configuration - selecting the environment.
    /// </summary>
    public interface IEnvironmentSelector
    {
        /// <summary>
        /// Returns a <see cref="IConfigurationBuilder"/> with properties set to values needed to access integration environment
        /// </summary>
        /// <returns>A <see cref="IConfigurationBuilder"/> with properties set to values needed to access integration environment</returns>
        IConfigurationBuilder SelectIntegration();

        /// <summary>
        /// Returns a <see cref="IConfigurationBuilder"/> that has properties set to production environment
        /// </summary>
        IConfigurationBuilder SelectProduction();

        /// <summary>
        /// Returns a <see cref="IReplayConfigurationBuilder"/>
        /// </summary>
        IReplayConfigurationBuilder SelectReplay();
    }
}

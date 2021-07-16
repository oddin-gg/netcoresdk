namespace Oddin.OddsFeedSdk.Configuration.Abstractions
{
    public interface IEnvironmentSelector
    {
        IConfigurationBuilder SelectIntegration();

        IConfigurationBuilder SelectProduction();

        IReplayConfigurationBuilder SelectReplay();
    }
}

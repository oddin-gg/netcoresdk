namespace Oddin.OddsFeedSdk.Configuration.Abstractions
{
    public interface IEnvironmentSelector
    {
        IConfigurationBuilder SelectIntegration();

        IConfigurationBuilder SelectProduction();

        IReplayConfigurationBuilder SelectReplay();

        IConfigurationBuilder SelectEnvironment(string host, string apiHost, int port = SdkDefaults.DefaultPort);
    }
}

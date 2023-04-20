namespace Oddin.OddsFeedSdk.Configuration.Abstractions
{
    public interface IEnvironmentSelector
    {
        IConfigurationBuilder SelectIntegration();

        IConfigurationBuilder SelectProduction();

        IConfigurationBuilder SelectTest();

        IReplayConfigurationBuilder SelectReplay();

        IConfigurationBuilder SelectEnvironment(string host, string apiHost, int port = SdkDefaults.DefaultPort);
    }
}

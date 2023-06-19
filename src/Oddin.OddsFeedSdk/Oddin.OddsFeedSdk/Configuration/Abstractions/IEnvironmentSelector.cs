namespace Oddin.OddsFeedSdk.Configuration.Abstractions
{
    public interface IEnvironmentSelector
    {
        IConfigurationBuilder SelectIntegration(string region = Region.DEFAULT);

        IConfigurationBuilder SelectProduction(string region = Region.DEFAULT);

        IConfigurationBuilder SelectTest(string region = Region.DEFAULT);

        IReplayConfigurationBuilder SelectReplay();

        IConfigurationBuilder SelectEnvironment(string host, string apiHost, int port = SdkDefaults.DefaultPort);
    }
}

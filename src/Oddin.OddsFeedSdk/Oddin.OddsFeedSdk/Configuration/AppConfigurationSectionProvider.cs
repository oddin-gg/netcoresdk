using System.Threading;

namespace Oddin.OddsFeedSdk.Configuration;

internal class AppConfigurationSectionProvider : IAppConfigurationSectionProvider
{
    private readonly SemaphoreSlim _lock = new(1, 1);

    private AppConfigurationSection _config;

    public AppConfigurationSection Get()
    {
        _lock.Wait();
        try
        {
            if (_config == null)
                _config = AppConfigurationSection.LoadFromFile();
        }
        finally
        {
            _lock.Release();
        }

        return _config;
    }
}
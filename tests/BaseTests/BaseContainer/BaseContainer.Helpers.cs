using Autofac;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.BaseTests;

public partial class BaseContainer
{
    public async Task SetDownloadSpeedLimit(Action<UnitTestDataConfig>? options = null)
    {
        var config = new UnitTestDataConfig();
        options?.Invoke(config);

        var plexServers = await PlexRipperDbContext.PlexServers.ToListAsync();
        foreach (var plexServer in plexServers)
            GetServerSettings.SetDownloadSpeedLimit(plexServer.MachineIdentifier, config.DownloadSpeedLimitInKib);
    }

    public T Resolve<T>()
        where T : notnull => _lifeTimeScope.Resolve<T>();

    public List<PlexMockServer> PlexMockServers => _factory.PlexMockServers;

    public void Dispose()
    {
        _log.WarningLine("Disposing Container");
        PlexRipperDbContext.Database.EnsureDeleted();
        _lifeTimeScope.Dispose();
        _factory.Dispose();
        ApiClient.Dispose();
    }
}

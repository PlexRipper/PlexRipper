using Microsoft.EntityFrameworkCore;

namespace PlexRipper.BaseTests;

public partial class BaseContainer
{
    public async Task SetDownloadSpeedLimit(Action<UnitTestDataConfig> options = null)
    {
        var config = new UnitTestDataConfig();
        options?.Invoke(config);

        var plexServers = await PlexRipperDbContext.PlexServers.ToListAsync();
        GetServerSettings.EnsureAllServersHaveASettingsEntry(plexServers);
        foreach (var plexServer in plexServers)
            GetServerSettings.SetDownloadSpeedLimit(plexServer.MachineIdentifier, config.DownloadSpeedLimitInKib);
    }
}
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain.DownloadManager;

namespace PlexRipper.BaseTests;

public partial class BaseContainer
{
    public async Task SetDownloadSpeedLimit([CanBeNull] Action<UnitTestDataConfig> options = null)
    {
        var config = new UnitTestDataConfig();
        options?.Invoke(config);

        var plexServers = await PlexRipperDbContext.PlexServers.ToListAsync();
        foreach (var plexServer in plexServers)
            GetServerSettings.AddServerToSettings(new PlexServerSettingsModel
            {
                PlexServerName = plexServer.Name,
                MachineIdentifier = plexServer.MachineIdentifier,
                DownloadSpeedLimit = config.DownloadSpeedLimit,
            });
    }
}
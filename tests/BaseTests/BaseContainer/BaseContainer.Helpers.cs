using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain.DownloadManager;

namespace PlexRipper.BaseTests
{
    public partial class BaseContainer
    {
        public async Task SetDownloadSpeedLimit(UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();

            var plexServers = await PlexRipperDbContext.PlexServers.ToListAsync();
            foreach (var plexServer in plexServers)
            {
                GetUserSettings.SetDownloadSpeedLimit(new DownloadSpeedLimitModel
                {
                    PlexServerId = plexServer.Id,
                    MachineIdentifier = plexServer.MachineIdentifier,
                    DownloadSpeedLimit = config.DownloadSpeedLimit,
                });
            }
        }
    }
}
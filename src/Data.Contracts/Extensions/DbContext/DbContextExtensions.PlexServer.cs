using Microsoft.EntityFrameworkCore;

namespace Data.Contracts;

public static partial class DbContextExtensions
{
    public static async Task<string> GetPlexServerNameById(
        this IPlexRipperDbContext dbContext,
        int plexServerId,
        CancellationToken cancellationToken = default
    )
    {
        var plexServerName = await dbContext
            .PlexServers.Where(x => x.Id == plexServerId)
            .Select(x => x.Name)
            .FirstOrDefaultAsync(cancellationToken);
        return plexServerName ?? "Server Name Not Found";
    }

    public static async Task<string> GetPlexServerMachineIdentifierById(
        this IPlexRipperDbContext dbContext,
        int plexServerId,
        CancellationToken cancellationToken = default
    )
    {
        var plexServer = await dbContext.PlexServers.GetAsync(plexServerId, cancellationToken);
        return plexServer?.MachineIdentifier ?? string.Empty;
    }
}

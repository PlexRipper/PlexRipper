using Microsoft.EntityFrameworkCore;

namespace Reaparr.Data.Contracts;

public static partial class DbContextExtensions
{
    public static async Task<string> GetPlexServerNameById(
        this IReaparrDbContext dbContext,
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
        this IReaparrDbContext dbContext,
        int plexServerId,
        CancellationToken cancellationToken = default
    )
    {
        var plexServer = await dbContext.PlexServers.GetAsync(plexServerId, cancellationToken);
        return plexServer?.MachineIdentifier ?? string.Empty;
    }

    public static async Task<bool> IsServerOnline(
        this IReaparrDbContext dbContext,
        int plexServerId,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext
            .PlexServerStatuses.Where(x => x.PlexServerId == plexServerId && x.IsSuccessful)
            .AnyAsync(cancellationToken);
    }
}

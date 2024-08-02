using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

public static partial class DbContextExtensions
{
    public static Task<List<PlexServer>> GetAllPlexServersByPlexAccountIdQuery(
        this IPlexRipperDbContext dbContext,
        int plexAccountId,
        CancellationToken cancellationToken = default
    )
    {
        return dbContext
            .PlexAccountServers.Include(x => x.PlexServer)
            .ThenInclude(x => x.ServerStatus)
            .Include(x => x.PlexServer)
            .ThenInclude(x => x.PlexServerConnections)
            .ThenInclude(x => x.PlexServerStatus.OrderByDescending(y => y.LastChecked).Take(1))
            .Where(x => x.PlexAccountId == plexAccountId)
            .Select(x => x.PlexServer)
            .ToListAsync(cancellationToken);
    }

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

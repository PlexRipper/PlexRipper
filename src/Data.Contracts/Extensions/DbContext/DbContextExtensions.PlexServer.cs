using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

public static partial class DbContextExtensions
{
    public static Task<List<PlexServer>> GetAllPlexServersByPlexAccountIdQuery(
        this IPlexRipperDbContext dbContext,
        IMapper mapper,
        int plexAccountId,
        CancellationToken cancellationToken = default)
    {
        return dbContext
            .PlexAccountServers
            .Include(x => x.PlexServer)
            .ThenInclude(x => x.ServerStatus)
            .Include(x => x.PlexServer)
            .ThenInclude(x => x.PlexServerConnections)
            .ThenInclude(x => x.PlexServerStatus.OrderByDescending(y => y.LastChecked).Take(1))
            .Where(x => x.PlexAccountId == plexAccountId)
            .ProjectTo<PlexServer>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public static async Task<string> GetPlexServerNameById(this IPlexRipperDbContext dbContext, int plexServerId, CancellationToken cancellationToken = default)
    {
        var plexServer = await dbContext.PlexServers.GetAsync(plexServerId, cancellationToken);
        return plexServer?.Name ?? "Server Name Not Found";
    }

    public static async Task<string> GetPlexServerMachineIdentifierById(
        this IPlexRipperDbContext dbContext,
        int plexServerId,
        CancellationToken cancellationToken = default)
    {
        var plexServer = await dbContext.PlexServers.GetAsync(plexServerId, cancellationToken);
        return plexServer?.MachineIdentifier ?? string.Empty;
    }
}
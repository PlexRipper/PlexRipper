using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts.Extensions;

public static class DbContextExtensions
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
}
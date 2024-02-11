using FluentResults;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

public static class PlexAccountExtensions
{
    public static async Task<Result<List<PlexAccount>>> GetPlexAccountsWithAccessAsync(
        this IPlexRipperDbContext dbContext,
        int plexServerId,
        CancellationToken cancellationToken = default)
    {
        if (plexServerId <= 0)
            return ResultExtensions.IsInvalidId(nameof(plexServerId), plexServerId).LogError();

        var query = await dbContext.PlexAccountServers
            .Include(x => x.PlexAccount)
            .Where(x => x.PlexServerId == plexServerId)
            .Select(x => x.PlexAccount)
            .ToListAsync(cancellationToken);

        if (!query.Any())
            return Result.Fail($"There were no PlexAccounts that have access to PlexServer with id: {plexServerId}").LogError();

        return Result.Ok(query);
    }
}
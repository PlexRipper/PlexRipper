using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

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

    public static async Task<string> GetPlexServerNameById(this IPlexRipperDbContext dbContext, int plexServerId, CancellationToken cancellationToken = default)
    {
        var plexServer = await dbContext.PlexServers.GetAsync(plexServerId, cancellationToken);
        return plexServer?.Name ?? "Server Name Not Found";
    }

    public static async Task<Result<DownloadTask>> GetDownloadTaskByMediaKeyQuery(
        this IPlexRipperDbContext dbContext,
        int plexServerId,
        int mediaKey,
        CancellationToken cancellationToken = default)
    {
        var downloadTask =
            await dbContext.DownloadTasks
                .IncludeDownloadTasks()
                .FirstOrDefaultAsync(x => x.PlexServerId == plexServerId && x.Key == mediaKey, cancellationToken);

        if (downloadTask is null)
            return Result.Fail<DownloadTask>($"Couldn't find a download task with key {mediaKey}, plexServerId {plexServerId}");

        return Result.Ok(downloadTask);
    }
}
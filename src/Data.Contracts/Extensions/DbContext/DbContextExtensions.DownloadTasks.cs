using FluentResults;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

public static partial class DbContextExtensions
{
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

    public static async Task DeleteDownloadWorkerTasksAsync(
        this IPlexRipperDbContext dbContext,
        int downloadTaskId,
        CancellationToken cancellationToken = default)
    {
        var downloadWorkerTasks = await dbContext.DownloadWorkerTasks.AsTracking()
            .Where(x => x.DownloadTaskId == downloadTaskId)
            .ToListAsync(cancellationToken);
        if (!downloadWorkerTasks.Any())
        {
            Result.Fail(
                    $"Could not find any {nameof(DownloadWorkerTask)}s assigned to {nameof(DownloadTask)} with id: {downloadTaskId}")
                .LogWarning();
            return;
        }

        dbContext.DownloadWorkerTasks.RemoveRange(downloadWorkerTasks);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
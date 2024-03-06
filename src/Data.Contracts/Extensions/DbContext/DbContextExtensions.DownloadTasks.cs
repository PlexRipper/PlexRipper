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
                .AsTracking()
                .IncludeDownloadTasks()
                .FirstOrDefaultAsync(x => x.PlexServerId == plexServerId && x.Key == mediaKey, cancellationToken);

        if (downloadTask is null)
            return Result.Fail<DownloadTask>($"Couldn't find a download task with key {mediaKey}, plexServerId {plexServerId}");

        return Result.Ok(downloadTask);
    }

    public static async Task UpdateDownloadTasksAsync(
        this IPlexRipperDbContext dbContext,
        List<DownloadTask> downloadTasks,
        CancellationToken cancellationToken = default)
    {
        var flattenedDownloadTasks = downloadTasks.Flatten(x => x.Children).ToList();

        await dbContext.BulkUpdateAsync(flattenedDownloadTasks, cancellationToken: cancellationToken);
        await dbContext.BulkUpdateAsync(flattenedDownloadTasks.SelectMany(x => x.DownloadWorkerTasks).ToList(),
            cancellationToken: cancellationToken);
    }

    public static async Task UpdateDownloadTasksAsync(
        this IPlexRipperDbContext dbContext,
        DownloadTask downloadTask,
        CancellationToken cancellationToken = default)
    {
        await dbContext.UpdateDownloadTasksAsync(new List<DownloadTask>() { downloadTask }, cancellationToken);
    }

    public static async Task<Result<string>> GetDownloadUrl(
        this IPlexRipperDbContext dbContext,
        int plexServerId,
        string fileLocationUrl,
        CancellationToken cancellationToken = default)
    {
        var plexServerConnectionResult = await dbContext.GetValidPlexServerConnection(plexServerId, cancellationToken);
        if (plexServerConnectionResult.IsFailed)
            return plexServerConnectionResult.ToResult().LogError();

        var plexServerConnection = plexServerConnectionResult.Value;
        var plexServer = plexServerConnection.PlexServer;

        var tokenResult = await dbContext.GetPlexServerTokenAsync(plexServerId, cancellationToken);
        if (tokenResult.IsFailed)
        {
            _log.Error("Could not find a valid token for server {ServerName}", plexServer.Name);
            return tokenResult.ToResult();
        }

        var downloadUrl = plexServerConnection.GetDownloadUrl(fileLocationUrl, tokenResult.Value);
        return Result.Ok(downloadUrl);
    }
}
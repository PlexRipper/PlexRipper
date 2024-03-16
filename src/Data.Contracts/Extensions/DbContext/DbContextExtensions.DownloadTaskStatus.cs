using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

public static partial class DbContextExtensions
{
    /// <summary>
    /// This will determine the download status of the download task and it's children. It will start from the lower nested hierarchy and traverse up to the root to determine the <see cref="DownloadStatus"/>.
    /// </summary>
    /// <param name="dbContext">The <see cref="IPlexRipperDbContext"/> to extend from. </param>
    /// <param name="key">The <see cref="DownloadTaskKey"/> to traverse from. </param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe. </param>
    public static async Task DetermineDownloadStatus(
        this IPlexRipperDbContext dbContext,
        DownloadTaskKey key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var downloadTask = await dbContext.GetDownloadTaskAsync(key, cancellationToken);

            var downloadTaskCheck = downloadTask;
            while (downloadTaskCheck != null)
            {
                var parentKey = downloadTaskCheck.ToParentKey();
                switch (downloadTaskCheck.DownloadTaskType)
                {
                    case DownloadTaskType.Movie:
                    {
                        var downloadStatusList = downloadTaskCheck.Children.Select(x => x.DownloadStatus).ToList();
                        await Update(parentKey, downloadStatusList);
                        break;
                    }
                    case DownloadTaskType.TvShow:
                    {
                        var downloadStatusList = downloadTaskCheck.Children.SelectMany(x => x.Children)
                            .SelectMany(x => x.Children)
                            .Select(x => x.DownloadStatus)
                            .ToList();
                        await Update(parentKey, downloadStatusList);
                        break;
                    }
                    case DownloadTaskType.Season:
                    {
                        var downloadStatusList = downloadTaskCheck.Children.SelectMany(x => x.Children).Select(x => x.DownloadStatus).ToList();
                        await Update(parentKey, downloadStatusList);
                        break;
                    }
                    case DownloadTaskType.Episode:
                    {
                        var downloadStatusList = downloadTaskCheck.Children.Select(x => x.DownloadStatus).ToList();
                        await Update(parentKey, downloadStatusList);
                        break;
                    }

                    // The DownloadStatus here is determined by PlexDownloadClient and the FileMerger
                    case DownloadTaskType.MovieData:
                    case DownloadTaskType.EpisodeData:
                    {
                        downloadTaskCheck = parentKey is not null ? await dbContext.GetDownloadTaskAsync(parentKey, cancellationToken) : null;
                        break;
                    }
                    default:
                        _log.Error("DownloadTaskType {DownloadTaskType} is not supported in {CalculateDownloadStatusName}", downloadTaskCheck.DownloadTaskType,
                            nameof(DetermineDownloadStatus), 0);
                        downloadTaskCheck = null;
                        break;
                }
            }

            return;

            [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
            async Task Update(DownloadTaskKey? parentKey, List<DownloadStatus> downloadStatusList)
            {
                var newStatus = DownloadTaskActions.Aggregate(downloadStatusList);
                if (downloadTaskCheck.DownloadStatus != newStatus)
                {
                    downloadTaskCheck.DownloadStatus = newStatus;
                    await dbContext.SetDownloadStatus(downloadTaskCheck.ToKey(), downloadTaskCheck.DownloadStatus);
                }

                downloadTaskCheck = parentKey is not null ? await dbContext.GetDownloadTaskAsync(parentKey, cancellationToken) : null;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _log.Error(ex);
            throw;
        }
    }

    public static async Task SetDownloadStatus(
        this IPlexRipperDbContext dbContext,
        DownloadTaskKey key,
        DownloadStatus status)
    {
        await dbContext.SetDownloadStatus(key.Id, status, key.Type);
    }

    /// <summary>
    /// Sets the <see cref="DownloadStatus"/> of the download task but not its children or parent and immediately saves the changes to the database.
    /// </summary>
    /// <param name="dbContext"> The <see cref="IPlexRipperDbContext"/> to extend from. </param>
    /// <param name="id"> The <see cref="Guid"/> of the download task to update. </param>
    /// <param name="status"> The <see cref="DownloadStatus"/> to set. </param>
    /// <param name="type"> The <see cref="DownloadTaskType"/>  of the DownloadTask, not required. </param>
    /// <exception cref="ArgumentOutOfRangeException"> Thrown when the <see cref="DownloadTaskType"/> is not supported. </exception>
    public static async Task SetDownloadStatus(
        this IPlexRipperDbContext dbContext,
        Guid id,
        DownloadStatus status,
        DownloadTaskType type = DownloadTaskType.None)
    {
        if (type == DownloadTaskType.None)
            type = await dbContext.GetDownloadTaskTypeAsync(id);

        switch (type)
        {
            case DownloadTaskType.Movie:
                await dbContext.DownloadTaskMovie
                    .Where(x => x.Id == id)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, status));
                break;
            case DownloadTaskType.MovieData:
                await dbContext.DownloadTaskMovieFile
                    .Where(x => x.Id == id)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, status));
                break;
            case DownloadTaskType.TvShow:
                await dbContext.DownloadTaskTvShow
                    .Where(x => x.Id == id)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, status));
                break;
            case DownloadTaskType.Season:
                await dbContext.DownloadTaskTvShowSeason
                    .Where(x => x.Id == id)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, status));
                break;
            case DownloadTaskType.Episode:
                await dbContext.DownloadTaskTvShowEpisode
                    .Where(x => x.Id == id)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, status));
                break;
            case DownloadTaskType.EpisodeData:
                await dbContext.DownloadTaskTvShowEpisodeFile
                    .Where(x => x.Id == id)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, status));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
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
            var parentKey = key;
            while (parentKey != null)
            {
                switch (parentKey.Type)
                {
                    case DownloadTaskType.Movie:
                    {
                        var downloadTask = await dbContext.DownloadTaskMovie
                            .AsTracking()
                            .Include(x => x.Children)
                            .GetAsync(parentKey.Id, cancellationToken);
                        var downloadStatusList = downloadTask?.Children.Select(x => x.DownloadStatus).ToList() ?? [];
                        Update(downloadTask, downloadStatusList);
                        break;
                    }
                    case DownloadTaskType.TvShow:
                    {
                        var downloadTask = await dbContext.DownloadTaskTvShow
                            .AsTracking()
                            .Include(x => x.Children)
                            .ThenInclude(x => x.Children)
                            .ThenInclude(x => x.Children)
                            .GetAsync(parentKey.Id, cancellationToken);
                        var downloadStatusList = downloadTask?.Children.SelectMany(x => x.Children.SelectMany(y => y.Children))
                            .Select(x => x.DownloadStatus)
                            .ToList() ?? [];
                        Update(downloadTask, downloadStatusList);
                        break;
                    }
                    case DownloadTaskType.Season:
                    {
                        var downloadTask = await dbContext.DownloadTaskTvShowSeason
                            .AsTracking()
                            .Include(x => x.Children)
                            .ThenInclude(x => x.Children)
                            .GetAsync(parentKey.Id, cancellationToken);
                        var downloadStatusList = downloadTask?.Children.SelectMany(x => x.Children).Select(x => x.DownloadStatus).ToList() ?? [];
                        Update(downloadTask, downloadStatusList);
                        break;
                    }
                    case DownloadTaskType.Episode:
                    {
                        var downloadTask = await dbContext.DownloadTaskTvShowEpisode
                            .AsTracking()
                            .Include(x => x.Children)
                            .GetAsync(parentKey.Id, cancellationToken);
                        var downloadStatusList = downloadTask?.Children.Select(x => x.DownloadStatus).ToList() ?? [];
                        Update(downloadTask, downloadStatusList);
                        break;
                    }

                    // The DownloadStatus here is determined by PlexDownloadClient and the FileMerger
                    case DownloadTaskType.MovieData:
                    {
                        parentKey = await dbContext.DownloadTaskMovieFile
                            .Where(x => x.Id == parentKey.Id)
                            .ProjectToParentKey()
                            .FirstOrDefaultAsync(cancellationToken);
                        break;
                    }

                    // The DownloadStatus here is determined by PlexDownloadClient and the FileMerger
                    case DownloadTaskType.EpisodeData:
                    {
                        parentKey = await dbContext.DownloadTaskTvShowEpisodeFile
                            .Where(x => x.Id == parentKey.Id)
                            .ProjectToParentKey()
                            .FirstOrDefaultAsync(cancellationToken);
                        break;
                    }
                    default:
                        _log.Error("DownloadTaskType {DownloadTaskType} is not supported in {DetermineDownloadStatus}", parentKey.Type,
                            nameof(DetermineDownloadStatus), 0);
                        break;
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            return;

            [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
            void Update(DownloadTaskBase? downloadTaskBase, List<DownloadStatus> downloadStatusList)
            {
                if (downloadTaskBase == null)
                {
                    _log.Error("DownloadTaskBase is null in {DetermineDownloadStatus}", nameof(DetermineDownloadStatus), 0);
                    return;
                }

                var newStatus = DownloadTaskActions.Aggregate(downloadStatusList);
                if (downloadTaskBase.DownloadStatus != newStatus)
                    downloadTaskBase.DownloadStatus = newStatus;

                parentKey = downloadTaskBase.ToParentKey();
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _log.Error(ex);
            throw;
        }
    }

    /// <summary>
    /// Sets the <see cref="DownloadStatus"/> of the download task but not its children or parent and immediately saves the changes to the database.
    /// </summary>
    /// <param name="dbContext"> The <see cref="IPlexRipperDbContext"/> to extend from. </param>
    /// <param name="key"> The <see cref="DownloadTaskKey"/> of the download task to update. </param>
    /// <param name="status"> The <see cref="DownloadStatus"/> to set. </param>
    public static async Task SetDownloadStatus(
        this IPlexRipperDbContext dbContext,
        DownloadTaskKey key,
        DownloadStatus status)
    {
        switch (key.Type)
        {
            case DownloadTaskType.Movie:
                await dbContext.DownloadTaskMovie
                    .Where(x => x.Id == key.Id)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, status));
                break;
            case DownloadTaskType.MovieData:
                await dbContext.DownloadTaskMovieFile
                    .Where(x => x.Id == key.Id)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, status));
                break;
            case DownloadTaskType.TvShow:
                await dbContext.DownloadTaskTvShow
                    .Where(x => x.Id == key.Id)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, status));
                break;
            case DownloadTaskType.Season:
                await dbContext.DownloadTaskTvShowSeason
                    .Where(x => x.Id == key.Id)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, status));
                break;
            case DownloadTaskType.Episode:
                await dbContext.DownloadTaskTvShowEpisode
                    .Where(x => x.Id == key.Id)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, status));
                break;
            case DownloadTaskType.EpisodeData:
                await dbContext.DownloadTaskTvShowEpisodeFile
                    .Where(x => x.Id == key.Id)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, status));
                break;
            default:
                throw new ArgumentOutOfRangeException($"{key.Type} is not supported in {nameof(SetDownloadStatus)}");
        }
    }
}
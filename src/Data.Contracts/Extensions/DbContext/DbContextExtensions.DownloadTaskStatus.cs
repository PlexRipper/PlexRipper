using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

public static partial class DbContextExtensions
{
    public static async Task CalculateDownloadStatus(
        this IPlexRipperDbContext dbContext,
        DownloadTaskKey key,
        CancellationToken cancellationToken = default)
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
                        nameof(CalculateDownloadStatus), 0);
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
                await dbContext.SetDownloadStatus(downloadTaskCheck.ToKey(), downloadTaskCheck.DownloadStatus, cancellationToken);
            }

            downloadTaskCheck = parentKey is not null ? await dbContext.GetDownloadTaskAsync(parentKey, cancellationToken) : null;
        }
    }

    public static async Task SetDownloadStatus(
        this IPlexRipperDbContext dbContext,
        DownloadTaskKey key,
        DownloadStatus status,
        CancellationToken cancellationToken = default)
    {
        await dbContext.SetDownloadStatus(key.Id, status, key.Type, cancellationToken);
    }

    public static async Task SetDownloadStatus(
        this IPlexRipperDbContext dbContext,
        Guid id,
        DownloadStatus status,
        DownloadTaskType type = DownloadTaskType.None,
        CancellationToken cancellationToken = default)
    {
        if (type == DownloadTaskType.None)
            type = await dbContext.GetDownloadTaskTypeAsync(id, cancellationToken);

        switch (type)
        {
            case DownloadTaskType.Movie:
                await dbContext.DownloadTaskMovie
                    .Where(x => x.Id == id)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, status), cancellationToken);
                break;
            case DownloadTaskType.MovieData:
                await dbContext.DownloadTaskMovieFile
                    .Where(x => x.Id == id)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, status), cancellationToken);
                break;
            case DownloadTaskType.TvShow:
                await dbContext.DownloadTaskTvShow
                    .Where(x => x.Id == id)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, status), cancellationToken);
                break;
            case DownloadTaskType.Season:
                await dbContext.DownloadTaskTvShowSeason
                    .Where(x => x.Id == id)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, status), cancellationToken);
                break;
            case DownloadTaskType.Episode:
                await dbContext.DownloadTaskTvShowEpisode
                    .Where(x => x.Id == id)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, status), cancellationToken);
                break;
            case DownloadTaskType.EpisodeData:
                await dbContext.DownloadTaskTvShowEpisodeFile
                    .Where(x => x.Id == id)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, status), cancellationToken);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

public static partial class DbSetExtensions
{
    public static IQueryable<DownloadTask> IncludeDownloadTasks(this IQueryable<DownloadTask> downloadTasks) => downloadTasks.IncludeDownloadTasks("");

    public static IQueryable<DownloadTask> IncludeByRoot(this IQueryable<DownloadTask> downloadTasks) => downloadTasks.Where(x => x.ParentId == null);

    private static IQueryable<T> IncludeDownloadTasks<T>(this IQueryable<T> query, string prefix = "") where T : class
    {
        if (!string.IsNullOrEmpty(prefix))
            query = query.Include(prefix.TrimEnd('.'));

        // The Include path 'Children->Parent' results in a cycle.
        // Cycles are not allowed in no-tracking queries; either use a tracking query or remove the cycle
        query = query.AsTracking();

        // Include downloadTask children up to 5 levels deep
        for (var i = 1; i <= 5; i++)
        {
            var childPath = prefix + string.Concat(Enumerable.Repeat($"{nameof(DownloadTask.Children)}.", i));

            query = query
                .Include($"{childPath}".TrimEnd('.'))
                .Include($"{childPath}{nameof(DownloadTask.DownloadFolder)}")
                .Include($"{childPath}{nameof(DownloadTask.DestinationFolder)}")
                .Include($"{childPath}{nameof(DownloadTask.DownloadWorkerTasks)}")
                .Include($"{childPath}{nameof(DownloadTask.PlexServer)}")
                .Include($"{childPath}{nameof(DownloadTask.PlexLibrary)}");
        }

        return query;
    }

    public static async Task SetDownloadStatusAsync(
        this IQueryable<DownloadTask> downloadTasks,
        int downloadTaskId,
        DownloadStatus downloadStatus,
        CancellationToken cancellationToken = default)
    {
        await downloadTasks
            .Where(x => x.Id == downloadTaskId)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, x => downloadStatus), cancellationToken);
    }

    public static IQueryable<DownloadTaskMovie> IncludeAll(this IQueryable<DownloadTaskMovie> downloadTasks) => downloadTasks
        .Include(x => x.PlexServer)
        .Include(x => x.PlexLibrary)
        .Include(x => x.Children)
        .ThenInclude(x => x.PlexServer)
        .Include(x => x.Children)
        .ThenInclude(x => x.PlexLibrary);

    public static IQueryable<DownloadTaskTvShow> IncludeAll(this IQueryable<DownloadTaskTvShow> downloadTasks) => downloadTasks
        .Include($"{nameof(DownloadTaskTvShow.PlexServer)}")
        .Include($"{nameof(DownloadTaskTvShow.PlexLibrary)}")
        .Include($"{nameof(DownloadTaskTvShow.Children)}.{nameof(DownloadTaskTvShowSeason.PlexServer)}")
        .Include($"{nameof(DownloadTaskTvShow.Children)}.{nameof(DownloadTaskTvShowSeason.PlexLibrary)}")
        .Include($"{nameof(DownloadTaskTvShow.Children)}.{nameof(DownloadTaskTvShowSeason.Children)}.{nameof(DownloadTaskTvShowEpisode.PlexLibrary)}")
        .Include($"{nameof(DownloadTaskTvShow.Children)}.{nameof(DownloadTaskTvShowSeason.Children)}.{nameof(DownloadTaskTvShowEpisode.PlexServer)}")
        .Include(
            $"{nameof(DownloadTaskTvShow.Children)}.{nameof(DownloadTaskTvShowSeason.Children)}.{nameof(DownloadTaskTvShowEpisode.Children)}.{nameof(DownloadTaskTvShowEpisodeFile.PlexLibrary)}")
        .Include(
            $"{nameof(DownloadTaskTvShow.Children)}.{nameof(DownloadTaskTvShowSeason.Children)}.{nameof(DownloadTaskTvShowEpisode.Children)}.{nameof(DownloadTaskTvShowEpisodeFile.PlexServer)}");

    public static IQueryable<DownloadTaskTvShowSeason> IncludeAll(this IQueryable<DownloadTaskTvShowSeason> downloadTasks) => downloadTasks
        .Include($"{nameof(DownloadTaskTvShowSeason.PlexServer)}")
        .Include($"{nameof(DownloadTaskTvShowSeason.PlexLibrary)}")
        .Include($"{nameof(DownloadTaskTvShowSeason.Children)}.{nameof(DownloadTaskTvShowEpisode.PlexServer)}")
        .Include($"{nameof(DownloadTaskTvShowSeason.Children)}.{nameof(DownloadTaskTvShowEpisode.PlexLibrary)}")
        .Include(
            $"{nameof(DownloadTaskTvShowSeason.Children)}.{nameof(DownloadTaskTvShowEpisode.Children)}.{nameof(DownloadTaskTvShowEpisodeFile.PlexLibrary)}")
        .Include(
            $"{nameof(DownloadTaskTvShowSeason.Children)}.{nameof(DownloadTaskTvShowEpisode.Children)}.{nameof(DownloadTaskTvShowEpisodeFile.PlexServer)}");

    public static IQueryable<DownloadTaskTvShowEpisode> IncludeAll(this IQueryable<DownloadTaskTvShowEpisode> downloadTasks) => downloadTasks
        .Include($"{nameof(DownloadTaskTvShowEpisode.PlexServer)}")
        .Include($"{nameof(DownloadTaskTvShowEpisode.PlexLibrary)}")
        .Include($"{nameof(DownloadTaskTvShowEpisode.Children)}.{nameof(DownloadTaskTvShowEpisodeFile.PlexServer)}")
        .Include($"{nameof(DownloadTaskTvShowEpisode.Children)}.{nameof(DownloadTaskTvShowEpisodeFile.PlexLibrary)}");
}
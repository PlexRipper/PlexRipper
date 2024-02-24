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
}
using Microsoft.EntityFrameworkCore;
using Reaparr.Domain;

namespace Reaparr.Data.Contracts;

public static partial class DbSetExtensions
{
    /// <summary>
    ///  Resets the status of <see cref="LibrarySyncJobStatus.Completed"/> or <see cref="LibrarySyncJobStatus.Failed"/> library sync jobs to <see cref="LibrarySyncJobStatus.Queued"/> for the specified library IDs.
    /// </summary>
    public static async Task<int> ResetLibrarySyncJobQueue(
        this IQueryable<LibrarySyncJobQueue> dbSet,
        List<int> libraryIds,
        CancellationToken token = default
    )
    {
        return await dbSet
            .Where(x =>
                libraryIds.Contains(x.PlexLibraryId)
                && (x.Status == LibrarySyncJobStatus.Completed || x.Status == LibrarySyncJobStatus.Failed)
            )
            .ResetJobsToQueuedAsync(token);
    }

    public static async Task<int> ResetJobsToQueuedAsync(
        this IQueryable<LibrarySyncJobQueue> queryable,
        CancellationToken token = default
    )
    {
        return await queryable.ExecuteUpdateAsync(
            x =>
                x.SetProperty(y => y.Status, LibrarySyncJobStatus.Queued)
                    .SetProperty(y => y.CreatedAt, DateTime.UtcNow)
                    .SetProperty(y => y.StartedAt, (DateTime?)null)
                    .SetProperty(y => y.CompletedAt, (DateTime?)null)
                    .SetProperty(y => y.ErrorMessage, (string?)null),
            token
        );
    }
}

using TickerQ.Utilities.Enums;

namespace Reaparr.Data.Contracts;

public static partial class DbContextExtensions
{
    public static Task<bool> HasActiveLibraryComparisonAsync(
        this IReaparrDbContext dbContext,
        IEnumerable<JobKey> jobKeys,
        CancellationToken cancellationToken
    ) => dbContext.TimeTickers.AnyAsync(
        x => jobKeys.Select(y => y.Name).Contains(x.JobKey)
             && x.JobType == JobTypes.LibraryComparisonJob
             && (x.Status == TickerStatus.Idle
                 || x.Status == TickerStatus.Queued
                 || x.Status == TickerStatus.InProgress),
        cancellationToken
    );
}
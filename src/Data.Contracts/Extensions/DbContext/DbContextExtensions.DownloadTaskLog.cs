using Reaparr.Domain;

namespace Reaparr.Data.Contracts;

public static partial class DbContextExtensions
{
    public static async Task CreateDownloadClientLog(
        this IReaparrDbContext dbContext,
        DownloadTaskKey downloadTaskKey,
        NotificationLevel logLevel,
        DownloadStatus status,
        string message
    )
    {
        await dbContext.DownloadTasksLogs.AddAsync(
            new DownloadTaskLog
            {
                Message = message,
                LogLevel = logLevel,
                Status = status,
                DownloadTaskId = downloadTaskKey.Id,
                CreatedAt = DateTime.UtcNow,
            }
        );
        await dbContext.SaveChangesAsync(CancellationToken.None);
    }

    public static async Task CreateDownloadClientLogs(this IReaparrDbContext dbContext, List<DownloadTaskLog> logs)
    {
        dbContext.DownloadTasksLogs.AddRange(logs);
        await dbContext.SaveChangesAsync(CancellationToken.None);
    }
}

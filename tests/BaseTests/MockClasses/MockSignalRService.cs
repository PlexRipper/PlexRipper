using System.Collections.Concurrent;
using Reaparr.Application.Contracts;
using Reaparr.Logging;
using Reaparr.WebAPI.Contracts;
using Serilog;

namespace Reaparr.BaseTests;

public class MockSignalRService : ISignalRService
{
    private readonly Serilog.ILogger _log;

    public BlockingCollection<DownloadTaskDTO> DownloadTaskUpdate { get; } = new();

    public BlockingCollection<ServerDownloadProgressDTO> ServerDownloadProgressList { get; } = new();
    public BlockingCollection<JobStatusUpdateDTO> JobStatusUpdateList { get; } = new();
    public BlockingCollection<RefreshDataType> RefreshNotificationList { get; } = new();

    public MockSignalRService(ILogger log)
    {
        _log = log.ForContext<MockSignalRService>();
    }

    public Task SendLibraryProgressUpdateAsync(LibraryProgress libraryProgress) => Task.CompletedTask;

    public Task SendLibraryProgressUpdateAsync(int id, int received, int total, bool isRefreshing = true) =>
        Task.CompletedTask;

    public Task SendDownloadTaskCreationProgressUpdate(int current, int total) => Task.CompletedTask;

    public Task SendNotificationAsync(Notification notification) => Task.CompletedTask;

    public Task SendServerSyncProgressUpdateAsync(SyncServerMediaProgress syncServerMediaProgress) =>
        Task.CompletedTask;

    public Task SendDownloadProgressUpdateAsync(
        List<DownloadTaskGeneric> downloadTasks,
        CancellationToken cancellationToken = default
    )
    {
        var update = downloadTasks.ToServerDownloadProgressDTOList();

        ServerDownloadProgressList.Add(update.First(), cancellationToken);
        _log.Here().Verbose("{ClassName} => {@DownloadTaskDto}", nameof(MockSignalRService), update.First());

        return Task.CompletedTask;
    }

    public Task SendServerConnectionCheckStatusProgressAsync(ServerConnectionCheckStatusProgress progress) =>
        Task.CompletedTask;

    public Task SendJobStatusUpdateAsync<T>(JobStatusUpdate<T> jobStatusUpdate)
        where T : class
    {
        JobStatusUpdateList.Add(jobStatusUpdate.ToDTO());
        _log.Here().Verbose("{ClassName} => {@JobStatusUpdate}", nameof(MockSignalRService), jobStatusUpdate);

        return Task.CompletedTask;
    }

    public Task SendRefreshNotificationAsync(RefreshDataType dataType, CancellationToken cancellationToken = default)
    {
        RefreshNotificationList.Add(dataType, cancellationToken);
        _log.Here().Verbose("{ClassName} => {@DataType}", nameof(MockSignalRService), dataType);

        return Task.CompletedTask;
    }

    public async Task SendRefreshNotificationAsync(
        List<RefreshDataType> dataTypes,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var dataType in dataTypes)
            await SendRefreshNotificationAsync(dataType, cancellationToken);
    }
}

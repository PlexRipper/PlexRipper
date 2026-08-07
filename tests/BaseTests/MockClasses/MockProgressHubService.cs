using System.Collections.Concurrent;

namespace Reaparr.BaseTests;

public class MockProgressHubService : IProgressHubService
{
    private readonly ILogger _log;

    public BlockingCollection<LibrarySyncProgressDTO> LibraryProgressUpdateList { get; } = new();

    public BlockingCollection<JobStatusUpdateDTO> JobStatusUpdateList { get; } = new();

    public BlockingCollection<LibraryComparisonCompletedDTO> LibraryComparisonCompletedList { get; } = new();

    public BlockingCollection<AppUpdateDownloadProgressDTO> AppDownloadProgressList { get; } = new();

    public MockProgressHubService(ILogger log)
    {
        _log = log.ForContext<MockProgressHubService>();
    }

    public Task SendLibraryProgressUpdateAsync(
        LibrarySyncProgressDTO progress,
        CancellationToken cancellationToken = default
    )
    {
        LibraryProgressUpdateList.Add(progress, cancellationToken);
        _log.Here().Verbose("{ClassName} => {@LibraryProgress}", nameof(MockProgressHubService), progress);
        return Task.CompletedTask;
    }

    public Task SendServerConnectionCheckStatusProgressAsync(
        ServerConnectionCheckStatusProgress progress,
        CancellationToken cancellationToken = default
    ) => Task.CompletedTask;

    public Task SendJobStatusUpdateAsync<T>(JobStatusUpdate<T> jobStatusUpdate)
        where T : class
    {
        JobStatusUpdateList.Add(jobStatusUpdate.ToDTO());
        _log.Here().Verbose("{ClassName} => {@JobStatusUpdate}", nameof(MockProgressHubService), jobStatusUpdate);

        return Task.CompletedTask;
    }

    public Task SendAppUpdateDownloadProgressAsync(
        AppUpdateDownloadProgressDTO progress,
        CancellationToken cancellationToken = default
    )
    {
        AppDownloadProgressList.Add(progress, cancellationToken);
        _log.Here().Verbose("{ClassName} => {@AppDownloadProgress}", nameof(MockProgressHubService), progress);
        return Task.CompletedTask;
    }
    
    
    public Task SendLibraryComparisonCompletedAsync(
        LibraryComparisonCompletedDTO notification,
        CancellationToken cancellationToken = default
    )
    {
        LibraryComparisonCompletedList.Add(notification, cancellationToken);
        _log.Here().Verbose("{ClassName} => {@Notification}", nameof(MockProgressHubService), notification);

        return Task.CompletedTask;
    }
}

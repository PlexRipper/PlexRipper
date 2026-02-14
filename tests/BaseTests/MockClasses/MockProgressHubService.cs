using System.Collections.Concurrent;
using Reaparr.SignalR.Contracts;

namespace Reaparr.BaseTests;

public class MockProgressHubService : IProgressHubService
{
    private readonly ILogger _log;

    public BlockingCollection<LibrarySyncProgressDTO> LibraryProgressUpdateList { get; } = new();

    public BlockingCollection<JobStatusUpdateDTO> JobStatusUpdateList { get; } = new();

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

    public Task SendJobStatusUpdateAsync<T>(
        JobStatusUpdate<T> jobStatusUpdate,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        JobStatusUpdateList.Add(jobStatusUpdate.ToDTO());
        _log.Here().Verbose("{ClassName} => {@JobStatusUpdate}", nameof(MockProgressHubService), jobStatusUpdate);

        return Task.CompletedTask;
    }
}

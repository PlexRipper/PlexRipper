using System.Collections.Concurrent;
using Reaparr.Application.Contracts;
using Reaparr.SignalR.Contracts;

namespace Reaparr.BaseTests;

public class MockProgressHubService : IProgressHubService
{
    private readonly ILogger _log;

    public BlockingCollection<JobStatusUpdateDTO> JobStatusUpdateList { get; } = new();

    public MockProgressHubService(ILogger log)
    {
        _log = log.ForContext<MockProgressHubService>();
    }

    public Task SendLibraryProgressUpdateAsync(LibrarySyncProgressDTO progress) => Task.CompletedTask;

    public Task SendServerConnectionCheckStatusProgressAsync(ServerConnectionCheckStatusProgress progress) =>
        Task.CompletedTask;

    public Task SendJobStatusUpdateAsync<T>(JobStatusUpdate<T> jobStatusUpdate)
        where T : class
    {
        JobStatusUpdateList.Add(jobStatusUpdate.ToDTO());
        _log.Here().Verbose("{ClassName} => {@JobStatusUpdate}", nameof(MockProgressHubService), jobStatusUpdate);

        return Task.CompletedTask;
    }
}

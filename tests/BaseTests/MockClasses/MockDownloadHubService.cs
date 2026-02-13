using System.Collections.Concurrent;
using Reaparr.Application.Contracts;
using Reaparr.SignalR.Contracts;

namespace Reaparr.BaseTests;

public class MockDownloadHubService : IDownloadHubService
{
    private readonly ILogger _log;

    public BlockingCollection<ServerDownloadProgressDTO> ServerDownloadProgressList { get; } = new();

    public MockDownloadHubService(ILogger log)
    {
        _log = log.ForContext<MockDownloadHubService>();
    }

    public Task SendDownloadProgressUpdateAsync(
        List<DownloadTaskGeneric> downloadTasks,
        CancellationToken cancellationToken = default
    )
    {
        var update = downloadTasks.ToServerDownloadProgressDTOList();

        if (!update.Any())
            return Task.CompletedTask;

        foreach (var dto in update)
        {
            ServerDownloadProgressList.Add(dto, cancellationToken);
            _log.Here().Verbose("{ClassName} => {@DownloadTaskDto}", nameof(MockDownloadHubService), dto);
        }

        return Task.CompletedTask;
    }
}

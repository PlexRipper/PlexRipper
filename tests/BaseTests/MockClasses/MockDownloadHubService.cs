using System.Collections.Concurrent;

namespace Reaparr.BaseTests;

public class MockDownloadHubService : IDownloadHubService
{
    private readonly ILogger _log;

    public BlockingCollection<ServerDownloadProgressDTO> ServerDownloadProgressList { get; } = new();

    public BlockingCollection<(
        int ServerId,
        long Sequence,
        List<DownloadPatchDTO> Upserts,
        List<Guid> DeletedIds
    )> DownloadPatchList { get; } = new();

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

    public Task SendDownloadPatchAsync(
        int plexServerId,
        long sequence,
        IReadOnlyCollection<DownloadPatchDTO> upserts,
        IReadOnlyCollection<Guid>? deletedIds = null,
        CancellationToken cancellationToken = default
    )
    {
        DownloadPatchList.Add(
            (plexServerId, sequence, upserts.ToList(), deletedIds?.ToList() ?? []),
            cancellationToken
        );
        return Task.CompletedTask;
    }
}

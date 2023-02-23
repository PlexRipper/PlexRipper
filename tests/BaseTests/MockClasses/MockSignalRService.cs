using System.Collections.Concurrent;
using AutoMapper;
using BackgroundServices.Contracts;
using PlexRipper.WebAPI.Common.DTO;
using WebAPI.Contracts;

namespace PlexRipper.BaseTests;

public class MockSignalRService : ISignalRService
{
    private readonly IMapper _mapper;
    public BlockingCollection<DownloadTaskDTO> DownloadTaskUpdate { get; } = new();

    public MockSignalRService(IMapper mapper)
    {
        _mapper = mapper;
    }

    public void SendLibraryProgressUpdate(LibraryProgress libraryProgress) { }

    public Task SendLibraryProgressUpdateAsync(int id, int received, int total, bool isRefreshing = true)
    {
        return Task.CompletedTask;
    }

    public Task SendDownloadTaskCreationProgressUpdate(int current, int total)
    {
        return Task.CompletedTask;
    }

    public Task SendDownloadTaskUpdateAsync(DownloadTask downloadTask, CancellationToken cancellationToken = default)
    {
        var downloadTaskDTO = _mapper.Map<DownloadTaskDTO>(downloadTask);
        DownloadTaskUpdate.Add(downloadTaskDTO, cancellationToken);

        return Task.CompletedTask;
    }

    public Task SendFileMergeProgressUpdateAsync(FileMergeProgress fileMergeProgress)
    {
        return Task.CompletedTask;
    }

    public Task SendNotificationAsync(Notification notification)
    {
        return Task.CompletedTask;
    }

    public Task SendServerInspectStatusProgressAsync(InspectServerProgress progress)
    {
        return Task.CompletedTask;
    }

    public Task SendServerSyncProgressUpdateAsync(SyncServerProgress syncServerProgress)
    {
        return Task.CompletedTask;
    }

    public Task SendDownloadProgressUpdateAsync(int plexServerId, List<DownloadTask> downloadTasks)
    {
        return Task.CompletedTask;
    }

    public Task SendServerConnectionCheckStatusProgressAsync(ServerConnectionCheckStatusProgress progress)
    {
        return Task.CompletedTask;
    }

    public Task SendJobStatusUpdateAsync(JobStatusUpdate jobStatusUpdate)
    {
        return Task.CompletedTask;
    }
}
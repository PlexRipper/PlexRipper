using System.Collections.Concurrent;
using AutoMapper;
using BackgroundServices.Contracts;
using Logging.Interface;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.SignalR.Common;
using WebAPI.Contracts;

namespace PlexRipper.BaseTests;

public class MockSignalRService : ISignalRService
{
    private readonly ILog<MockSignalRService> _log;
    private readonly IMapper _mapper;
    public BlockingCollection<DownloadTaskDTO> DownloadTaskUpdate { get; } = new();
    public BlockingCollection<FileMergeProgress> FileMergeProgressList { get; } = new();

    public BlockingCollection<ServerDownloadProgressDTO> ServerDownloadProgressList { get; } = new();

    public MockSignalRService(ILog<MockSignalRService> log, IMapper mapper)
    {
        _log = log;
        _mapper = mapper;
    }

    public void SendLibraryProgressUpdate(LibraryProgress libraryProgress) { }

    public Task SendLibraryProgressUpdateAsync(int id, int received, int total, bool isRefreshing = true) => Task.CompletedTask;

    public Task SendDownloadTaskCreationProgressUpdate(int current, int total) => Task.CompletedTask;

    public Task SendDownloadTaskUpdateAsync(DownloadTask downloadTask, CancellationToken cancellationToken = default)
    {
        var downloadTaskDTO = _mapper.Map<DownloadTaskDTO>(downloadTask);
        DownloadTaskUpdate.Add(downloadTaskDTO, cancellationToken);
        _log.Verbose("{ClassName} => {@DownloadTaskDto}", nameof(MockSignalRService), downloadTaskDTO);

        return Task.CompletedTask;
    }

    public Task SendFileMergeProgressUpdateAsync(FileMergeProgress fileMergeProgress, CancellationToken cancellationToken = default)
    {
        FileMergeProgressList.Add(fileMergeProgress, cancellationToken);
        _log.Verbose("{ClassName} => {@FileMergeProgress}", nameof(MockSignalRService), fileMergeProgress);
        return Task.CompletedTask;
    }

    public Task SendNotificationAsync(Notification notification) => Task.CompletedTask;

    public Task SendServerInspectStatusProgressAsync(InspectServerProgress progress) => Task.CompletedTask;

    public Task SendServerSyncProgressUpdateAsync(SyncServerProgress syncServerProgress) => Task.CompletedTask;

    public Task SendDownloadProgressUpdateAsync(int plexServerId, List<DownloadTaskGeneric> downloadTasks, CancellationToken cancellationToken = default)
    {
        var update = _mapper.Map<List<ServerDownloadProgressDTO>>(downloadTasks);

        ServerDownloadProgressList.Add(update.First(), cancellationToken);
        _log.Verbose("{ClassName} => {@DownloadTaskDto}", nameof(MockSignalRService), update);

        return Task.CompletedTask;
    }

    public Task SendServerConnectionCheckStatusProgressAsync(ServerConnectionCheckStatusProgress progress) => Task.CompletedTask;

    public Task SendJobStatusUpdateAsync(JobStatusUpdate jobStatusUpdate) => Task.CompletedTask;
}
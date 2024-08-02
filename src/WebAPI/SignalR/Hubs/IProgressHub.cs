using Application.Contracts;
using SignalRSwaggerGen.Attributes;
using SignalRSwaggerGen.Enums;
using WebAPI.Contracts;

namespace PlexRipper.WebAPI;

[SignalRHub(autoDiscover: AutoDiscover.MethodsAndParams)]
public interface IProgressHub
{
    [return: SignalRReturn(typeof(Task<JobStatusUpdateDTO>), 200, "Success")]
    [SignalRMethod(operation: Operation.Get, autoDiscover: AutoDiscover.None)]
    Task JobStatusUpdate(JobStatusUpdateDTO jobStatusUpdate, CancellationToken cancellationToken = default);

    [return: SignalRReturn(typeof(Task<SyncServerProgress>), 200, "Success")]
    [SignalRMethod(operation: Operation.Get, autoDiscover: AutoDiscover.None)]
    Task SyncServerProgress(SyncServerProgress SyncServerProgress, CancellationToken cancellationToken = default);

    [return: SignalRReturn(typeof(Task<FileMergeProgress>), 200, "Success")]
    [SignalRMethod(operation: Operation.Get, autoDiscover: AutoDiscover.None)]
    Task FileMergeProgress(FileMergeProgress fileMergeProgress, CancellationToken cancellationToken = default);

    [return: SignalRReturn(typeof(Task<ServerConnectionCheckStatusProgressDTO>), 200, "Success")]
    [SignalRMethod(operation: Operation.Get, autoDiscover: AutoDiscover.None)]
    Task ServerConnectionCheckStatusProgress(
        ServerConnectionCheckStatusProgressDTO serverConnectionCheckStatusProgress,
        CancellationToken cancellationToken = default
    );

    [return: SignalRReturn(typeof(Task<InspectServerProgressDTO>), 200, "Success")]
    [SignalRMethod(operation: Operation.Get, autoDiscover: AutoDiscover.None)]
    Task InspectServerProgress(
        InspectServerProgressDTO InspectServerProgress,
        CancellationToken cancellationToken = default
    );

    [return: SignalRReturn(typeof(Task<ServerDownloadProgressDTO>), 200, "Success")]
    [SignalRMethod(operation: Operation.Get, autoDiscover: AutoDiscover.None)]
    Task ServerDownloadProgress(
        ServerDownloadProgressDTO serverDownloadProgress,
        CancellationToken cancellationToken = default
    );

    [return: SignalRReturn(typeof(Task<DownloadTaskDTO>), 200, "Success")]
    [SignalRMethod(operation: Operation.Get, autoDiscover: AutoDiscover.None)]
    Task DownloadTaskUpdate(DownloadTaskDTO downloadTask, CancellationToken cancellationToken = default);

    [return: SignalRReturn(typeof(Task<LibraryProgress>), 200, "Success")]
    [SignalRMethod(operation: Operation.Get, autoDiscover: AutoDiscover.None)]
    Task LibraryProgress(LibraryProgress libraryProgress, CancellationToken cancellationToken = default);
}

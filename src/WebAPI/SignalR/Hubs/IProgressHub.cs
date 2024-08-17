using Application.Contracts;
using WebAPI.Contracts;

namespace PlexRipper.WebAPI;

public interface IProgressHub
{
    Task JobStatusUpdate(JobStatusUpdateDTO jobStatusUpdate, CancellationToken cancellationToken = default);

    Task SyncServerMediaProgress(
        SyncServerMediaProgress syncServerMediaProgress,
        CancellationToken cancellationToken = default
    );

    Task FileMergeProgress(FileMergeProgress fileMergeProgress, CancellationToken cancellationToken = default);

    Task ServerConnectionCheckStatusProgress(
        ServerConnectionCheckStatusProgressDTO serverConnectionCheckStatusProgress,
        CancellationToken cancellationToken = default
    );

    Task InspectServerProgress(
        InspectServerProgressDTO inspectServerProgress,
        CancellationToken cancellationToken = default
    );

    Task ServerDownloadProgress(
        ServerDownloadProgressDTO serverDownloadProgress,
        CancellationToken cancellationToken = default
    );

    Task DownloadTaskUpdate(DownloadTaskDTO downloadTask, CancellationToken cancellationToken = default);

    Task LibraryProgress(LibraryProgress libraryProgress, CancellationToken cancellationToken = default);
}

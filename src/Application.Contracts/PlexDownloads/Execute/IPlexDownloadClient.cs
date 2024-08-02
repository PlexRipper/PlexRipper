using FluentResults;
using PlexRipper.Domain;

namespace Application.Contracts;

public interface IPlexDownloadClient
{
    DownloadStatus DownloadStatus { get; }

    IObservable<IList<DownloadWorkerLog>> ListenToDownloadWorkerLog { get; }

    /// <summary>
    /// Gets the Task that completes when all download workers have finished.
    /// </summary>
    Task DownloadProcessTask { get; }

    /// <summary>
    /// Setup this <see cref="PlexDownloadClient"/> to prepare for the download process.
    /// This needs to be called before any other action can be taken.
    /// Note: adding this in the constructor prevents us from returning a <see cref="Result"/>.
    /// </summary>
    /// <returns></returns>
    Task<Result> Setup(DownloadTaskKey downloadTaskKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts the download workers for the <see cref="DownloadTaskGeneric"/> given during setup.
    /// </summary>
    /// <returns>Is successful.</returns>
    Result Start(CancellationToken cancellationToken = default);

    Task<Result> StopAsync();
}

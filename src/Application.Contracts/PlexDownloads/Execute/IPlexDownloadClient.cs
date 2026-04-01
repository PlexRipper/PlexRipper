namespace Reaparr.Application.Contracts;

public interface IPlexDownloadClient : IAsyncDisposable
{
    /// <summary>
    /// Starts the download workers for the <see cref="DownloadTaskGeneric"/> given during setup.
    /// </summary>
    /// <returns>Is successful.</returns>
    Task<Result> Start(DownloadTaskKey downloadTaskKey, CancellationToken cancellationToken = default);

    Task<Result> StopAsync();
}

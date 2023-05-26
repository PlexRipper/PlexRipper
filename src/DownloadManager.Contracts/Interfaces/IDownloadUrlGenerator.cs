using FluentResults;
using PlexRipper.Domain;

namespace DownloadManager.Contracts;

public interface IDownloadUrlGenerator
{
    Task<Result<string>> GetDownloadUrl(DownloadTask downloadTask, CancellationToken cancellationToken = default);
    Task<Result<string>> GetDownloadUrl(int plexServerId, string fileLocationUrl, CancellationToken cancellationToken = default);
}
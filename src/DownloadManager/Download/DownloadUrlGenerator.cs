using Data.Contracts;
using DownloadManager.Contracts;
using Logging.Interface;

namespace PlexRipper.DownloadManager;

public class DownloadUrlGenerator : IDownloadUrlGenerator
{
    private readonly ILog _log;
    private readonly IMediator _mediator;

    public DownloadUrlGenerator(ILog log, IMediator mediator)
    {
        _log = log;
        _mediator = mediator;
    }

    public async Task<Result<string>> GetDownloadUrl(DownloadTask downloadTask, CancellationToken cancellationToken = default)
    {
        return await GetDownloadUrl(downloadTask.PlexServerId, downloadTask.FileLocationUrl, cancellationToken);
    }

    public async Task<Result<string>> GetDownloadUrl(int plexServerId, string fileLocationUrl, CancellationToken cancellationToken = default)
    {
        var plexServerConnectionResult =
            await _mediator.Send(new GetPlexServerConnectionByPlexServerIdQuery(plexServerId), cancellationToken);
        if (plexServerConnectionResult.IsFailed)
            return plexServerConnectionResult.ToResult().LogError();

        var plexServerConnection = plexServerConnectionResult.Value;
        var plexServer = plexServerConnection.PlexServer;

        var tokenResult = await _mediator.Send(new GetPlexServerTokenQuery(plexServerId), cancellationToken);
        if (tokenResult.IsFailed)
        {
            _log.Error("Could not find a valid token for server {ServerName}", plexServer.Name);
            return tokenResult.ToResult();
        }

        var downloadUrl = plexServerConnection.GetDownloadUrl(fileLocationUrl, tokenResult.Value);
        return Result.Ok(downloadUrl);
    }
}
using Data.Contracts;
using DownloadManager.Contracts;
using Logging.Interface;

namespace PlexRipper.DownloadManager;

public class DownloadUrlGenerator : IDownloadUrlGenerator
{
    private readonly ILog _log;
    private readonly IMediator _mediator;
    private readonly IPlexRipperDbContext _dbContext;

    public DownloadUrlGenerator(ILog log, IMediator mediator, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _mediator = mediator;
        _dbContext = dbContext;
    }

    public async Task<Result<string>> GetDownloadUrl(DownloadTask downloadTask, CancellationToken cancellationToken = default)
    {
        return await GetDownloadUrl(downloadTask.PlexServerId, downloadTask.FileLocationUrl, cancellationToken);
    }

    public async Task<Result<string>> GetDownloadUrl(int plexServerId, string fileLocationUrl, CancellationToken cancellationToken = default)
    {
        var plexServerConnectionResult = await _dbContext.GetValidPlexServerConnection(plexServerId, cancellationToken);
        if (plexServerConnectionResult.IsFailed)
            return plexServerConnectionResult.ToResult().LogError();

        var plexServerConnection = plexServerConnectionResult.Value;
        var plexServer = plexServerConnection.PlexServer;

        var tokenResult = await _dbContext.GetPlexServerTokenAsync(plexServerId, cancellationToken: cancellationToken);
        if (tokenResult.IsFailed)
        {
            _log.Error("Could not find a valid token for server {ServerName}", plexServer.Name);
            return tokenResult.ToResult();
        }

        var downloadUrl = plexServerConnection.GetDownloadUrl(fileLocationUrl, tokenResult.Value);
        return Result.Ok(downloadUrl);
    }
}
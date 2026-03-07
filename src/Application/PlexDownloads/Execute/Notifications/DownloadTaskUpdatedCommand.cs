using FastEndpoints;
using Reaparr.Data.Contracts;
using Reaparr.SignalR.Contracts;

namespace Reaparr.Application;

public record DownloadTaskUpdatedCommand(DownloadTaskKey Key) : ICommand<Result>;

public class DownloadTaskUpdatedHandler : ICommandHandler<DownloadTaskUpdatedCommand, Result>
{
    private readonly IReaparrDbContext _dbContext;
    private readonly IDownloadHubService _downloadHubService;

    public DownloadTaskUpdatedHandler(IReaparrDbContext dbContext, IDownloadHubService downloadHubService)
    {
        _dbContext = dbContext;
        _downloadHubService = downloadHubService;
    }

    public async Task<Result> ExecuteAsync(DownloadTaskUpdatedCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var plexServerId = command.Key.PlexServerId;

            // Ensure the up-to-date download status is written to the database as the DownloadQueue depends on that status to pick a new DownloadTask
            await _dbContext.DetermineDownloadStatus(command.Key, cancellationToken);

            var downloadTasks = await _dbContext.GetAllDownloadTasksByServerAsync(
                plexServerId,
                cancellationToken: cancellationToken
            );

            // Update the front-end with the download progress
            await _downloadHubService.SendDownloadProgressUpdateAsync(downloadTasks, cancellationToken);

            return Result.Ok();
        }
        finally
        {
            _dbContext.ClearChangeTracker();
        }
    }
}

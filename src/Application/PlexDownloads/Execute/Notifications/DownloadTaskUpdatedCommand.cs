using Application.Contracts;
using Data.Contracts;
using FastEndpoints;

namespace PlexRipper.Application;

public record DownloadTaskUpdatedCommand(DownloadTaskKey Key) : ICommand<Result>;

public class DownloadTaskUpdatedHandler : ICommandHandler<DownloadTaskUpdatedCommand, Result>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly ISignalRService _signalRService;

    public DownloadTaskUpdatedHandler(IPlexRipperDbContext dbContext, ISignalRService signalRService)
    {
        _dbContext = dbContext;
        _signalRService = signalRService;
    }

    public async Task<Result> ExecuteAsync(
        DownloadTaskUpdatedCommand command,
        CancellationToken cancellationToken
    )
    {
        var plexServerId = command.Key.PlexServerId;

        // Ensure the up-to-date download status is written to the database as the DownloadQueue depends on that status to pick a new DownloadTask
        await _dbContext.DetermineDownloadStatus(command.Key, cancellationToken);

        var downloadTasks = await _dbContext.GetAllDownloadTasksByServerAsync(
            plexServerId,
            cancellationToken: cancellationToken
        );

        // Update the front-end with the download progress
        await _signalRService.SendDownloadProgressUpdateAsync(downloadTasks, cancellationToken);

        var changedDownloadTask = await _dbContext.GetDownloadTaskAsync(command.Key, cancellationToken);
        if (changedDownloadTask is null)
        {
            return ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), command.Key.ToString()).LogError();
        }

        return Result.Ok();
    }
}

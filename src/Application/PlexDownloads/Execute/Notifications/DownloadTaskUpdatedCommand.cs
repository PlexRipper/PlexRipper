using FastEndpoints;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

public record DownloadTaskUpdatedCommand(DownloadTaskKey Key) : ICommand<Result>;

public class DownloadTaskUpdatedHandler : ICommandHandler<DownloadTaskUpdatedCommand, Result>
{
    private readonly IReaparrDbContext _dbContext;
    private readonly IDownloadPatchBroadcaster _downloadPatchBroadcaster;

    public DownloadTaskUpdatedHandler(IReaparrDbContext dbContext, IDownloadPatchBroadcaster downloadPatchBroadcaster)
    {
        _dbContext = dbContext;
        _downloadPatchBroadcaster = downloadPatchBroadcaster;
    }

    public async Task<Result> ExecuteAsync(DownloadTaskUpdatedCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var plexServerId = command.Key.PlexServerId;

            // Ensure the up-to-date download status is written to the database as the DownloadQueue depends on that status to pick a new DownloadTask
            var changedParentKeys = await _dbContext.DetermineDownloadStatus(command.Key, cancellationToken);
            var rootKey = await _dbContext.GetRootDownloadTaskKeyAsync(command.Key, cancellationToken);
            var currentStatus = await _dbContext.GetDownloadStatusAsync(command.Key, cancellationToken);

            if (rootKey is null)
                return Result.Ok();

            var changedNodeIds = changedParentKeys.Select(x => x.Id).Append(command.Key.Id).Distinct().ToList();

            var isStatusChanged =
                currentStatus.HasValue
                && _downloadPatchBroadcaster.TryMarkStatusChanged(command.Key.Id, currentStatus.Value);

            await _downloadPatchBroadcaster.MarkProgressDirtyAsync(
                plexServerId,
                rootKey,
                command.Key.Id,
                cancellationToken
            );

            if (changedParentKeys.Count > 0 || isStatusChanged)
            {
                await _downloadPatchBroadcaster.PublishImmediateStatusPatchAsync(
                    plexServerId,
                    rootKey,
                    changedNodeIds,
                    cancellationToken
                );
            }

            return Result.Ok();
        }
        finally
        {
            _dbContext.ClearChangeTracker();
        }
    }
}

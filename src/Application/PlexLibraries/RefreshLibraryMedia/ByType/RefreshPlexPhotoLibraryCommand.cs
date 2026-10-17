namespace Reaparr.Application;

public record RefreshPlexPhotoLibraryCommand(PlexLibrary PlexLibrary, bool ForceMediaRefresh = false)
    : ICommand<Result<PlexLibrary>>;

public class RefreshPlexPhotoLibraryCommandValidator : AbstractValidator<RefreshPlexPhotoLibraryCommand>
{
    public RefreshPlexPhotoLibraryCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.PlexLibrary).NotNull();
        RuleFor(x => x.PlexLibrary.Id).GreaterThan(0);
        RuleFor(x => x.PlexLibrary.Type).Equal(PlexMediaType.PhotoAlbum);
        RuleFor(x => x.PlexLibrary.PlexServerId).GreaterThan(0);
    }
}

public class RefreshPlexPhotoLibraryCommandHandler : ICommandHandler<RefreshPlexPhotoLibraryCommand, Result<PlexLibrary>>
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IReaparrDbContext _dbContext;
    private readonly ILibrarySyncProgressStore _librarySyncProgressStore;

    public RefreshPlexPhotoLibraryCommandHandler(
        ILogger log,
        ICommandExecutor commandExecutor,
        IReaparrDbContext dbContext,
        ILibrarySyncProgressStore librarySyncProgressStore
    )
    {
        _log = log.ForContext<RefreshPlexPhotoLibraryCommandHandler>();
        _commandExecutor = commandExecutor;
        _dbContext = dbContext;
        _librarySyncProgressStore = librarySyncProgressStore;
    }

    public async Task<Result<PlexLibrary>> ExecuteAsync(
        RefreshPlexPhotoLibraryCommand command,
        CancellationToken cancellationToken
    )
    {
        var libraryId = command.PlexLibrary.Id;
        await _librarySyncProgressStore.StartAsync(libraryId, PlexMediaType.PhotoAlbum);
        var retrievalResult = await Result.Try(() =>
            _commandExecutor.Send(
                new GetPhotoLibrarySnapshotFromPlexApiCommand(command.PlexLibrary),
                cancellationToken
            )
        );
        if (retrievalResult.IsCancelled)
            return retrievalResult.ToResult().LogWarning();
        if (retrievalResult.IsFailed)
        {
            await _librarySyncProgressStore.UpdateErrorAsync(libraryId, retrievalResult.ToResult(), cancellationToken);
            return retrievalResult.ToResult().LogError();
        }

        var snapshot = retrievalResult.Value;
        var syncResult = await Result.Try(() =>
            _commandExecutor.Send(
                new SyncPlexPhotosCommand(snapshot, command.ForceMediaRefresh),
                cancellationToken
            )
        );
        if (syncResult.IsCancelled)
            return syncResult.ToResult().LogWarning();
        if (syncResult.IsFailed)
        {
            await _librarySyncProgressStore.UpdateErrorAsync(libraryId, syncResult.ToResult(), cancellationToken);
            return syncResult.ToResult().LogError();
        }

        var optimizationResult = await _commandExecutor.Send(
            new ScheduleOptimizeDatabaseJobCommand { ChangedItemCount = syncResult.Value.ChangedItemCount },
            cancellationToken
        );
        optimizationResult.LogIfFailed();

        await _librarySyncProgressStore.UpdateItemAsync(
            libraryId,
            new LibraryProgressItem
            {
                MediaType = PlexMediaType.PhotoAlbum,
                Received = snapshot.PlexLibrary.PhotoAlbums.Count,
                Total = snapshot.PlexLibrary.PhotoAlbums.Count,
                TimeRemaining = TimeSpan.Zero,
            },
            cancellationToken
        );
        await _librarySyncProgressStore.UpdateItemAsync(
            libraryId,
            new LibraryProgressItem
            {
                MediaType = PlexMediaType.PhotoImage,
                Received = snapshot.PlexLibrary.PhotoMediaData.Count,
                Total = snapshot.PlexLibrary.PhotoMediaData.Count,
                TimeRemaining = TimeSpan.Zero,
            },
            cancellationToken
        );

        var rebuildResult = await _commandExecutor.Send(new QueueMediaOverviewRebuildCommand(), cancellationToken);
        rebuildResult.LogIfFailed();

        _log.Here().Information("Successfully refreshed library {PlexLibraryName} with id: {PlexLibraryId}", command.PlexLibrary.Title, libraryId);

        var plexLibraryDb = await _dbContext.PlexLibraries.GetAsync(libraryId, CancellationToken.None);
        return plexLibraryDb is null
            ? ResultExtensions.EntityNotFound(nameof(PlexLibrary), libraryId)
            : Result.Ok(plexLibraryDb);
    }
}

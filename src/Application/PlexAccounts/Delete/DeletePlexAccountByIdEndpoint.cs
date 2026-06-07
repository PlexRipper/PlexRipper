namespace Reaparr.Application;

public record DeletePlexAccountByIdRequest(int PlexAccountId);

public class DeletePlexAccountByIdRequestValidator : Validator<DeletePlexAccountByIdRequest>
{
    public DeletePlexAccountByIdRequestValidator()
    {
        RuleFor(x => x.PlexAccountId).GreaterThan(0);
    }
}

public class DeletePlexAccountByIdEndpoint : Endpoint<DeletePlexAccountByIdRequest, BaseResultDTO>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly INotificationHubService _notificationHubService;

    public DeletePlexAccountByIdEndpoint(
        ILogger log,
        IReaparrDbContext dbContext,
        INotificationHubService notificationHubService
    )
    {
        _log = log.ForContext<DeletePlexAccountByIdEndpoint>();
        _dbContext = dbContext;
        _notificationHubService = notificationHubService;
    }

    public override void Configure()
    {
        Delete(ApiRoutes.PlexAccountController + "/{PlexAccountId}");

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(DeletePlexAccountByIdRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);
        var deletedPlexAccountsCount = await _dbContext
            .PlexAccounts.Where(x => x.Id == req.PlexAccountId)
            .ExecuteDeleteAsync(ct);

        if (deletedPlexAccountsCount == 0)
        {
            await Send.FluentResult(
                Result.Fail($"Could not find {nameof(PlexAccount)} with id {req.PlexAccountId} to delete.").LogError(),
                ct
            );
            return;
        }

        await _dbContext.PlexAccountServers.Where(x => x.PlexAccountId == req.PlexAccountId).ExecuteDeleteAsync(ct);
        await _dbContext.PlexAccountLibraries.Where(x => x.PlexAccountId == req.PlexAccountId).ExecuteDeleteAsync(ct);

        // Clean up orphaned PlexServers and PlexLibraries
        var accessibleServerIds = await _dbContext.PlexAccountServers.Select(y => y.PlexServerId).ToListAsync(ct);
        var accessibleLibraryIds = await _dbContext.PlexAccountLibraries.Select(y => y.PlexLibraryId).ToListAsync(ct);

        var deletedServersCount = await _dbContext
            .PlexServers.IgnoreIsEnabledFilter()
            .Where(x => !accessibleServerIds.Contains(x.Id))
            .ExecuteDeleteAsync(ct);

        var deletedLibrariesCount = await _dbContext
            .PlexLibraries.Where(x => !accessibleLibraryIds.Contains(x.Id))
            .ExecuteDeleteAsync(ct);

        _log.Here()
            .Debug(
                "Deleted {PlexAccount} with Id: {CommandId} from the database, and cleaned up {DeletedServersCount} PlexServers and {DeletedLibrariesCount} PlexLibraries",
                nameof(PlexAccount),
                req.PlexAccountId,
                deletedServersCount,
                deletedLibrariesCount
            );

        await _notificationHubService.SendRefreshNotificationAsync(
            [
                RefreshDataType.PlexAccount,
                RefreshDataType.PlexServer,
                RefreshDataType.PlexServerConnection,
                RefreshDataType.PlexLibrary,
            ],
            ct
        );

        await Send.FluentResult(Result.Ok(), ct);
    }
}

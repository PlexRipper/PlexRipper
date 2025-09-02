using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.Logging;
using Serilog;

namespace Reaparr.Application;

public record DeletePlexAccountByIdRequest(int PlexAccountId);

public class DeletePlexAccountByIdRequestValidator : Validator<DeletePlexAccountByIdRequest>
{
    public DeletePlexAccountByIdRequestValidator()
    {
        RuleFor(x => x.PlexAccountId).GreaterThan(0);
    }
}

public class DeletePlexAccountByIdEndpoint : BaseEndpoint<DeletePlexAccountByIdRequest>
{
    private readonly Serilog.ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ISignalRService _signalRService;

    public override string EndpointPath => ApiRoutes.PlexAccountController + "/{PlexAccountId}";

    public DeletePlexAccountByIdEndpoint(ILogger log, IReaparrDbContext dbContext, ISignalRService signalRService)
    {
        _log = log.ForContext<DeletePlexAccountByIdEndpoint>();
        _dbContext = dbContext;
        _signalRService = signalRService;
    }

    public override void Configure()
    {
        Delete(EndpointPath);

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
            await SendFluentResult(
                Result.Fail($"Could not find {nameof(PlexAccount)} with id {req.PlexAccountId} to delete.").LogError(),
                ct
            );
            return;
        }

        // Clean up orphaned PlexServers and PlexLibraries
        var accessibleServerIds = await _dbContext.PlexAccountServers.Select(y => y.PlexServerId).ToListAsync(ct);
        var accessibleLibraryIds = await _dbContext.PlexAccountLibraries.Select(y => y.PlexLibraryId).ToListAsync(ct);

        var deletedServersCount = await _dbContext
            .PlexServers.Where(x => !accessibleServerIds.Contains(x.Id))
            .ExecuteDeleteAsync(ct);

        var deletedLibrariesCount = await _dbContext
            .PlexLibraries.Where(x => !accessibleLibraryIds.Contains(x.Id))
            .ExecuteDeleteAsync(ct);

        await _dbContext.PlexAccountServers.Where(x => x.PlexAccountId == req.PlexAccountId).ExecuteDeleteAsync(ct);
        await _dbContext.PlexAccountLibraries.Where(x => x.PlexAccountId == req.PlexAccountId).ExecuteDeleteAsync(ct);

        _log.Here()
            .Debug(
                "Deleted {PlexAccount} with Id: {CommandId} from the database, and cleaned up {DeletedServersCount} PlexServers and {DeletedLibrariesCount} PlexLibraries",
                nameof(PlexAccount),
                req.PlexAccountId,
                deletedServersCount,
                deletedLibrariesCount
            );

        await _signalRService.SendRefreshNotificationAsync(
            [
                RefreshDataType.PlexAccount,
                RefreshDataType.PlexServer,
                RefreshDataType.PlexServerConnection,
                RefreshDataType.PlexLibrary,
            ],
            ct
        );

        await SendFluentResult(Result.Ok(), ct);
    }
}

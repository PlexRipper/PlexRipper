using FastEndpoints;
using FluentValidation;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.Application;

/// <summary>
/// Retrieve the accessible <see cref="PlexLibrary">PlexLibraries</see> for this <see cref="PlexServer"/> which the <see cref="PlexAccount"/> has access to and update the database. The <see cref="PlexServer"/> in question will need to be online.
/// </summary>
/// <param name="PlexAccountId">The id of the <see cref="PlexAccount"/> to retrieve the accessible <see cref="PlexLibrary">Plex Libraries</see> for.</param>
/// <param name="PlexServerId">The id of the <see cref="PlexServer"/> to retrieve <see cref="PlexLibrary">Plex Libraries</see> for.</param>
///  <returns>If successful.</returns>
public record RefreshLibraryAccessCommand(int PlexAccountId, int PlexServerId = 0)
    : ICommand<Result<PlexLibraryAccessRefreshResponse>>;

public class RefreshLibraryAccessValidator : AbstractValidator<RefreshLibraryAccessCommand>
{
    public RefreshLibraryAccessValidator()
    {
        RuleFor(x => x.PlexAccountId).GreaterThan(0);
        RuleFor(x => x.PlexServerId).GreaterThanOrEqualTo(0);
    }
}

public class RefreshLibraryAccessHandler
    : ICommandHandler<RefreshLibraryAccessCommand, Result<PlexLibraryAccessRefreshResponse>>
{
    private readonly Serilog.ILogger _log;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandDispatcher;

    public RefreshLibraryAccessHandler(
        ILogger log,
        ICommandExecutor commandExecutor,
        IReaparrDbContext dbContext,
        ICommandExecutor commandDispatcher
    )
    {
        _log = log.ForContext<RefreshLibraryAccessHandler>();
        _commandExecutor = commandExecutor;
        _dbContext = dbContext;
        _commandDispatcher = commandDispatcher;
    }

    public async Task<Result<PlexLibraryAccessRefreshResponse>> ExecuteAsync(
        RefreshLibraryAccessCommand command,
        CancellationToken cancellationToken
    )
    {
        var plexAccountId = command.PlexAccountId;
        var plexServerId = command.PlexServerId;

        var plexServers = new List<PlexServer>();

        // Determine the Plex servers to refresh the Plex libraries for
        if (plexServerId == 0)
        {
            var result = await _dbContext.GetAccessiblePlexServers(plexAccountId, cancellationToken);
            if (result.IsFailed)
                return result.ToResult();

            plexServers = result.Value;
        }
        else
        {
            var plexServer = await _dbContext.PlexServers.GetAsync(plexServerId, cancellationToken);
            if (plexServer is not null)
            {
                plexServers.Add(plexServer);
            }
        }

        if (!plexServers.Any())
        {
            var plexAccountName = await _dbContext.GetPlexAccountDisplayName(plexAccountId, cancellationToken);
            _log.Here().Warning("No accessible Plex servers found for PlexAccount {PlexAccountName}", plexAccountName);
            return Result.Ok(new PlexLibraryAccessRefreshResponse { Reports = [], OfflineServers = [] });
        }

        // Refresh Plex libraries
        var failedServers = new List<int>();

        var libraryResults = await Task.WhenAll(
            plexServers.Select(async server =>
            {
                var refreshResult = await RefreshLibrary(server.Id, plexAccountId, cancellationToken);
                if (refreshResult.ToResult().Has504GatewayTimeoutError())
                {
                    failedServers.Add(server.Id);
                }

                return refreshResult;
            })
        );

        if (libraryResults.All(x => x.IsFailed))
            return Result.Merge(libraryResults).ToResult();

        var plexLibraries = libraryResults.Where(x => x.IsSuccess).SelectMany(x => x.Value).ToList();

        var updateResult = await _commandExecutor.Send(
            new AddOrUpdatePlexLibrariesCommand { PlexAccountId = plexAccountId, PlexLibraries = plexLibraries },
            cancellationToken
        );

        if (updateResult.IsFailed)
        {
            return updateResult.ToResult();
        }

        return Result.Ok(
            new PlexLibraryAccessRefreshResponse { Reports = updateResult.Value, OfflineServers = failedServers }
        );
    }

    private async Task<Result<List<PlexLibrary>>> RefreshLibrary(
        int plexServerId,
        int plexAccountId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var plexServerName = await _dbContext.GetPlexServerNameById(plexServerId, cancellationToken);
            var plexAccountName = await _dbContext.GetPlexAccountDisplayName(plexAccountId, cancellationToken);
            _log.Here()
                .Debug(
                    "Retrieving accessible PlexLibraries for plexServer with name: {PlexServerName} by using Plex account: {PlexAccountName}",
                    plexServerName,
                    plexAccountName
                );

            var libraries = await _commandDispatcher.Send(
                new GetLibrarySectionsCommand(plexServerId, plexAccountId),
                cancellationToken
            );

            if (libraries.IsFailed)
            {
                _log.Here()
                    .Error(
                        "Failed to retrieve libraries for PlexServer {PlexServerName} and PlexAccount {PlexAccountName}",
                        plexServerName,
                        plexAccountName
                    );

                return libraries.ToResult().LogError();
            }

            if (!libraries.Value.Any())
            {
                return _log.Here()
                    .WarningResult(
                        "PlexServer with name {PlexServerName} returned no Plex libraries for Plex account {plexAccountName}",
                        plexServerName,
                        plexAccountName
                    );
            }

            return libraries;
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}

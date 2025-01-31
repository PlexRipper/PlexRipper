using Application.Contracts;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using PlexApi.Contracts;

namespace PlexRipper.Application;

/// <summary>
/// Retrieve the accessible <see cref="PlexLibrary">PlexLibraries</see> for this <see cref="PlexServer"/> which the <see cref="PlexAccount"/> has access to and update the database. The <see cref="PlexServer"/> in question will need to be online.
/// </summary>
/// <param name="PlexAccountId">The id of the <see cref="PlexAccount"/> to retrieve the accessible <see cref="PlexLibrary">Plex Libraries</see> for.</param>
/// <param name="PlexServerId">The id of the <see cref="PlexServer"/> to retrieve <see cref="PlexLibrary">Plex Libraries</see> for.</param>
///  <returns>If successful.</returns>
public record RefreshLibraryAccessCommand(int PlexAccountId, int PlexServerId = 0)
    : IRequest<Result<PlexLibraryAccessRefreshResponse>>;

public class RefreshLibraryAccessValidator : AbstractValidator<RefreshLibraryAccessCommand>
{
    public RefreshLibraryAccessValidator()
    {
        RuleFor(x => x.PlexAccountId).GreaterThan(0);
        RuleFor(x => x.PlexServerId).GreaterThanOrEqualTo(0);
    }
}

public class RefreshLibraryAccessHandler
    : IRequestHandler<RefreshLibraryAccessCommand, Result<PlexLibraryAccessRefreshResponse>>
{
    private readonly ILog _log;
    private readonly IMediator _mediator;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IPlexApiService _plexServiceApi;

    public RefreshLibraryAccessHandler(
        ILog log,
        IMediator mediator,
        IPlexRipperDbContext dbContext,
        IPlexApiService plexServiceApi
    )
    {
        _log = log;
        _mediator = mediator;
        _dbContext = dbContext;
        _plexServiceApi = plexServiceApi;
    }

    public async Task<Result<PlexLibraryAccessRefreshResponse>> Handle(
        RefreshLibraryAccessCommand command,
        CancellationToken cancellationToken
    )
    {
        var plexAccountId = command.PlexAccountId;
        var plexServerId = command.PlexServerId;

        var plexLibraries = new List<PlexLibrary>();
        var plexServers = new List<PlexServer>();

        // Determine the Plex servers to refresh the Plex libraries for
        if (plexServerId == 0)
        {
            var result = await _dbContext.GetAccessiblePlexServers(plexAccountId, CancellationToken.None);
            if (result.IsFailed)
                return result.ToResult();

            plexServers = result.Value;
        }
        else
        {
            var plexServer = await _dbContext.PlexServers.GetAsync(plexServerId, CancellationToken.None);
            if (plexServer is not null)
            {
                plexServers.Add(plexServer);
            }
        }

        if (!plexServers.Any())
        {
            var plexAccountName = await _dbContext.GetPlexAccountDisplayName(plexAccountId, cancellationToken);
            _log.Warning("No accessible Plex servers found for PlexAccount {PlexAccountName}", plexAccountName);
            return Result.Ok(new PlexLibraryAccessRefreshResponse() { Reports = [], OfflineServers = [] });
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

        plexLibraries = libraryResults.Where(x => x.IsSuccess).SelectMany(x => x.Value).ToList();

        var updateResult = await _mediator.Send(
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

            var libraries = await _plexServiceApi.GetLibrarySectionsAsync(
                plexServerId,
                plexAccountId,
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
                    .Warning(
                        "PlexServer with name {PlexServerName} returned no Plex libraries for Plex account {plexAccountName}",
                        plexServerName,
                        plexAccountName
                    )
                    .ToResult();
            }

            return libraries;
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}

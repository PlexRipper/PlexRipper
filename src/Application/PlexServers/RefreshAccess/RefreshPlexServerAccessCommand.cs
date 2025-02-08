using Application.Contracts;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexApi.Contracts;

namespace PlexRipper.Application;

/// <summary>
/// Retrieve the latest accessible <see cref="PlexServer">PlexServers</see> for this <see cref="PlexAccount"/> from the PlexAPI and stores it in the Database.
/// </summary>
/// <param name="PlexAccountId">The id of the <see cref="PlexAccount"/> to check.</param>
public record RefreshPlexServerAccessCommand(int PlexAccountId) : IRequest<Result<RefreshPlexAccountAccessRapportDTO>>;

public class RefreshPlexServerAccessCommandValidator : AbstractValidator<RefreshPlexServerAccessCommand>
{
    public RefreshPlexServerAccessCommandValidator()
    {
        RuleFor(x => x.PlexAccountId).GreaterThan(0);
    }
}

public class RefreshPlexServerAccessCommandHandler
    : IRequestHandler<RefreshPlexServerAccessCommand, Result<RefreshPlexAccountAccessRapportDTO>>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly IPlexApiService _plexServiceApi;

    public RefreshPlexServerAccessCommandHandler(
        ILog log,
        IPlexRipperDbContext dbContext,
        IMediator mediator,
        IPlexApiService plexServiceApi
    )
    {
        _log = log;
        _dbContext = dbContext;
        _mediator = mediator;
        _plexServiceApi = plexServiceApi;
    }

    public async Task<Result<RefreshPlexAccountAccessRapportDTO>> Handle(
        RefreshPlexServerAccessCommand command,
        CancellationToken cancellationToken
    )
    {
        var plexAccountId = command.PlexAccountId;

        var plexAccountName = await _dbContext.GetPlexAccountDisplayName(plexAccountId, cancellationToken);

        var rapport = new RefreshPlexAccountAccessRapportDTO(plexAccountId, plexAccountName);

        _log.Debug("Refreshing Plex servers for PlexAccount: {PlexAccountName}", plexAccountName);

        var result = await _plexServiceApi.GetAccessiblePlexServersAsync(plexAccountId);

        // If the Plex API returns a 401 Unauthorized error, remove the PlexAccount and PlexServerAccess
        if (result.HasPlex401UnauthorizedError())
        {
            _log.Warning(
                "Plex API returned 401 Unauthorized for PlexAccount: {PlexAccountDisplayName}",
                plexAccountName
            );
            _log.Warning(
                "Removing PlexServerAccess and LibraryAccess for PlexAccount: {PlexAccountDisplayName}",
                plexAccountName
            );

            await RemovePlexAccess(rapport, plexAccountId);

            _log.Error("{PlexAccountName} token has been invalidated and has lost Plex Server access", plexAccountName);
            return Result.Ok(rapport);
        }

        if (result.IsFailed)
            return result.LogError();

        if (!result.Value.Any())
        {
            _log.Warning("No Plex servers found for PlexAccount: {plexAccountName}", plexAccountName);

            await RemovePlexAccess(rapport, plexAccountId);

            return Result.Ok(rapport);
        }

        var serverList = result.Value.Select(x => x.PlexServer).ToList();
        var serverAccessTokens = result.Value.Select(x => x.AccessToken).ToList();

        // Add PlexServers and their PlexServerConnections
        var updateResult = await _mediator.Send(new AddOrUpdatePlexServersCommand(serverList), CancellationToken.None);
        if (updateResult.IsFailed)
            return updateResult.LogError();

        // Add or update the PlexAccount and PlexServer relationships
        var plexAccountTokensResult = await _mediator.Send(
            new AddOrUpdatePlexAccountServersCommand(plexAccountId, serverAccessTokens),
            cancellationToken
        );

        if (plexAccountTokensResult.IsFailed)
            return plexAccountTokensResult.LogError();

        // Update library access
        var libraryAccessResult = await _mediator.Send(
            new RefreshLibraryAccessCommand(plexAccountId),
            CancellationToken.None
        );

        if (libraryAccessResult.IsFailed)
            return libraryAccessResult.LogError();

        UpdateRapport(rapport, plexAccountTokensResult.Value, libraryAccessResult.Value);

        _log.Information(
            "Successfully refreshed accessible Plex servers for account {PlexAccountDisplayName}",
            plexAccountName
        );

        return Result.Ok(rapport);
    }

    private async Task RemovePlexAccess(RefreshPlexAccountAccessRapportDTO rapport, int plexAccountId)
    {
        var plexServers = await _dbContext
            .PlexAccountServers.Include(x => x.PlexServer)
            .Where(x => x.PlexAccountId == plexAccountId)
            .Select(x => new { PlexServerId = x.PlexServerId, Name = x.PlexServer.Name! })
            .ToListAsync();

        if (!plexServers.Any())
            return;

        var plexLibraries = await _dbContext
            .PlexAccountLibraries.Include(x => x.PlexLibrary)
            .Where(x => x.PlexAccountId == plexAccountId)
            .ToListAsync();

        foreach (var plexServer in plexServers)
        {
            rapport.Access.Add(
                new PlexServerAccessRapportDTO
                {
                    PlexServerId = plexServer.PlexServerId,
                    PlexServerName = plexServer.Name,
                    State = PlexAccessState.Revoked,
                    IsServerOffline = false,
                    LibraryAccess = plexLibraries
                        .FindAll(library => library.PlexServerId == plexServer.PlexServerId)
                        .Select(x => new PlexLibraryAccessRapportDTO
                        {
                            PlexServerId = x.PlexServerId,
                            PlexLibraryId = x.PlexLibraryId,
                            PlexLibraryName = x.PlexLibrary?.Name ?? "Unknown",
                            State = PlexAccessState.Revoked,
                        })
                        .ToList(),
                }
            );
        }

        await _dbContext
            .PlexAccountServers.Where(x => x.PlexAccountId == plexAccountId)
            .ExecuteDeleteAsync(CancellationToken.None);

        await _dbContext
            .PlexAccountLibraries.Where(x => x.PlexAccountId == plexAccountId)
            .ExecuteDeleteAsync(CancellationToken.None);
    }

    private void UpdateRapport(
        RefreshPlexAccountAccessRapportDTO rapport,
        PlexServerAccessRapport serverAccessRapport,
        PlexLibraryAccessRefreshResponse libraryAccessRapport
    )
    {
        rapport.Access.AddRange(
            serverAccessRapport
                .Data.Select(x => new PlexServerAccessRapportDTO
                {
                    IsServerOffline = libraryAccessRapport.OfflineServers.Contains(x.PlexServerId),
                    PlexServerId = x.PlexServerId,
                    PlexServerName = x.PlexServerName,
                    State = libraryAccessRapport.OfflineServers.Contains(x.PlexServerId)
                        ? PlexAccessState.Unknown
                        : x.State,
                    LibraryAccess =
                        libraryAccessRapport
                            .Reports.Find(y => y.PlexServerId == x.PlexServerId)
                            ?.Data.Select(y => new PlexLibraryAccessRapportDTO
                            {
                                PlexLibraryName = y.PlexLibraryName,
                                PlexServerId = y.PlexServerId,
                                State = y.State,
                                PlexLibraryId = y.PlexLibraryId,
                            })
                            .ToList() ?? [],
                })
                .ToList()
        );
    }
}

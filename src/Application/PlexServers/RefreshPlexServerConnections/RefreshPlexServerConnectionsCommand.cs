using Application.Contracts;
using Data.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexApi.Contracts;

namespace PlexRipper.Application;

/// <summary>
/// Refreshes the <see cref="PlexServer"/> data with its <see cref="PlexServerConnection"/> from the Plex API
/// </summary>
/// <param name="PlexServerId">The id of the <see cref="PlexServer"/> to refresh the connections of.</param>
/// <returns>The refreshed <see cref="PlexServer"/> with the latest <see cref="PlexServerConnection"/>.</returns>
public record RefreshPlexServerConnectionsCommand(int PlexServerId) : IRequest<Result<PlexServer>>;

public class RefreshPlexServerConnectionsCommandValidator : AbstractValidator<RefreshPlexServerConnectionsCommand>
{
    public RefreshPlexServerConnectionsCommandValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class RefreshPlexServerConnectionsCommandHandler
    : IRequestHandler<RefreshPlexServerConnectionsCommand, Result<PlexServer>>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly IPlexApiService _plexServiceApi;
    private readonly ISignalRService _signalRService;

    public RefreshPlexServerConnectionsCommandHandler(
        IPlexRipperDbContext dbContext,
        IMediator mediator,
        IPlexApiService plexServiceApi,
        ISignalRService signalRService
    )
    {
        _dbContext = dbContext;
        _mediator = mediator;
        _plexServiceApi = plexServiceApi;
        _signalRService = signalRService;
    }

    public async Task<Result<PlexServer>> Handle(
        RefreshPlexServerConnectionsCommand request,
        CancellationToken cancellationToken
    )
    {
        // Pick an account that has access to the PlexServer to connect with
        var plexAccountResult = await _dbContext.ChoosePlexAccountToConnect(request.PlexServerId, cancellationToken);
        if (plexAccountResult.IsFailed)
            return plexAccountResult.ToResult();

        var plexAccount = plexAccountResult.Value;
        var plexServer = await _dbContext.PlexServers.GetAsync(request.PlexServerId, cancellationToken);
        if (plexServer is null)
            return ResultExtensions.EntityNotFound(nameof(PlexServer), request.PlexServerId).LogError();

        // Retrieve the PlexApi server data
        var tupleResult = await _plexServiceApi.GetAccessiblePlexServersAsync(plexAccount.Id);
        var serverList = tupleResult.servers.Value.FindAll(x => x.MachineIdentifier == plexServer.MachineIdentifier);

        // Check if we got the plex server we are looking for
        if (!serverList.Any())
        {
            return Result
                .Fail($"Could not retrieve the Plex server data with machine id: {plexServer.MachineIdentifier}")
                .LogError();
        }

        var serverAccessTokens = tupleResult.tokens.Value.FindAll(x =>
            x.MachineIdentifier == plexServer.MachineIdentifier
        );

        // We only want to update one plexServer and discard the rest
        var updateResult = await _mediator.Send(new AddOrUpdatePlexServersCommand(serverList), cancellationToken);
        if (updateResult.IsFailed)
            return updateResult;

        // We only want to update tokens for the plexServer and discard the rest
        var plexAccountTokensResult = await _mediator.Send(
            new AddOrUpdatePlexAccountServersCommand(plexAccount.Id, serverAccessTokens),
            cancellationToken
        );

        if (plexAccountTokensResult.IsFailed)
            return plexAccountTokensResult;

        // Send notifications to the client to refresh the PlexServerConnection data
        await _signalRService.SendRefreshNotificationAsync(
            [DataType.PlexAccount, DataType.PlexServer, DataType.PlexServerConnection],
            cancellationToken
        );

        var plexServerWithConnections = _dbContext
            .PlexServers.AsNoTracking()
            .Include(x => x.PlexServerConnections)
            .FirstOrDefault(x => x.Id == request.PlexServerId);

        return plexServerWithConnections is null
            ? ResultExtensions.EntityNotFound(nameof(PlexServer), request.PlexServerId)
            : Result.Ok(plexServerWithConnections);
    }
}

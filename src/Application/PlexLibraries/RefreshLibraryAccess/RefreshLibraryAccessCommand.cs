using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using PlexApi.Contracts;

namespace PlexRipper.Application;

/// <summary>
/// Retrieve the accessible <see cref="PlexLibrary">PlexLibraries</see> for this <see cref="PlexServer"/> which the <see cref="PlexAccount"/> has access to and update the database.
/// </summary>
/// <param name="PlexAccountId">The id of the <see cref="PlexAccount"/> to retrieve the accessible <see cref="PlexLibrary">Plex Libraries</see> for.</param>
/// <param name="PlexServerId">The id of the <see cref="PlexServer"/> to retrieve <see cref="PlexLibrary">Plex Libraries</see> for.</param>
///  <returns>If successful.</returns>
public record RefreshLibraryAccessCommand(int PlexAccountId, int PlexServerId) : IRequest<Result>;

public class RefreshLibraryAccessValidator : AbstractValidator<RefreshLibraryAccessCommand>
{
    public RefreshLibraryAccessValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
        RuleFor(x => x.PlexAccountId).GreaterThan(0);
    }
}

public class RefreshLibraryAccessHandler : IRequestHandler<RefreshLibraryAccessCommand, Result>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IPlexApiService _plexServiceApi;

    public RefreshLibraryAccessHandler(ILog log, IPlexRipperDbContext dbContext, IPlexApiService plexServiceApi)
    {
        _log = log;
        _dbContext = dbContext;
        _plexServiceApi = plexServiceApi;
    }

    public async Task<Result> Handle(RefreshLibraryAccessCommand command, CancellationToken cancellationToken)
    {
        var plexAccountId = command.PlexAccountId;
        var plexServerId = command.PlexServerId;

        _log.Debug("Retrieving accessible PlexLibraries for plexServer with id: {PlexServerId} by using Plex account with id {PlexAccountId}", plexServerId,
            plexAccountId);

        var libraries = await _plexServiceApi.GetLibrarySectionsAsync(plexServerId, plexAccountId);
        if (libraries.IsFailed)
            return libraries.ToResult();

        if (!libraries.Value.Any())
        {
            var msg = $"{nameof(PlexServer)} with Id {plexServerId} returned no Plex libraries for Plex account with id {plexAccountId}";
            return Result.Fail(msg).LogWarning();
        }

        return await _mediator.Send(new AddOrUpdatePlexLibrariesCommand(plexAccountId, libraries.Value));
    }
}
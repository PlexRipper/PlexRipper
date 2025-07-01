using Data.Contracts;
using FastEndpoints;
using Logging.Interface;
using PlexApi.Contracts;

namespace PlexRipper.PlexApi;

public class GetLibrarySectionsCommandHandler : ICommandHandler<GetLibrarySectionsCommand, Result<List<PlexLibrary>>>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IPlexApiClientFactory _plexApiClientFactory;

    public GetLibrarySectionsCommandHandler(
        ILog log,
        IPlexRipperDbContext dbContext,
        IPlexApiClientFactory plexApiClientFactory
    )
    {
        _log = log;
        _dbContext = dbContext;
        _plexApiClientFactory = plexApiClientFactory;
    }

    public async Task<Result<List<PlexLibrary>>> ExecuteAsync(GetLibrarySectionsCommand command, CancellationToken ct)
    {
        var plexServerId = command.PlexServerId;
        var plexAccountId = command.PlexAccountId;

        var tokenResult = await _dbContext.GetPlexServerTokenAsync(plexServerId, plexAccountId, ct);
        if (tokenResult.IsFailed)
            return tokenResult.ToResult();

        var plexServerConnectionResult = await _dbContext.ChoosePlexServerConnection(plexServerId, ct);
        if (plexServerConnectionResult.IsFailed)
            return plexServerConnectionResult.ToResult();

        var connection = plexServerConnectionResult.Value;

        var plexServer = connection.PlexServer;
        if (plexServer is null)
            return ResultExtensions.EntityNotFound(nameof(PlexServer), plexServerId);

        var client = _plexApiClientFactory.CreateClient(
            tokenResult.Value,
            new PlexApiClientOptions { ConnectionUrl = connection.Url }
        );

        var response = await client.Library.GetAllLibrariesAsync().ToResponse();
        if (response.IsFailed)
            return response.ToResult();

        if (response.Value.Object?.MediaContainer?.Directory is null)
        {
            _log.Error(
                "Plex server: {PlexServerName} returned an empty response when libraries were requested",
                connection.PlexServer?.Name
            );
            return response.ToResult();
        }

        var directories = response.Value.Object.MediaContainer.Directory;

        var mappedLibraries = directories
            .Select(x => new PlexLibrary
            {
                Id = 0,
                Type = x.Type.ToPlexMediaType(),
                Title = x.Title,
                Key = x.Key,
                CreatedAt = DateTimeExtensions.FromUnixTime(x.CreatedAt),
                UpdatedAt = DateTimeExtensions.FromUnixTime(x.UpdatedAt),
                ScannedAt = DateTimeExtensions.FromUnixTime(x.ScannedAt),
                SyncedAt = null,
                Uuid = x.Uuid,
                PlexServer = null,
                PlexServerId = plexServerId,
                DefaultDestination = null,
                DefaultDestinationId = null,
                Language = x.Language,
            })
            .ToList();

        return Result.Ok(mappedLibraries);
    }
}

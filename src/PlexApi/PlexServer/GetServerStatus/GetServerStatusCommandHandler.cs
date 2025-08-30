using FastEndpoints;
using Reaparr.Data.Contracts;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.PlexApi;

public class GetServerStatusCommandHandler : ICommandHandler<GetServerStatusCommand, Result<PlexServerStatus>>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IPlexApiClientFactory _plexApiClientFactory;

    public GetServerStatusCommandHandler(IPlexRipperDbContext dbContext, IPlexApiClientFactory plexApiClientFactory)
    {
        _dbContext = dbContext;
        _plexApiClientFactory = plexApiClientFactory;
    }

    public async Task<Result<PlexServerStatus>> ExecuteAsync(GetServerStatusCommand command, CancellationToken ct)
    {
        var connection = await _dbContext.PlexServerConnections.GetAsync(
            command.PlexServerConnectionId,
            CancellationToken.None
        );

        if (connection is null)
            return ResultExtensions.EntityNotFound(nameof(PlexServerConnection), command.PlexServerConnectionId);

        var client = _plexApiClientFactory.CreateClient(
            string.Empty, // No need for auth token here
            new PlexApiClientOptions
            {
                ConnectionUrl = connection.Url,
                Action = command.ProgressAction,
                Timeout = 10,
                RetryCount = 0,
            }
        );

        var responseResult = await client.Server.GetServerIdentityAsync().ToResponse();

        var statusCode = responseResult.IsSuccess
            ? responseResult.Value.StatusCode
            : responseResult.GetStatusCodeReason()?.GetStatusCode();
        var statusMessage = statusCode switch
        {
            200 => "The Plex server is online!",
            401 => "The Plex token has expired and needs to be refreshed.",
            _ => "The Plex server could not be reached, most likely it's offline.",
        };

        return Result.Ok(
            new PlexServerStatus
            {
                StatusCode = statusCode ?? -1,
                StatusMessage = statusMessage,
                LastChecked = DateTime.UtcNow,
                IsSuccessful = responseResult.IsSuccess,
                PlexServerId = connection.PlexServerId,
                PlexServerConnectionId = connection.Id,
            }
        );
    }
}

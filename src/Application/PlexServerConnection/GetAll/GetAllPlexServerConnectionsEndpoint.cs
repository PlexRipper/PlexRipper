using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

public class GetAllPlexServerConnectionsEndpoint : BaseEndpointWithoutRequest<List<PlexServerConnectionDTO>>
{
    private readonly IReaparrDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexServerConnectionController + "/";

    public GetAllPlexServerConnectionsEndpoint(IReaparrDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<PlexServerConnectionDTO>>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var plexServers = await _dbContext
            .PlexServers.Include(x => x.PlexServerConnections)
            .ThenInclude(x => x.LatestConnectionStatus)
            .Where(x => x.IsEnabled)
            .ToListAsync(ct);

        // Decide for the frontend which connection to use
        var chosenHash = new HashSet<int>();

        foreach (var plexServer in plexServers)
        {
            var chosenConnectionResult = await _dbContext.ChoosePlexServerConnection(
                plexServer.Id,
                cancellationToken: ct
            );

            if (chosenConnectionResult.IsFailed)
            {
                chosenConnectionResult.LogWarning();
                continue;
            }

            chosenHash.Add(chosenConnectionResult.Value.Id);
        }

        var plexServerConnections = plexServers.SelectMany(x => x.PlexServerConnections).ToList().ToDTO();
        foreach (var connection in plexServerConnections)
            connection.ChosenConnection = chosenHash.Contains(connection.Id);

        await SendFluentResult(Result.Ok(plexServerConnections), x => x, ct);
    }
}

namespace Reaparr.Application;

/// <summary>
/// Clears all completed <see cref="DownloadTaskGeneric"/> for a server from the database.
/// </summary>
/// <returns>Is successful.</returns>
public sealed class ClearCompletedDownloadTasksByServerIdEndpointRequest
{
    [RouteParam, BindFrom("PlexServerId")]
    public int PlexServerId { get; init; }
}

public class ClearCompletedDownloadTasksByServerIdEndpoint
    : Endpoint<ClearCompletedDownloadTasksByServerIdEndpointRequest, ResultDTO<CountResponseDTO>>
{
    private readonly ICommandExecutor _commandExecutor;

    public ClearCompletedDownloadTasksByServerIdEndpoint(ICommandExecutor commandExecutor)
    {
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Delete(ApiRoutes.DownloadController + "/clear/{PlexServerId}");

        Description(x => x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<CountResponseDTO>)));
    }

    public override async Task HandleAsync(
        ClearCompletedDownloadTasksByServerIdEndpointRequest req,
        CancellationToken ct
    )
    {
        var result = await _commandExecutor.Send(
            new ClearCompletedDownloadTasksByServerIdCommand(req.PlexServerId),
            ct
        );
        if (result.IsFailed)
        {
            await Send.FluentResult(result.ToResult(), ct);
            return;
        }

        await Send.FluentResult(Result.Ok(new CountResponseDTO(result.Value)), ct);
    }
}

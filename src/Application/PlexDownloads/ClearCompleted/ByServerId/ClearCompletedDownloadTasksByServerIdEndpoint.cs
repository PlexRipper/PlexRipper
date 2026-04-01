using Reaparr.Application.Contracts;

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
    : BaseEndpoint<ClearCompletedDownloadTasksByServerIdEndpointRequest, ResultDTO<CountResponseDTO>>
{
    private readonly ICommandExecutor _commandExecutor;

    public override string EndpointPath => ApiRoutes.DownloadController + "/clear/{PlexServerId}";

    public ClearCompletedDownloadTasksByServerIdEndpoint(ICommandExecutor commandExecutor)
    {
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Delete(EndpointPath);

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
            await SendFluentResult(result.ToResult(), ct);
            return;
        }

        await SendFluentResult(Result.Ok(new CountResponseDTO(result.Value)), ct);
    }
}

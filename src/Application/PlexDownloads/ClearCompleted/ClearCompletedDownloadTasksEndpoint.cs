using FastEndpoints;
using Reaparr.Application.Contracts;

namespace Reaparr.Application;

/// <summary>
/// Will clear any completed <see cref="DownloadTaskGeneric"/> from the database.
/// </summary>
/// <returns>Is successful.</returns>
public class ClearCompletedDownloadTasksEndpoint : BaseEndpoint<List<Guid>, ResultDTO<CountResponseDTO>>
{
    private readonly ICommandExecutor _commandExecutor;

    public override string EndpointPath => ApiRoutes.DownloadController + "/clear";

    public ClearCompletedDownloadTasksEndpoint(ICommandExecutor commandExecutor)
    {
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Verbs(Http.POST);
        Post(EndpointPath);

        Description(x => x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<CountResponseDTO>)));
    }

    public override async Task HandleAsync(List<Guid> downloadTaskIds, CancellationToken ct)
    {
        var result = await _commandExecutor.Send(new ClearCompletedDownloadTasksCommand(downloadTaskIds), ct);
        if (result.IsFailed)
        {
            await SendFluentResult(result.ToResult(), ct);
            return;
        }

        await SendFluentResult(Result.Ok(new CountResponseDTO(result.Value)), ct);
    }
}

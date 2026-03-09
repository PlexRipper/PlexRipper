using FastEndpoints;
using FluentValidation;
using Reaparr.Application.Contracts;

namespace Reaparr.Application;

/// <summary>
/// Clears specific completed <see cref="DownloadTaskGeneric"/> from the database by their IDs.
/// </summary>
/// <returns>Is successful.</returns>
public sealed class ClearCompletedDownloadTasksByDownloadTaskIdEndpointRequest
{
    public List<Guid> DownloadTaskIds { get; init; } = [];
}

public class ClearCompletedDownloadTasksByDownloadTaskIdEndpointRequestValidator
    : Validator<ClearCompletedDownloadTasksByDownloadTaskIdEndpointRequest>
{
    public ClearCompletedDownloadTasksByDownloadTaskIdEndpointRequestValidator()
    {
        RuleFor(x => x.DownloadTaskIds).NotEmpty();
    }
}

public class ClearCompletedDownloadTasksByDownloadTaskIdEndpoint
    : BaseEndpoint<ClearCompletedDownloadTasksByDownloadTaskIdEndpointRequest, ResultDTO<CountResponseDTO>>
{
    private readonly ICommandExecutor _commandExecutor;

    public override string EndpointPath => ApiRoutes.DownloadController + "/clear/tasks";

    public ClearCompletedDownloadTasksByDownloadTaskIdEndpoint(ICommandExecutor commandExecutor)
    {
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Delete(EndpointPath);

        Description(x => x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<CountResponseDTO>)));
    }

    public override async Task HandleAsync(
        ClearCompletedDownloadTasksByDownloadTaskIdEndpointRequest req,
        CancellationToken ct
    )
    {
        var result = await _commandExecutor.Send(
            new ClearCompletedDownloadTasksByDownloadTaskIdCommand(req.DownloadTaskIds),
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

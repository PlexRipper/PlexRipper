namespace Reaparr.Application;

/// <summary>
/// Clears specific completed <see cref="DownloadTaskGeneric"/> from the database by their IDs.
/// </summary>
/// <returns>Is successful.</returns>
public class ClearCompletedDownloadTasksByDownloadTaskIdEndpoint : BaseEndpoint<List<Guid>, ResultDTO<CountResponseDTO>>
{
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    public ClearCompletedDownloadTasksByDownloadTaskIdEndpoint(
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor
    )
    {
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Delete(ApiRoutes.DownloadController + "/clear/tasks");

        Description(x => x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<CountResponseDTO>)));
    }

    public override async Task HandleAsync(List<Guid> req, CancellationToken ct)
    {
        var keys = await _dbContext.GetDownloadTaskKeysAsync(req, ct);
        var result = await _commandExecutor.Send(new ClearCompletedDownloadTasksByDownloadTaskKeyCommand(keys), ct);
        if (result.IsFailed)
        {
            await Send.FluentResult(result.ToResult(), ct);
            return;
        }

        await Send.FluentResult(Result.Ok(new CountResponseDTO(result.Value)), ct);
    }
}

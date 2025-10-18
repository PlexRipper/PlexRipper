using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

public record DeleteFolderPathEndpointRequest(int Id);

public class DeleteFolderPathEndpointRequestValidator : Validator<DeleteFolderPathEndpointRequest>
{
    public DeleteFolderPathEndpointRequestValidator()
    {
        RuleFor(x => x.Id).GreaterThan(10).WithMessage("Cannot delete reserved folder paths with an Id less than 10");
    }
}

public class DeleteFolderPathEndpoint : BaseEndpoint<DeleteFolderPathEndpointRequest, BaseResultDTO>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.FolderPathController + "/{Id}";

    public DeleteFolderPathEndpoint(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<DeleteFolderPathEndpoint>();
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Delete(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(DeleteFolderPathEndpointRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);
        await _dbContext.FolderPaths.Where(x => x.Id == req.Id).ExecuteDeleteAsync(ct);
        _log.Here()
            .Debug("Deleted {FolderPathName} with Id: {CommandId} from the database", nameof(FolderPath), req.Id);

        await SendFluentResult(Result.Ok(), ct);
    }
}

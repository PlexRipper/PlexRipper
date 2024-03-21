using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Logging.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record DeleteFolderPathEndpointRequest(int Id);

public class DeleteFolderPathEndpointRequestValidator : Validator<DeleteFolderPathEndpointRequest>
{
    public DeleteFolderPathEndpointRequestValidator()
    {
        RuleFor(x => x.Id).GreaterThan(10).WithMessage("Cannot delete reserved folder paths with an Id less than 10");
    }
}

public class DeleteFolderPathEndpoint : BaseEndpoint<DeleteFolderPathEndpointRequest, ResultDTO>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.FolderPathController + "/{Id}";

    public DeleteFolderPathEndpoint(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Delete(EndpointPath);
        AllowAnonymous();
        Description(x => x
            .Produces(StatusCodes.Status200OK, typeof(ResultDTO))
            .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(DeleteFolderPathEndpointRequest req, CancellationToken ct)
    {
        await _dbContext.FolderPaths.Where(x => x.Id == req.Id).ExecuteDeleteAsync(ct);
        _log.Debug("Deleted {FolderPathName} with Id: {CommandId} from the database", nameof(FolderPath), req.Id);

        await SendFluentResult(Result.Ok(), ct);
    }
}
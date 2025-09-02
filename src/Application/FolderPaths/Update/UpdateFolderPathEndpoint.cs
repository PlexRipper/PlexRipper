using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.Logging;
using Serilog;

namespace Reaparr.Application;

public class UpdateFolderPathEndpointRequest
{
    [FromBody]
    public FolderPathDTO? FolderPathDto { get; init; }
}

public class UpdateFolderPathEndpointRequestValidator : Validator<UpdateFolderPathEndpointRequest>
{
    public UpdateFolderPathEndpointRequestValidator()
    {
        RuleFor(x => x.FolderPathDto).NotNull();
        RuleFor(x => x.FolderPathDto!.Id).GreaterThan(0);
        RuleFor(x => x.FolderPathDto!.DisplayName).NotEmpty();
        RuleFor(x => x.FolderPathDto!.Directory).NotEmpty();
        RuleFor(x => x.FolderPathDto!.FolderType).NotEqual(FolderType.Unknown);
        RuleFor(x => x.FolderPathDto!.MediaType).NotEqual(PlexMediaType.Unknown);
    }
}

public class UpdateFolderPathEndpoint : BaseEndpoint<UpdateFolderPathEndpointRequest, FolderPathDTO>
{
    private readonly Serilog.ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.FolderPathController + "/";

    public UpdateFolderPathEndpoint(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<UpdateFolderPathEndpoint>();
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Put(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<FolderPathDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(UpdateFolderPathEndpointRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);
        // TODO: Should prevent updating reserved folder paths with id < 10
        var folderPath = req.FolderPathDto!.ToModel();
        var folderPathDb = await _dbContext
            .FolderPaths.AsTracking()
            .FirstOrDefaultAsync(x => x.Id == folderPath.Id, ct);

        if (folderPathDb is null)
        {
            await SendFluentResult(ResultExtensions.EntityNotFound(nameof(FolderPath), folderPath.Id), ct);
            return;
        }

        _dbContext.Entry(folderPathDb).CurrentValues.SetValues(folderPath);
        await _dbContext.SaveChangesAsync(ct);

        await SendFluentResult(Result.Ok(folderPathDb), path => path.ToDTO(), ct);
    }
}

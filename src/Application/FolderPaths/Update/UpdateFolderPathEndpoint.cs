using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public class UpdateFolderPathEndpointRequest
{
    [FromBody]
    public FolderPathDTO FolderPathDto { get; init; }
}

public class UpdateFolderPathEndpointRequestValidator : Validator<UpdateFolderPathEndpointRequest>
{
    public UpdateFolderPathEndpointRequestValidator()
    {
        RuleFor(x => x.FolderPathDto).NotNull();
        RuleFor(x => x.FolderPathDto.Id).GreaterThan(0);
        RuleFor(x => x.FolderPathDto.DisplayName).NotEmpty();
        RuleFor(x => x.FolderPathDto.Directory).NotEmpty();
        RuleFor(x => x.FolderPathDto.FolderType).NotEqual(FolderType.None).NotEqual(FolderType.Unknown);
        RuleFor(x => x.FolderPathDto.MediaType).NotEqual(PlexMediaType.None).NotEqual(PlexMediaType.Unknown);
    }
}

public class UpdateFolderPathEndpoint : BaseCustomEndpoint<UpdateFolderPathEndpointRequest, ResultDTO<List<FolderPathDTO>>>
{
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.FolderPathController + "/";

    public UpdateFolderPathEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Put(EndpointPath);
        AllowAnonymous();
        Description(x => x
            .Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<FolderPathDTO>>))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(UpdateFolderPathEndpointRequest req, CancellationToken ct)
    {
        // TODO Should prevent updating reserved folder paths with id < 10
        var folderPath = req.FolderPathDto.ToEntity();
        var folderPathDb = await _dbContext.FolderPaths.AsTracking().FirstOrDefaultAsync(x => x.Id == folderPath.Id, ct);

        if (folderPathDb is null)
        {
            await SendResult(ResultExtensions.EntityNotFound(nameof(FolderPath), folderPath.Id), ct);
            return;
        }

        _dbContext.Entry(folderPathDb).CurrentValues.SetValues(folderPath);
        await _dbContext.SaveChangesAsync(ct);

        await SendResult(Result.Ok(folderPathDb.ToDTO()), ct);
    }
}
using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public class CreateFolderPathEndpointRequest
{
    [FromBody]
    public FolderPathDTO FolderPathDto { get; init; }
}

public class CreateFolderPathEndpointRequestValidator : Validator<CreateFolderPathEndpointRequest>
{
    public CreateFolderPathEndpointRequestValidator()
    {
        RuleFor(x => x.FolderPathDto).NotNull();
        RuleFor(x => x.FolderPathDto.Id).GreaterThan(0);
        RuleFor(x => x.FolderPathDto.DisplayName).NotEmpty();
        RuleFor(x => x.FolderPathDto.Directory).NotEmpty();
        RuleFor(x => x.FolderPathDto.FolderType).NotEqual(FolderType.None).NotEqual(FolderType.Unknown);
        RuleFor(x => x.FolderPathDto.MediaType).NotEqual(PlexMediaType.None).NotEqual(PlexMediaType.Unknown);
    }
}

public class CreateFolderPathEndpoint : BaseCustomEndpoint<CreateFolderPathEndpointRequest, ResultDTO<FolderPathDTO>>
{
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.FolderPathController + "/";

    public CreateFolderPathEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Post(EndpointPath);
        AllowAnonymous();
        Description(x => x
            .Produces(StatusCodes.Status200OK, typeof(ResultDTO<FolderPathDTO>))
            .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(CreateFolderPathEndpointRequest req, CancellationToken ct)
    {
        var folderPath = req.FolderPathDto.ToEntity();
        await _dbContext.FolderPaths.AddAsync(folderPath, ct);
        await _dbContext.SaveChangesAsync(ct);

        folderPath = await _dbContext.FolderPaths.GetAsync(folderPath.Id, ct);
        await SendResult(Result.Ok(folderPath.ToDTO()), ct);
    }
}
using Data.Contracts;
using FluentValidation;
using Logging.Interface;

namespace PlexRipper.Application;

public record CreateFolderPathCommand(FolderPath FolderPath) : IRequest<Result<FolderPath>>;

public class CreateFolderPathValidator : AbstractValidator<CreateFolderPathCommand>
{
    public CreateFolderPathValidator()
    {
        RuleFor(x => x.FolderPath).NotNull();
        RuleFor(x => x.FolderPath.DisplayName).NotEmpty();
        RuleFor(x => x.FolderPath.FolderType).NotEqual(FolderType.None);
        RuleFor(x => x.FolderPath.FolderType).NotEqual(FolderType.Unknown);
        RuleFor(x => x.FolderPath.MediaType).NotEqual(PlexMediaType.None);
        RuleFor(x => x.FolderPath.MediaType).NotEqual(PlexMediaType.Unknown);
    }
}

public class CreateFolderPathCommandHandler : IRequestHandler<CreateFolderPathCommand, Result<FolderPath>>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    public CreateFolderPathCommandHandler(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public async Task<Result<FolderPath>> Handle(CreateFolderPathCommand command, CancellationToken cancellationToken)
    {
        await _dbContext.FolderPaths.AddAsync(command.FolderPath, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var findResult = await _dbContext.FolderPaths.GetAsync(command.FolderPath.Id, cancellationToken);
        return Result.Ok(findResult);
    }
}
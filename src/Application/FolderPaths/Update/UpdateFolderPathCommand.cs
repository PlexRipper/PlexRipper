using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record UpdateFolderPathCommand(FolderPath FolderPath) : IRequest<Result<FolderPath>>;

public class UpdateFolderPathCommandValidator : AbstractValidator<UpdateFolderPathCommand>
{
    public UpdateFolderPathCommandValidator()
    {
        RuleFor(x => x.FolderPath).NotNull();
        RuleFor(x => x.FolderPath.Id).GreaterThan(0);
        RuleFor(x => x.FolderPath.DisplayName).NotEmpty();
        RuleFor(x => x.FolderPath.FolderType).NotEqual(FolderType.Unknown);
        RuleFor(x => x.FolderPath.MediaType).NotEqual(PlexMediaType.Unknown);
    }
}

public class UpdateFolderPathCommandHandler : IRequestHandler<UpdateFolderPathCommand, Result<FolderPath>>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    public UpdateFolderPathCommandHandler(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public async Task<Result<FolderPath>> Handle(UpdateFolderPathCommand command, CancellationToken cancellationToken)
    {
        var folderPath = await _dbContext.FolderPaths.AsTracking().FirstOrDefaultAsync(x => x.Id == command.FolderPath.Id, cancellationToken);

        if (folderPath == null)
            return ResultExtensions.EntityNotFound(nameof(FolderPath), command.FolderPath.Id);

        _dbContext.Entry(folderPath).CurrentValues.SetValues(command.FolderPath);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok(folderPath);
    }
}
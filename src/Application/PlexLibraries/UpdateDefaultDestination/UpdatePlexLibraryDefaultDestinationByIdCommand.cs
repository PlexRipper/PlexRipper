using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

/// <summary>
/// Set the default media destination where the media will be stored after the download process is finished.
/// </summary>
/// <param name="PlexLibraryId"></param>
/// <param name="FolderPathId"></param>
public record UpdatePlexLibraryDefaultDestinationByIdCommand(int PlexLibraryId, int FolderPathId) : IRequest<Result>;

public class UpdatePlexLibraryDefaultDestinationByIdCommandHandlerValidator : AbstractValidator<UpdatePlexLibraryDefaultDestinationByIdCommand>
{
    public UpdatePlexLibraryDefaultDestinationByIdCommandHandlerValidator()
    {
        RuleFor(x => x.PlexLibraryId).GreaterThan(0);
        RuleFor(x => x.FolderPathId).GreaterThan(0);
    }
}

public class UpdatePlexLibraryDefaultDestinationByIdCommandHandlerHandler :
    IRequestHandler<UpdatePlexLibraryDefaultDestinationByIdCommand, Result>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    public UpdatePlexLibraryDefaultDestinationByIdCommandHandlerHandler(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(UpdatePlexLibraryDefaultDestinationByIdCommand command, CancellationToken cancellationToken)
    {
        var plexLibraryDb = await _dbContext.PlexLibraries.AsTracking().FirstOrDefaultAsync(x => x.Id == command.PlexLibraryId, cancellationToken);

        plexLibraryDb.DefaultDestinationId = command.FolderPathId;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
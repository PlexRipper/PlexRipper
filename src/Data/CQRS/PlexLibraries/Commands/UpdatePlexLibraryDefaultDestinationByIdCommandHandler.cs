using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexLibraries;

public class UpdatePlexLibraryDefaultDestinationByIdCommandHandlerValidator : AbstractValidator<UpdatePlexLibraryDefaultDestinationByIdCommand>
{
    public UpdatePlexLibraryDefaultDestinationByIdCommandHandlerValidator()
    {
        RuleFor(x => x.PlexLibraryId).GreaterThan(0);
        RuleFor(x => x.FolderPathId).GreaterThan(0);
    }
}

public class UpdatePlexLibraryDefaultDestinationByIdCommandHandlerHandler : BaseHandler,
    IRequestHandler<UpdatePlexLibraryDefaultDestinationByIdCommand, Result>
{
    public UpdatePlexLibraryDefaultDestinationByIdCommandHandlerHandler(ILog log, PlexRipperDbContext dbContext) : base(log, dbContext) { }

    public async Task<Result> Handle(UpdatePlexLibraryDefaultDestinationByIdCommand command, CancellationToken cancellationToken)
    {
        var plexLibraryDb = await _dbContext.PlexLibraries.AsTracking().FirstOrDefaultAsync(x => x.Id == command.PlexLibraryId);

        plexLibraryDb.DefaultDestinationId = command.FolderPathId;

        await SaveChangesAsync();
        return Result.Ok();
    }
}
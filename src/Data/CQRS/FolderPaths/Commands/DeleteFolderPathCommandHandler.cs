using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.FolderPaths;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.FolderPaths
{
    public class DeleteFolderPathValidator : AbstractValidator<DeleteFolderPathCommand>
    {
        public DeleteFolderPathValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }

    public class DeleteFolderPathHandler : BaseHandler, IRequestHandler<DeleteFolderPathCommand, Result>
    {
        public DeleteFolderPathHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result> Handle(DeleteFolderPathCommand command, CancellationToken cancellationToken)
        {
            var folderPath = await _dbContext.FolderPaths.AsTracking().FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (folderPath == null)
            {
                return ResultExtensions.GetEntityNotFound(nameof(FolderPath), command.Id);
            }

            _dbContext.FolderPaths.Remove(folderPath);
            await _dbContext.SaveChangesAsync(cancellationToken);
            Log.Debug($"Deleted {nameof(FolderPath)} with Id: {command.Id} from the database");

            return Result.Ok();
        }
    }
}
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
    public class UpdateFolderPathCommandValidator : AbstractValidator<UpdateFolderPathCommand>
    {
        public UpdateFolderPathCommandValidator()
        {
            RuleFor(x => x.FolderPath).NotNull();
            RuleFor(x => x.FolderPath.Id).GreaterThan(0);
            RuleFor(x => x.FolderPath.DisplayName).NotEmpty();
            RuleFor(x => x.FolderPath.Type).NotEmpty();
        }
    }

    public class UpdateFolderPathCommandHandler : BaseHandler, IRequestHandler<UpdateFolderPathCommand, Result<FolderPath>>
    {
        public UpdateFolderPathCommandHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<FolderPath>> Handle(UpdateFolderPathCommand command, CancellationToken cancellationToken)
        {
            var folderPath = await _dbContext.FolderPaths.AsTracking().FirstOrDefaultAsync(x => x.Id == command.FolderPath.Id, cancellationToken);

            if (folderPath == null)
            {
                return ResultExtensions.GetEntityNotFound(nameof(FolderPath), command.FolderPath.Id);
            }

            _dbContext.Entry(folderPath).CurrentValues.SetValues(command.FolderPath);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok(folderPath);
        }
    }
}
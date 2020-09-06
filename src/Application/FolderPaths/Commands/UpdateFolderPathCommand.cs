using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.Base;
using PlexRipper.Domain;

namespace PlexRipper.Application.FolderPaths.Commands
{
    public class UpdateFolderPathCommand : IRequest<Result<FolderPath>>
    {
        public FolderPath FolderPath { get; }

        public UpdateFolderPathCommand(FolderPath folderPath)
        {
            FolderPath = folderPath;
        }
    }

    public class UpdateFolderPathCommandValidator : AbstractValidator<UpdateFolderPathCommand>
    {
        public UpdateFolderPathCommandValidator()
        {
            RuleFor(x => x.FolderPath).NotNull();
            RuleFor(x => x.FolderPath.Id).GreaterThan(0);
            RuleFor(x => x.FolderPath.DisplayName).NotEmpty();
            RuleFor(x => x.FolderPath.Type).NotEmpty();
            RuleFor(x => x.FolderPath.DirectoryPath).NotEmpty();

        }
    }

    public class UpdateFolderPathCommandHandler : BaseHandler, IRequestHandler<UpdateFolderPathCommand, Result<FolderPath>>
    {
        public UpdateFolderPathCommandHandler(IPlexRipperDbContext dbContext): base(dbContext) { }

        public async Task<Result<FolderPath>> Handle(UpdateFolderPathCommand command, CancellationToken cancellationToken)
        {
            var folderPath = await _dbContext.FolderPaths.AsTracking().FirstOrDefaultAsync(x => x.Id == command.FolderPath.Id, cancellationToken: cancellationToken);

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

using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Base;
using PlexRipper.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

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
            RuleFor(x => x.FolderPath.Directory).NotEmpty();

        }
    }

    public class UpdateFolderPathCommandHandler : BaseHandler, IRequestHandler<UpdateFolderPathCommand, Result<FolderPath>>
    {
        private readonly IPlexRipperDbContext _dbContext;

        public UpdateFolderPathCommandHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<FolderPath>> Handle(UpdateFolderPathCommand command, CancellationToken cancellationToken)
        {
            var folderPathInDb = await _dbContext.FolderPaths.AsTracking().FirstOrDefaultAsync(x => x.Id == command.FolderPath.Id);

            if (folderPathInDb == null)
            {
                return Result.Fail(new Error($"Could not find a FolderPath to update with id: {command.FolderPath.Id}"));
            }

            _dbContext.Entry(folderPathInDb).CurrentValues.SetValues(command.FolderPath);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok(folderPathInDb);

        }
    }
}

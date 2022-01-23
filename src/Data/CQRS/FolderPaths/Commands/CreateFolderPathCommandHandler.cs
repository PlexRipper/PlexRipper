using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using PlexRipper.Application;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.FolderPaths
{
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

    public class CreateFolderPathCommandHandler : BaseHandler, IRequestHandler<CreateFolderPathCommand, Result<int>>
    {
        public CreateFolderPathCommandHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<int>> Handle(CreateFolderPathCommand command, CancellationToken cancellationToken)
        {
            await _dbContext.FolderPaths.AddAsync(command.FolderPath);
            await _dbContext.SaveChangesAsync();
            return Result.Ok(command.FolderPath.Id);
        }
    }
}
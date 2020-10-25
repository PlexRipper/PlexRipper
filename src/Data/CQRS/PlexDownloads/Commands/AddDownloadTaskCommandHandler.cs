using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common;
using PlexRipper.Application.PlexDownloads.Commands;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.CQRS.PlexDownloads
{
    public class AddDownloadTaskCommandValidator : AbstractValidator<AddDownloadTaskCommand>
    {
        public AddDownloadTaskCommandValidator()
        {
            RuleFor(x => x.DownloadTask.Id).Equal(0);
            RuleFor(x => x.DownloadTask.PlexServerId).GreaterThanOrEqualTo(0);
            RuleFor(x => x.DownloadTask.PlexServer).NotNull();

            RuleFor(x => x.DownloadTask.PlexAccountId).GreaterThanOrEqualTo(0);

            RuleFor(x => x.DownloadTask.DownloadFolderId).GreaterThanOrEqualTo(0);
            RuleFor(x => x.DownloadTask.DownloadFolder).NotNull();
            RuleFor(x => x.DownloadTask.DownloadFolder.IsValid()).Must(x => x).WithMessage("The Download folder is not a valid directory");

            RuleFor(x => x.DownloadTask.DestinationFolderId).GreaterThanOrEqualTo(0);
            RuleFor(x => x.DownloadTask.DestinationFolder).NotNull();
            RuleFor(x => x.DownloadTask.DestinationFolder.IsValid()).Must(x => x).WithMessage("The Destination folder is not a valid directory");

            RuleFor(x => x.DownloadTask.FileName).NotEmpty();
        }
    }

    public class AddDownloadTaskCommandHandler : BaseHandler, IRequestHandler<AddDownloadTaskCommand, Result<int>>
    {
        public AddDownloadTaskCommandHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<int>> Handle(AddDownloadTaskCommand command, CancellationToken cancellationToken)
        {
            // This is nulled as to prevent EF Core thinking these should be added
            command.DownloadTask.DownloadFolder = null;
            command.DownloadTask.DestinationFolder = null;
            command.DownloadTask.PlexAccount = null;
            command.DownloadTask.PlexServer = null;
            command.DownloadTask.PlexLibrary = null;

            _dbContext.DownloadTasks.Attach(command.DownloadTask);
            _dbContext.Entry(command.DownloadTask).State = EntityState.Added;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _dbContext.Entry(command.DownloadTask).GetDatabaseValuesAsync(cancellationToken);

            return Result.Ok(command.DownloadTask.Id);
        }
    }
}
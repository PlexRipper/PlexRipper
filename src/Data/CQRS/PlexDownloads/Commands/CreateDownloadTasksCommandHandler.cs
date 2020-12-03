using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using PlexRipper.Application.PlexDownloads;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.PlexDownloads
{
    public class CreateDownloadTasksCommandValidator : AbstractValidator<CreateDownloadTasksCommand>
    {
        public CreateDownloadTasksCommandValidator()
        {
            RuleForEach(x => x.DownloadTasks).ChildRules(downloadTask =>
            {
                downloadTask.RuleFor(x => x.Id).Equal(0);
                downloadTask.RuleFor(x => x.PlexServerId).GreaterThan(0);
                downloadTask.RuleFor(x => x.DataTotal).GreaterThan(0);
                downloadTask.RuleFor(x => x.RatingKey).GreaterThan(0);
                downloadTask.RuleFor(x => x.MediaType).NotEqual(PlexMediaType.None);
                downloadTask.RuleFor(x => x.MediaType).NotEqual(PlexMediaType.Unknown);
                downloadTask.RuleFor(x => x.DownloadFolderId).GreaterThan(0);
                downloadTask.RuleFor(x => x.DestinationFolderId).GreaterThan(0);
                downloadTask.RuleFor(x => x.PlexLibraryId).GreaterThan(0);
                downloadTask.RuleFor(x => x.FileName).NotEmpty();
                downloadTask.RuleFor(x => x.FileLocationUrl).NotEmpty();
            });
        }
    }

    public class CreateDownloadTasksCommandHandler : BaseHandler, IRequestHandler<CreateDownloadTasksCommand, Result<List<int>>>
    {
        public CreateDownloadTasksCommandHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<List<int>>> Handle(CreateDownloadTasksCommand command, CancellationToken cancellationToken)
        {
            // Prevent the navigation properties from being updated
            command.DownloadTasks.ForEach(x =>
            {
                x.DestinationFolder = null;
                x.DownloadFolder = null;
                x.PlexServer = null;
                x.PlexLibrary = null;
                x.DownloadWorkerTasks = new List<DownloadWorkerTask>();
            });

            await _dbContext.AddRangeAsync(command.DownloadTasks, cancellationToken);
            await SaveChangesAsync();

            return Result.Ok(command.DownloadTasks.Select(x => x.Id).ToList());
        }
    }
}
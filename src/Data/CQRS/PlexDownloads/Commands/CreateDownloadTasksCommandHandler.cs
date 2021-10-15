using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using FluentResults;
using FluentValidation;
using MediatR;
using PlexRipper.Application;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.PlexDownloads
{
    public class CreateDownloadTasksCommandValidator : AbstractValidator<CreateDownloadTasksCommand>
    {
        public CreateDownloadTasksCommandValidator()
        {
            RuleForEach(x => x.DownloadTasks).ChildRules(downloadTask => { downloadTask.RuleFor(x => x.IsValid().IsSuccess).Equal(true); });
        }
    }

    public class CreateDownloadTasksCommandHandler : BaseHandler, IRequestHandler<CreateDownloadTasksCommand, Result<List<int>>>
    {
        public CreateDownloadTasksCommandHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<List<int>>> Handle(CreateDownloadTasksCommand command, CancellationToken cancellationToken)
        {
            // Prevent the navigation properties from being updated
            CleanUp(command.DownloadTasks);

            return Result.Ok(command.DownloadTasks.Select(x => x.Id).ToList());
        }

        private void CleanUp(List<DownloadTask> downloadTasks)
        {
            downloadTasks.ForEach(x =>
            {
                x.DestinationFolder = null;
                x.DownloadFolder = null;
                x.PlexServer = null;
                x.PlexLibrary = null;
            });

            _dbContext.BulkInsert(downloadTasks, _bulkConfig);

            foreach (var downloadTask in downloadTasks)
            {
                if (downloadTask.Children.Any())
                {
                    foreach (var downloadTaskChild in downloadTask.Children)
                    {
                        downloadTaskChild.ParentId = downloadTask.Id;
                    }

                    CleanUp(downloadTask.Children);
                }
            }
        }
    }
}
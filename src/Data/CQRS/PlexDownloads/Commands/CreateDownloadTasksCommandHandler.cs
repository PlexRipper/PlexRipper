using EFCore.BulkExtensions;
using FluentResults;
using FluentValidation;
using MediatR;
using PlexRipper.Application;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data
{
    public class CreateDownloadTasksCommandValidator : AbstractValidator<CreateDownloadTasksCommand>
    {
        public CreateDownloadTasksCommandValidator()
        {
            RuleForEach(x => x.DownloadTasks).ChildRules(x =>
            {
                x.RuleFor(y => y).NotNull();
                x.RuleFor(y => y.DataReceived).Equal(0);
                x.RuleFor(y => y.DataTotal).GreaterThan(0);
                x.RuleFor(y => y.Key).GreaterThan(0);
                x.RuleFor(y => y.MediaType).NotEqual(PlexMediaType.None).NotEqual(PlexMediaType.Unknown);
                x.RuleFor(y => y.Title).NotEmpty();
                x.RuleFor(y => y.DownloadDirectory).NotEmpty();
                x.RuleFor(y => y.DestinationDirectory).NotEmpty();
                x.RuleFor(y => y.FullTitle).NotEmpty();
                x.RuleFor(y => y.PlexServerId).GreaterThan(0);
                x.RuleFor(y => y.PlexLibraryId).GreaterThan(0);
                x.RuleFor(y => y.DownloadFolderId).GreaterThan(0);
                x.RuleFor(y => y.DestinationFolderId).GreaterThan(0);
                x.When(c => c.IsDownloadable, () =>
                {
                    x.RuleFor(y => y.ParentId).GreaterThan(0);

                    x.RuleFor(y => y.FileName).NotEmpty();

                    x.RuleFor(y => y.FileLocationUrl).NotEmpty();
                    x.RuleFor(y => y.DownloadUrl).NotEmpty();
                    x.RuleFor(y => y.DownloadUri).NotNull();
                    x.RuleFor(y => y.DownloadUri.IsAbsoluteUri).NotNull().When(y => y.DownloadUri != null);

                    x.RuleFor(y => Uri.IsWellFormedUriString(y.DownloadUri.AbsoluteUri, UriKind.Absolute)).NotEqual(false)
                        .When(y => y.DownloadUri != null);
                    x.RuleFor(y => y.Created).NotEqual(DateTime.MinValue);
                });
            });
        }
    }

    public class CreateDownloadTasksCommandHandler : BaseHandler, IRequestHandler<CreateDownloadTasksCommand, Result>
    {
        public CreateDownloadTasksCommandHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result> Handle(CreateDownloadTasksCommand command, CancellationToken cancellationToken)
        {
            // Prevent the navigation properties from being updated
            command.DownloadTasks.ForEach(x =>
            {
                x.DestinationFolder = null;
                x.DownloadFolder = null;
                x.PlexServer = null;
                x.PlexLibrary = null;
            });

            // Only create new tasks, downloadTasks can be nested in tasks that already are in the database.
            await _dbContext.BulkInsertAsync(command.DownloadTasks.FindAll(x => x.Id == 0), _bulkConfig, cancellationToken: cancellationToken);

            foreach (var downloadTask in command.DownloadTasks)
            {
                if (downloadTask.Children is null || !downloadTask.Children.Any())
                    continue;

                downloadTask.Children = SetRootId(downloadTask.Children, downloadTask.Id);
            }

            InsertDownloadTasks(command.DownloadTasks);

            return Result.Ok();
        }


        private static List<DownloadTask> SetRootId(List<DownloadTask> downloadTasks, int rootTaskId)
        {
            foreach (var downloadTask in downloadTasks)
            {
                downloadTask.RootDownloadTaskId = rootTaskId;
                if (downloadTask.Children is null || !downloadTask.Children.Any())
                    continue;

                downloadTask.Children = SetRootId(downloadTask.Children, rootTaskId);
            }

            return downloadTasks;
        }

        private void InsertDownloadTasks(List<DownloadTask> downloadTasks)
        {
            downloadTasks.ForEach(x =>
            {
                x.DestinationFolder = null;
                x.DownloadFolder = null;
                x.PlexServer = null;
                x.PlexLibrary = null;
            });

            // Only create new tasks, downloadTasks can be nested in tasks that already are in the database.
            _dbContext.BulkInsert(downloadTasks.FindAll(x => x.Id == 0), _bulkConfig);

            foreach (var downloadTask in downloadTasks)
            {
                if (downloadTask.Children.Any())
                {
                    foreach (var downloadTaskChild in downloadTask.Children)
                    {
                        downloadTaskChild.ParentId = downloadTask.Id;
                        downloadTaskChild.Parent = null;
                    }

                    InsertDownloadTasks(downloadTask.Children);
                }
            }
        }
    }
}
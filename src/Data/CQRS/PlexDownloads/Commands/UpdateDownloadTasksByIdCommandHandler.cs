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

namespace PlexRipper.Data
{
    public class UpdateDownloadTasksByIdCommandValidator : AbstractValidator<UpdateDownloadTasksByIdCommand>
    {
        public UpdateDownloadTasksByIdCommandValidator()
        {
            RuleForEach(x => x.DownloadTasks.Flatten(y => y.Children).ToList())
                .ChildRules(x =>
                {
                    x.RuleFor(y => y).NotNull();
                    x.RuleFor(y => y.DataReceived).GreaterThanOrEqualTo(0);
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
                }).OverridePropertyName("Children");
        }
    }

    public class UpdateDownloadTasksByIdCommandHandler : BaseHandler, IRequestHandler<UpdateDownloadTasksByIdCommand, Result>
    {
        public UpdateDownloadTasksByIdCommandHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result> Handle(UpdateDownloadTasksByIdCommand command, CancellationToken cancellationToken)
        {
            var downloadTasks = command.DownloadTasks.Flatten(x => x.Children).ToList();

            await _dbContext.BulkUpdateAsync(downloadTasks, cancellationToken: cancellationToken);
            await _dbContext.BulkUpdateAsync(downloadTasks.SelectMany(x => x.DownloadWorkerTasks).ToList(),
                cancellationToken: cancellationToken);

            await SaveChangesAsync();

            return Result.Ok();
        }
    }
}
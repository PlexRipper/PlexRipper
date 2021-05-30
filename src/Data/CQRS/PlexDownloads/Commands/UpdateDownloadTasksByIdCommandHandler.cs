using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using PlexRipper.Application.PlexDownloads;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.CQRS.PlexDownloads
{
    public class UpdateDownloadTasksByIdCommandValidator : AbstractValidator<UpdateDownloadTasksByIdCommand>
    {
        public UpdateDownloadTasksByIdCommandValidator()
        {
            RuleForEach(x => x.DownloadTasks).NotNull();
            RuleForEach(x => x.DownloadTasks)
                .ChildRules(x => x.RuleFor(y => y.IsValid().IsSuccess).Equal(true));
        }
    }

    public class UpdateDownloadTasksByIdCommandHandler : BaseHandler,
        IRequestHandler<UpdateDownloadTasksByIdCommand, Result>
    {
        public UpdateDownloadTasksByIdCommandHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result> Handle(UpdateDownloadTasksByIdCommand command, CancellationToken cancellationToken)
        {
            // Prevent the navigation properties from being updated
            command.DownloadTasks.ForEach(x =>
            {
                x.DestinationFolder = null;
                x.DownloadFolder = null;
                x.PlexServer = null;
                x.PlexLibrary = null;
            });
            _dbContext.UpdateRange(command.DownloadTasks);
            await SaveChangesAsync();
            return Result.Ok();
        }
    }
}
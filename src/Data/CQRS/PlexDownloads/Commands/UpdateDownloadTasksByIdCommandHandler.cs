using System.Linq;
using PlexRipper.Domain;
using System.Threading;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using FluentResults;
using FluentValidation;
using MediatR;
using PlexRipper.Application;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.CQRS.PlexDownloads
{
    public class UpdateDownloadTasksByIdCommandValidator : AbstractValidator<UpdateDownloadTasksByIdCommand>
    {
        public UpdateDownloadTasksByIdCommandValidator()
        {
            RuleForEach(x => x.DownloadTasks).NotNull();
            RuleForEach(x => x.DownloadTasks)
                .ChildRules(x => x.RuleFor(y => y).SetValidator(new DownloadTaskValidator()));
        }
    }

    public class UpdateDownloadTasksByIdCommandHandler : BaseHandler,
        IRequestHandler<UpdateDownloadTasksByIdCommand, Result>
    {
        public UpdateDownloadTasksByIdCommandHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result> Handle(UpdateDownloadTasksByIdCommand command, CancellationToken cancellationToken)
        {
            await _dbContext.BulkUpdateAsync(command.DownloadTasks, cancellationToken: cancellationToken);
            await _dbContext.BulkUpdateAsync(command.DownloadTasks.SelectMany(x => x.DownloadWorkerTasks).ToList(), cancellationToken: cancellationToken);

            await SaveChangesAsync();

            return Result.Ok();
        }
    }
}
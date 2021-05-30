using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
            _dbContext.UpdateRange(command.DownloadTasks);

            // Prevent the navigation properties from being updated
            command.DownloadTasks.ForEach(x =>
            {
                _dbContext.Entry(x.DestinationFolder).State = EntityState.Unchanged;
                _dbContext.Entry(x.DownloadFolder).State = EntityState.Unchanged;
                _dbContext.Entry(x.PlexServer).State = EntityState.Unchanged;
                _dbContext.Entry(x.PlexLibrary).State = EntityState.Unchanged;
            });

            await SaveChangesAsync();
            return Result.Ok();
        }
    }
}
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Base;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Application.PlexDownloads.Commands
{
    public class DeleteDownloadTaskByIdCommand : IRequest<Result<bool>>
    {
        public int DownloadTaskId { get; }

        public DeleteDownloadTaskByIdCommand(int downloadTaskId)
        {
            DownloadTaskId = downloadTaskId;
        }
    }

    public class DeleteDownloadTaskByIdCommandValidator : AbstractValidator<DeleteDownloadTaskByIdCommand>
    {
        public DeleteDownloadTaskByIdCommandValidator()
        {
            RuleFor(x => x.DownloadTaskId).GreaterThan(0);
        }
    }

    public class DeleteDownloadTaskByIDHandler : BaseHandler, IRequestHandler<DeleteDownloadTaskByIdCommand, Result<bool>>
    {
        private readonly IPlexRipperDbContext _dbContext;

        public DeleteDownloadTaskByIDHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<bool>> Handle(DeleteDownloadTaskByIdCommand command, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.DownloadTasks.AsTracking().FirstOrDefaultAsync(x => x.Id == command.DownloadTaskId);
            if (entity == null)
            {
                return Result.Fail($"Could not find a DownloadTask with id: {command.DownloadTaskId}");
            }

            _dbContext.DownloadTasks.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok(true);

        }
    }
}

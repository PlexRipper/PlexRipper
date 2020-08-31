using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.Base;

namespace PlexRipper.Application.PlexServers.Commands
{
    public class CreatePlexServerStatusCommand : IRequest<Result<int>>
    {
        public PlexServerStatus PlexServerStatus { get; }

        public CreatePlexServerStatusCommand(PlexServerStatus plexServerStatus)
        {
            PlexServerStatus = plexServerStatus;
        }
    }

    public class CreatePlexServerStatusCommandValidator : AbstractValidator<CreatePlexServerStatusCommand>
    {
        public CreatePlexServerStatusCommandValidator()
        {
            RuleFor(x => x.PlexServerStatus).NotNull();
            RuleFor(x => x.PlexServerStatus.Id).Equal(0);
            RuleFor(x => x.PlexServerStatus.PlexServerId).GreaterThan(0);
            RuleFor(x => x.PlexServerStatus.LastChecked).NotNull();
            RuleFor(x => x.PlexServerStatus.StatusMessage).NotEmpty();
            RuleFor(x => x.PlexServerStatus.PlexServer).NotNull();
            RuleFor(x => x.PlexServerStatus.PlexServer.Id).GreaterThan(0);
        }
    }

    public class CreatePlexServerStatusCommandHandler : BaseHandler, IRequestHandler<CreatePlexServerStatusCommand, Result<int>>
    {
        public CreatePlexServerStatusCommandHandler(IPlexRipperDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Result<int>> Handle(CreatePlexServerStatusCommand command, CancellationToken cancellationToken)
        {
            Log.Debug("Creating a new PlexServerStatus in the DB");

            await _dbContext.PlexServerStatuses.AddAsync(command.PlexServerStatus, cancellationToken);
            if (command.PlexServerStatus.PlexServer != null)
            {
                _dbContext.Entry(command.PlexServerStatus.PlexServer).State = EntityState.Unchanged;
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _dbContext.Entry(command.PlexServerStatus).GetDatabaseValuesAsync(cancellationToken);

            return Result.Ok(command.PlexServerStatus.Id);
        }
    }
}
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain;
using PlexRipper.Domain.Base;
using PlexRipper.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Application.PlexServers.Commands
{
    public class CreatePlexServerStatusCommand : IRequest<Result<PlexServerStatus>>
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

    public class CreatePlexServerStatusCommandHandler : BaseHandler, IRequestHandler<CreatePlexServerStatusCommand, Result<PlexServerStatus>>
    {
        private readonly IPlexRipperDbContext _dbContext;

        public CreatePlexServerStatusCommandHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<PlexServerStatus>> Handle(CreatePlexServerStatusCommand command, CancellationToken cancellationToken)
        {
            var result = await ValidateAsync<CreatePlexServerStatusCommand, CreatePlexServerStatusCommandValidator>(command);
            if (result.IsFailed) return result;


            Log.Debug("Creating a new PlexServerStatus in the DB");

            await _dbContext.PlexServerStatuses.AddAsync(command.PlexServerStatus);
            _dbContext.Entry(command.PlexServerStatus.PlexServer).State = EntityState.Unchanged;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _dbContext.Entry(command.PlexServerStatus).GetDatabaseValuesAsync();

            return ReturnResult(command.PlexServerStatus, command.PlexServerStatus.Id);
        }
    }
}

using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Base;
using PlexRipper.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Application.PlexAccounts
{
    public class UpdatePlexAccountCommand : IRequest<Result<PlexAccount>>
    {
        public PlexAccount PlexAccount { get; }

        public UpdatePlexAccountCommand(PlexAccount plexAccount)
        {
            PlexAccount = plexAccount;
        }
    }

    public class UpdatePlexAccountValidator : AbstractValidator<UpdatePlexAccountCommand>
    {
        public UpdatePlexAccountValidator()
        {
            RuleFor(x => x.PlexAccount).NotNull();
            RuleFor(x => x.PlexAccount.Id).GreaterThan(0);
            RuleFor(x => x.PlexAccount.DisplayName).NotEmpty();
            RuleFor(x => x.PlexAccount.Username).NotEmpty().MinimumLength(5);
            RuleFor(x => x.PlexAccount.Password).NotEmpty().MinimumLength(5);
            RuleFor(x => x.PlexAccount.IsValidated).NotNull();
            RuleFor(x => x.PlexAccount.PlexId).GreaterThan(0);
            RuleFor(x => x.PlexAccount.Uuid).NotEmpty().MinimumLength(5);
            RuleFor(x => x.PlexAccount.Title).NotEmpty().MinimumLength(5);
            RuleFor(x => x.PlexAccount.AuthenticationToken).NotEmpty().MinimumLength(10);
        }
    }

    public class UpdatePlexAccountHandler : BaseHandler, IRequestHandler<UpdatePlexAccountCommand, Result<PlexAccount>>
    {
        private readonly IPlexRipperDbContext _dbContext;

        public UpdatePlexAccountHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<PlexAccount>> Handle(UpdatePlexAccountCommand command, CancellationToken cancellationToken)
        {
            var plexAccount = command.PlexAccount;
            var accountInDb = await _dbContext.PlexAccounts
                .Include(x => x.PlexAccountServers)
                .ThenInclude(x => x.PlexServer)
                .AsTracking().FirstOrDefaultAsync(x => x.Id == plexAccount.Id);

            if (accountInDb == null)
            {
                return Result.Fail(new Error($"Could not find a PlexAccount to update with id: {plexAccount.Id}"));
            }

            _dbContext.Entry(accountInDb).CurrentValues.SetValues(plexAccount);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}

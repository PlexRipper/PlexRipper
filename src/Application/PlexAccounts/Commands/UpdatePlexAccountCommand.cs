using FluentValidation;
using MediatR;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Application.PlexAccounts
{
    public class UpdatePlexAccountCommand : IRequest<ValidationResponse<PlexAccount>>
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
            RuleFor(x => x.PlexAccount.Username).NotEmpty().MinimumLength(5);
            RuleFor(x => x.PlexAccount.Password).NotEmpty().MinimumLength(5);
            RuleFor(x => x.PlexAccount.DisplayName).NotEmpty();
        }
    }

    public class UpdatePlexAccountHandler : IRequestHandler<UpdatePlexAccountCommand, ValidationResponse<PlexAccount>>
    {
        private readonly IPlexRipperDbContext _dbContext;

        public UpdatePlexAccountHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ValidationResponse<PlexAccount>> Handle(UpdatePlexAccountCommand command, CancellationToken cancellationToken)
        {
            _dbContext.PlexAccounts.Update(command.PlexAccount);
            await _dbContext.SaveChangesAsync();
            return new ValidationResponse<PlexAccount>(command.PlexAccount);

        }
    }
}

using FluentValidation;
using MediatR;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Application.PlexAccounts
{
    public class CreatePlexAccountCommand : IRequest<ValidationResponse<PlexAccount>>
    {
        public PlexAccount PlexAccount { get; }

        public CreatePlexAccountCommand(PlexAccount plexAccount)
        {
            PlexAccount = plexAccount;
        }
    }

    public class CreatePlexAccountValidator : AbstractValidator<CreatePlexAccountCommand>
    {
        public CreatePlexAccountValidator()
        {
            RuleFor(x => x.PlexAccount.Id).Equal(0).WithMessage("The Id should be 0 when creating a new PlexAccount");
            RuleFor(x => x.PlexAccount.Username).NotEmpty().MinimumLength(5);
            RuleFor(x => x.PlexAccount.Password).NotEmpty().MinimumLength(5);
            RuleFor(x => x.PlexAccount.DisplayName).NotEmpty();
        }
    }

    public class CreateAccountHandler : IRequestHandler<CreatePlexAccountCommand, ValidationResponse<PlexAccount>>
    {
        private readonly IPlexRipperDbContext _dbContext;

        public CreateAccountHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ValidationResponse<PlexAccount>> Handle(CreatePlexAccountCommand command, CancellationToken cancellationToken)
        {
            Log.Debug("Creating a new Account in DB");

            await _dbContext.PlexAccounts.AddAsync(command.PlexAccount);
            await _dbContext.SaveChangesAsync();
            await _dbContext.Entry(command.PlexAccount).GetDatabaseValuesAsync();

            return new ValidationResponse<PlexAccount>(command.PlexAccount);

        }
    }
}

using FluentResults;
using FluentValidation;
using MediatR;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.Base;

namespace PlexRipper.Application.PlexAccounts
{
    public class CreatePlexAccountCommand : IRequest<Result<int>>
    {
        public PlexAccount PlexAccount { get; }

        public CreatePlexAccountCommand(PlexAccount plexAccount)
        {
            PlexAccount = plexAccount;
        }
    }

    public class CreatePlexAccountCommandValidator : AbstractValidator<CreatePlexAccountCommand>
    {
        public CreatePlexAccountCommandValidator()
        {
            RuleFor(x => x.PlexAccount.Id).Equal(0).WithMessage("The Id should be 0 when creating a new PlexAccount");
            RuleFor(x => x.PlexAccount.Username).NotEmpty().MinimumLength(5);
            RuleFor(x => x.PlexAccount.Password).NotEmpty().MinimumLength(5);
            RuleFor(x => x.PlexAccount.DisplayName).NotEmpty();
        }
    }

    public class CreateAccountHandler : BaseHandler, IRequestHandler<CreatePlexAccountCommand, Result<int>>
    {
        public CreateAccountHandler(IPlexRipperDbContext dbContext): base(dbContext) { }

        public async Task<Result<int>> Handle(CreatePlexAccountCommand command, CancellationToken cancellationToken)
        {
            Log.Debug("Creating a new Account in DB");

            await _dbContext.PlexAccounts.AddAsync(command.PlexAccount, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _dbContext.Entry(command.PlexAccount).GetDatabaseValuesAsync(cancellationToken);

            return Result.Ok(command.PlexAccount.Id);
        }
    }
}

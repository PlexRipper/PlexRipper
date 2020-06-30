using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Application.Accounts.Queries
{
    /// <summary>
    /// Returns the <see cref="Account"/> by its id without any includes.
    /// </summary>
    public class GetAccountByUsernameQuery : IRequest<Account>
    {
        public string Username { get; }

        public GetAccountByUsernameQuery(string username)
        {
            Username = username;
        }

    }
    public class GetAccountByUsernameValidator : AbstractValidator<GetAccountByUsernameQuery>
    {
        public GetAccountByUsernameValidator()
        {
            RuleFor(x => x.Username).Length(5, 250).NotEmpty();
        }
    }

    public class GetAccountByUsernameQueryHandler : IRequestHandler<GetAccountByUsernameQuery, Account>
    {
        private readonly IPlexRipperDbContext _dbContext;

        public GetAccountByUsernameQueryHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<Account> Handle(GetAccountByUsernameQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.Accounts
                .FirstOrDefaultAsync(x => x.Username == request.Username);
        }
    }
}

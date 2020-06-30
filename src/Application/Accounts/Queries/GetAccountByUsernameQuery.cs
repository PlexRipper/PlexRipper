using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Application.Accounts.Queries
{
    /// <summary>
    /// Returns the <see cref="Account"/> by its id without any includes.
    /// </summary>
    public class GetAccountByUsernameQuery : IRequest<ValidationResponse<Account>>
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

    public class GetAccountByUsernameQueryHandler : IRequestHandler<GetAccountByUsernameQuery, ValidationResponse<Account>>
    {
        private readonly IPlexRipperDbContext _dbContext;

        public GetAccountByUsernameQueryHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ValidationResponse<Account>> Handle(GetAccountByUsernameQuery request, CancellationToken cancellationToken)
        {
            var account = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.Username == request.Username);

            return new ValidationResponse<Account>(account);

        }
    }
}

using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Application.PlexAccounts
{
    /// <summary>
    /// Returns the <see cref="PlexAccount"/> by its id without any includes.
    /// </summary>
    public class GetPlexAccountByUsernameQuery : IRequest<Result<PlexAccount>>
    {
        public string Username { get; }

        public GetPlexAccountByUsernameQuery(string username)
        {
            Username = username;
        }

    }

    public class GetAccountByUsernameValidator : AbstractValidator<GetPlexAccountByUsernameQuery>
    {
        public GetAccountByUsernameValidator()
        {
            RuleFor(x => x.Username).Length(5, 250).NotEmpty();
        }
    }

    public class GetPlexAccountByUsernameQueryHandler : IRequestHandler<GetPlexAccountByUsernameQuery, Result<PlexAccount>>
    {
        private readonly IPlexRipperDbContext _dbContext;

        public GetPlexAccountByUsernameQueryHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<PlexAccount>> Handle(GetPlexAccountByUsernameQuery request, CancellationToken cancellationToken)
        {
            var account = await _dbContext.PlexAccounts.FirstOrDefaultAsync(x => x.Username == request.Username);

            return Result.Ok(account);

        }
    }
}

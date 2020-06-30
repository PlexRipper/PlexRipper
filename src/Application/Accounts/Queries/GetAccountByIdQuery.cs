using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Application.Accounts.Queries
{
    /// <summary>
    /// Returns the <see cref="Account"/> by its id with an include to the <see cref="PlexAccount"/>.
    /// </summary>
    public class GetAccountByIdQuery : AbstractValidator<Account>, IRequest<Account>
    {
        public GetAccountByIdQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }



    public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, Account>
    {
        private readonly IPlexRipperDbContext _dbContext;

        public GetAccountByIdQueryHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<Account> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.Accounts
                .Include(x => x.PlexAccount)
                .ThenInclude(x => x.PlexAccountServers)
                .Where(x => x.PlexAccount.PlexAccountServers
                    .Any(y => y.PlexAccountId == request.Id))
                .FirstOrDefaultAsync(x => x.Id == request.Id);
        }
    }
}

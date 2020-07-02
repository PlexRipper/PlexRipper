using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Application.PlexAccounts
{
    public class GetPlexAccountByIdQuery : IRequest<Result<PlexAccount>>
    {
        public int Id { get; }

        /// <summary>
        /// Returns the <see cref="PlexAccount"/> by its id with an include <see cref="PlexServer"/>s.
        /// </summary>
        public GetPlexAccountByIdQuery(int id)
        {
            Id = id;
        }
    }

    public class GetAccountByIdQueryValidator : AbstractValidator<GetPlexAccountByIdQuery>
    {
        public GetAccountByIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }


    public class GetAccountByIdQueryHandler : IRequestHandler<GetPlexAccountByIdQuery, Result<PlexAccount>>
    {
        private readonly IPlexRipperDbContext _dbContext;

        public GetAccountByIdQueryHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<PlexAccount>> Handle(GetPlexAccountByIdQuery request, CancellationToken cancellationToken)
        {
            var account = await _dbContext.PlexAccounts
                .Include(x => x.PlexAccountServers)
                .Where(x => x.PlexAccountServers
                    .Any(y => y.PlexAccountId == request.Id))
                .FirstOrDefaultAsync(x => x.Id == request.Id);
            return Result.Ok(account);
        }
    }
}

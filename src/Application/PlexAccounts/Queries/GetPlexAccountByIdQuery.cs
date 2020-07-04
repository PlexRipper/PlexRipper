using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Entities;
using System.Collections.Generic;
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

            var list = _dbContext.PlexAccountLibraries.Include(x => x.PlexServer).ThenInclude(z => z.PlexLibraries).Where(b => b.PlexAccountId == request.Id).ToList();

            var account = await _dbContext.PlexAccounts
                .Include(x => x.PlexAccountServers)
                .ThenInclude(x => x.PlexServer)
                .ThenInclude(x => x.PlexLibraries
                    .Where(z => list.Any(n => n.PlexServerId == z.PlexServerId && n.PlexLibraryId == z.Id)))
                .FirstOrDefaultAsync(x => x.Id == request.Id);

            var dds = await _dbContext.PlexServers
                .Include(x => x.PlexLibraries
                    .Where(z => list.Any(n => n.PlexServerId == z.PlexServerId && n.PlexLibraryId == z.Id)))
                .ToListAsync();

            foreach (var server in account.PlexAccountServers)
            {
                var c = server.PlexServer.PlexLibraries.ToList();


                var x = new List<PlexLibrary>();
                for (int i = 0; i < server.PlexServer.PlexLibraries.Count; i++)
                {

                }

                foreach (var library in server.PlexServer.PlexLibraries)
                {

                }

            }

            return Result.Ok(new PlexAccount());
        }
    }
}

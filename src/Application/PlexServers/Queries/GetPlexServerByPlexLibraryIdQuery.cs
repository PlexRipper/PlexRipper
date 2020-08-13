using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Base;
using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexServers.Queries
{
    public class GetPlexServerByPlexLibraryIdQuery : IRequest<Result<PlexServer>>
    {
        public GetPlexServerByPlexLibraryIdQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }

    public class GetPlexServerByPlexLibraryIdQueryValidator : AbstractValidator<GetPlexServerByPlexLibraryIdQuery>
    {
        public GetPlexServerByPlexLibraryIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }


    public class GetPlexServerByPlexLibraryIdQueryHandler : BaseHandler,
        IRequestHandler<GetPlexServerByPlexLibraryIdQuery, Result<PlexServer>>
    {
        private readonly IPlexRipperDbContext _dbContext;

        public GetPlexServerByPlexLibraryIdQueryHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<PlexServer>> Handle(GetPlexServerByPlexLibraryIdQuery request,
            CancellationToken cancellationToken)
        {
            var result = await ValidateAsync<GetPlexServerByPlexLibraryIdQuery, GetPlexServerByPlexLibraryIdQueryValidator>(request);
            if (result.IsFailed) return result;

            var plexServer = await _dbContext.PlexServers
                .Include(x => x.PlexLibraries)
                .Where(x => x.PlexLibraries.Any(y => y.Id == request.Id))
                .FirstOrDefaultAsync(cancellationToken);

            if (plexServer == null)
            {
                return result.Set404NotFoundError();
            }

            return ReturnResult(plexServer);
        }
    }
}

using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common.Base;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexMovies
{
    public class GetPlexMovieByIdQuery : IRequest<Result<PlexMovie>>
    {
        public GetPlexMovieByIdQuery(int id, bool includePlexLibrary = false, bool includePlexServer = false)
        {
            Id = id;
            IncludePlexLibrary = includePlexLibrary;
            IncludePlexServer = includePlexServer;
        }

        public int Id { get; }
        public bool IncludePlexLibrary { get; }
        public bool IncludePlexServer { get; }
    }

    public class GetPlexMovieByIdQueryValidator : AbstractValidator<GetPlexMovieByIdQuery>
    {
        public GetPlexMovieByIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }


    public class GetPlexMovieByIdQueryHandler : BaseHandler, IRequestHandler<GetPlexMovieByIdQuery, Result<PlexMovie>>
    {
        public GetPlexMovieByIdQueryHandler(IPlexRipperDbContext dbContext): base(dbContext) { }

        public async Task<Result<PlexMovie>> Handle(GetPlexMovieByIdQuery request, CancellationToken cancellationToken)
        {
            var query = _dbContext.PlexMovies.AsQueryable();

            if (request.IncludePlexLibrary && !request.IncludePlexServer)
            {
                query = query
                    .Include(x => x.PlexLibrary);
            }

            if (request.IncludePlexLibrary && request.IncludePlexServer)
            {
                query = query
                    .Include(v => v.PlexLibrary)
                    .ThenInclude(x => x.PlexServer);
            }

            var plexMovie = await query.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (plexMovie == null)
            {
                return ResultExtensions.GetEntityNotFound(nameof(PlexMovie), request.Id);
            }
            return Result.Ok(plexMovie);
        }
    }
}

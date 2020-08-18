using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common.Base;

namespace PlexRipper.Application.PlexMovies
{
    public class GetPlexMovieByIdQuery : IRequest<Result<PlexMovie>>
    {
        public GetPlexMovieByIdQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }
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
            var result = await ValidateAsync<GetPlexMovieByIdQuery, GetPlexMovieByIdQueryValidator>(request);
            if (result.IsFailed) return result;

            var plexMovie = await _dbContext.PlexMovies
                                    .Include(x => x.PlexLibrary)
                                    .ThenInclude(x => x.PlexServer)
                                    .FirstOrDefaultAsync(x => x.Id == request.Id);

            return ReturnResult(plexMovie, request.Id);
        }
    }
}

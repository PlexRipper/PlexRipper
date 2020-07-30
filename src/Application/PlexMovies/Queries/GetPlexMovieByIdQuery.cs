using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Base;
using PlexRipper.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

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
        private readonly IPlexRipperDbContext _dbContext;

        public GetPlexMovieByIdQueryHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

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

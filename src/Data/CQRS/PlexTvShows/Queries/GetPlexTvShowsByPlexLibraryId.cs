using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.PlexTvShows.Queries;
using PlexRipper.Domain;

namespace PlexRipper.Data.Queries
{
    public class GetPlexTvShowsByPlexLibraryIdValidator : AbstractValidator<GetPlexTvShowsByPlexLibraryId>
    {
        public GetPlexTvShowsByPlexLibraryIdValidator()
        {
            RuleFor(x => x.PlexLibraryId).GreaterThan(0);
        }
    }

    public class GetPlexTvShowsByPlexLibraryIdHandler : IRequestHandler<GetPlexTvShowsByPlexLibraryId, Result<List<PlexTvShow>>>
    {
        private readonly PlexRipperDbContext _dbContext;

        public GetPlexTvShowsByPlexLibraryIdHandler(PlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<List<PlexTvShow>>> Handle(GetPlexTvShowsByPlexLibraryId request, CancellationToken cancellationToken)
        {
            var plexTvShows = await _dbContext.PlexTvShows
                .Include(x => x.Seasons)
                .ThenInclude(x => x.Episodes)
                .ThenInclude(x => x.EpisodeData)
                .ThenInclude(x => x.Parts)
                .Where(x => x.PlexLibraryId == request.PlexLibraryId)
                .ToListAsync(cancellationToken);

            return Result.Ok(plexTvShows);
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.PlexTvShows;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.PlexTvShows
{
    public class GetPlexTvShowsByPlexLibraryIdHandler : IRequestHandler<GetPlexTvShowsByPlexLibraryIdQuery, Result<List<PlexTvShow>>>
    {
        private readonly PlexRipperDbContext _dbContext;

        public GetPlexTvShowsByPlexLibraryIdHandler(PlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<List<PlexTvShow>>> Handle(GetPlexTvShowsByPlexLibraryIdQuery request, CancellationToken cancellationToken)
        {
            var plexTvShows = await _dbContext.PlexTvShows
                .Include(x => x.Seasons)
                .ThenInclude(x => x.Episodes)
                .Where(x => x.PlexLibraryId == request.PlexLibraryId)
                .ToListAsync(cancellationToken);

            return Result.Ok(plexTvShows);
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.PlexTvShows
{
    public class GetPlexTvShowsByPlexLibraryIdQueryHandler : BaseHandler,
        IRequestHandler<GetPlexTvShowsByPlexLibraryIdQuery, Result<List<PlexTvShow>>>
    {
        public GetPlexTvShowsByPlexLibraryIdQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<List<PlexTvShow>>> Handle(GetPlexTvShowsByPlexLibraryIdQuery request, CancellationToken cancellationToken)
        {
            var plexTvShows = await PlexTvShowsQueryable.IncludeEpisodes()
                .Where(x => x.PlexLibraryId == request.PlexLibraryId)
                .ToListAsync(cancellationToken);

            return Result.Ok(plexTvShows);
        }
    }
}
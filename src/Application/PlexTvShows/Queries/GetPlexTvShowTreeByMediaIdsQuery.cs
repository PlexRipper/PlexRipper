using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using PlexRipper.Domain;
using FluentResults;

namespace PlexRipper.Application.PlexTvShows
{
    public class GetPlexTvShowTreeByMediaIdsQuery : IRequest<Result<List<PlexTvShow>>>
    {
        public List<int> TvShowIds { get; }

        public List<int> TvShowSeasonIds { get; }

        public List<int> TvShowEpisodeIds { get; }

        public GetPlexTvShowTreeByMediaIdsQuery(List<int> tvShowIds, List<int> tvShowSeasonIds, List<int> tvShowEpisodeIds)
        {
            TvShowIds = tvShowIds;
            TvShowSeasonIds = tvShowSeasonIds;
            TvShowEpisodeIds = tvShowEpisodeIds;
        }
    }


}
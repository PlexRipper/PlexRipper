using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application
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
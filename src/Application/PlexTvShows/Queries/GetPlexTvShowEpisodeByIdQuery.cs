using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexTvShows
{
    public class GetPlexTvShowEpisodeByIdQuery : IRequest<Result<PlexTvShowEpisode>>
    {
        public GetPlexTvShowEpisodeByIdQuery(int id, bool includePlexTvShow = false)
        {
            Id = id;
            IncludePlexTvShow = includePlexTvShow;
        }

        public int Id { get; }

        public bool IncludePlexTvShow { get; }
    }
}
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexTvShows
{
    public class GetPlexTvShowEpisodeByIdQuery : IRequest<Result<PlexTvShowEpisode>>
    {
        public GetPlexTvShowEpisodeByIdQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }
}
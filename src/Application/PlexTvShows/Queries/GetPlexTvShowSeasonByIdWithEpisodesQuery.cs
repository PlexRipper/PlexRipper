using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexTvShows.Queries
{
    public class GetPlexTvShowSeasonByIdWithEpisodesQuery : IRequest<Result<PlexTvShowSeason>>
    {
        public GetPlexTvShowSeasonByIdWithEpisodesQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }
}
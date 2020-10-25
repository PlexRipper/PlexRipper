using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexTvShows.Queries
{
    public class GetPlexTvShowByIdWithEpisodesQuery : IRequest<Result<PlexTvShow>>
    {
        public GetPlexTvShowByIdWithEpisodesQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }
}
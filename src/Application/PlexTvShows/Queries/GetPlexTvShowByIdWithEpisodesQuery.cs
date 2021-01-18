using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexTvShows
{
    public class GetPlexTvShowByIdWithEpisodesQuery : IRequest<Result<PlexTvShow>>
    {
        public GetPlexTvShowByIdWithEpisodesQuery(int id, bool includeLibrary = false)
        {
            Id = id;
            IncludeLibrary = includeLibrary;
        }

        public int Id { get; }

        public bool IncludeLibrary { get; }
    }
}
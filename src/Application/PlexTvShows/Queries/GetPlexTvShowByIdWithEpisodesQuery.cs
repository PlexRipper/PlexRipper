using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public class GetPlexTvShowByIdWithEpisodesQuery : IRequest<Result<PlexTvShow>>
    {
        public GetPlexTvShowByIdWithEpisodesQuery(int id, bool includeLibrary = false, bool includeData = false)
        {
            Id = id;
            IncludeLibrary = includeLibrary;
            IncludeData = includeData;
        }

        public int Id { get; }

        public bool IncludeLibrary { get; }

        public bool IncludeData { get; }
    }
}
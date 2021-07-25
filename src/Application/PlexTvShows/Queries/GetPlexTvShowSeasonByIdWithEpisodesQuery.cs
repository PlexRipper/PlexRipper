using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexTvShows
{
    public class GetPlexTvShowSeasonByIdWithEpisodesQuery : IRequest<Result<PlexTvShowSeason>>
    {
        public GetPlexTvShowSeasonByIdWithEpisodesQuery(int id, bool includeLibrary = false, bool includeServer = false)
        {
            Id = id;
            IncludeLibrary = includeLibrary;
            IncludeServer = includeServer;
        }

        public int Id { get; }

        public bool IncludeLibrary { get; }

        public bool IncludeServer { get; }
    }
}
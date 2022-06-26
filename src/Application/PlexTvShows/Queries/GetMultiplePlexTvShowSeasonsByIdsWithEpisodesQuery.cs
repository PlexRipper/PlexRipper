using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public class GetMultiplePlexTvShowSeasonsByIdsWithEpisodesQuery : IRequest<Result<List<PlexTvShowSeason>>>
    {
        public GetMultiplePlexTvShowSeasonsByIdsWithEpisodesQuery(List<int> ids, bool includeData = false,  bool includeLibrary = false, bool includeServer = false)
        {
            Ids = ids;
            IncludeData = includeData;
            IncludeLibrary = includeLibrary;
            IncludeServer = includeServer;
        }

        public List<int> Ids { get; }

        public bool IncludeData { get; }

        public bool IncludeLibrary { get; }

        public bool IncludeServer { get; }
    }
}
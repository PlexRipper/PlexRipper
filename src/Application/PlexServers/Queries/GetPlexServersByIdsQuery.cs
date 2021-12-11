using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public class GetPlexServersByIdsQuery : IRequest<Result<List<PlexServer>>>
    {
        public GetPlexServersByIdsQuery(List<int> ids, bool includeLibraries = false)
        {
            Ids = ids;
            IncludeLibraries = includeLibraries;
        }

        public List<int> Ids { get; }

        public bool IncludeLibraries { get; }
    }
}
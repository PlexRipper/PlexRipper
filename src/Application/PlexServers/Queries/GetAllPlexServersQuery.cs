using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexServers
{
    public class GetAllPlexServersQuery : IRequest<Result<List<PlexServer>>>
    {
        public GetAllPlexServersQuery(bool includeLibraries = false)
        {
            IncludeLibraries = includeLibraries;
        }

        public bool IncludeLibraries { get; }
    }
}
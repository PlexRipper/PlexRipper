using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexServers
{
    public class GetPlexServerByIdQuery : IRequest<Result<PlexServer>>
    {
        public GetPlexServerByIdQuery(int id, bool includeLibraries = false)
        {
            Id = id;
            IncludeLibraries = includeLibraries;
        }

        public int Id { get; }

        public bool IncludeLibraries { get; }
    }
}
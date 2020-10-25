using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexServers.Queries
{
    public class GetPlexServersByPlexAccountIdQuery : IRequest<Result<List<PlexServer>>>
    {
        public GetPlexServersByPlexAccountIdQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }
}
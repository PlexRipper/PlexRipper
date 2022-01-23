using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public class GetAllPlexServersByPlexAccountIdQuery : IRequest<Result<List<PlexServer>>>
    {
        public GetAllPlexServersByPlexAccountIdQuery(int plexAccountId)
        {
            PlexAccountId = plexAccountId;
        }

        public int PlexAccountId { get; }
    }
}
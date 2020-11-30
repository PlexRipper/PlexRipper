using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexServers
{
    public class GetAllPlexServersQuery : IRequest<Result<List<PlexServer>>>
    {
        /// <summary>
        /// Retrieves all the PlexServers currently in the database.
        /// </summary>
        /// <param name="plexAccountId">Only retrieve <see cref="PlexServer"/>s accessible by this <see cref="PlexAccount"/>.</param>
        public GetAllPlexServersQuery(int plexAccountId = 0)
        {
            PlexAccountId = plexAccountId;
        }

        public int PlexAccountId { get; }
    }
}
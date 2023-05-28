using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class GetPlexServerTokenQuery : IRequest<Result<string>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlexServerTokenQuery"/> class.
    /// Retrieves an <see cref="PlexServer"/> authentication token associated with a <see cref="PlexAccount"/>.
    /// Note: An PlexAccountId of 0 can be passed to automatically retrieve first a non-main account token, and if not found a main account server token.
    /// </summary>
    /// <param name="plexServerId">The <see cref="PlexServer"/> to retrieve an authToken for.</param>
    /// <param name="plexAccountId"> An PlexAccountId of 0 can be passed to automatically retrieve first a non-main account token, and if not found a main account server token.</param>
    public GetPlexServerTokenQuery(int plexServerId, int plexAccountId = 0)
    {
        PlexServerId = plexServerId;
        PlexAccountId = plexAccountId;
    }

    public int PlexAccountId { get; }

    public int PlexServerId { get; }
}
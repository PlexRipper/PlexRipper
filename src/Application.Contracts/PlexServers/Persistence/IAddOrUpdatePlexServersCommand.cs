using FluentResults;
using PlexRipper.Domain;

namespace Application.Contracts;

public interface IAddOrUpdatePlexServersCommand
{
    Task<Result> ExecuteAsync(List<PlexServer> plexServers, CancellationToken cancellationToken = default);
}
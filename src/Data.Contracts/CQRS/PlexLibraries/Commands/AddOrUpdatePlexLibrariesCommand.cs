using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class AddOrUpdatePlexLibrariesCommand : IRequest<Result>
{
    public int PlexAccountId { get; }

    public List<PlexLibrary> PlexLibraries { get; }

    public AddOrUpdatePlexLibrariesCommand(int plexAccountId, List<PlexLibrary> plexLibraries)
    {
        PlexAccountId = plexAccountId;
        PlexLibraries = plexLibraries;
    }
}
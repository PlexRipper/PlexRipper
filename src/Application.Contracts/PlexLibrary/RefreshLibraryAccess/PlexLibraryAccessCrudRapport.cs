using PlexRipper.Domain;

namespace Application.Contracts;

public record PlexLibraryAccessCrudRapport(int PlexServerId, string PlexAccountName, string PlexServerName)
{
    public List<int> Created { get; init; } = [];
    public List<int> Updated { get; init; } = [];
    public List<int> Deleted { get; init; } = [];

    public override string ToString() =>
        $@"
        Plex library access rapport for account: {PlexAccountName} on server: {PlexServerName}
        Gained {nameof(PlexLibrary)} Access: {Created.ToListString()}
        Maintained {nameof(PlexLibrary)} Access: {Updated.ToListString()}
        Lost {nameof(PlexLibrary)} Access: {Deleted.ToListString()}";
}

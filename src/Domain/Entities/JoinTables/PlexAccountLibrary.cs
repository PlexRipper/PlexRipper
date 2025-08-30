namespace Reaparr.Domain;

/// <summary>
/// This is a join table entity that will return the specific libraries the PlexAccount has access to.
/// Cases may happen where multiple PlexAccounts might have access to the same <see cref="PlexServer"/> but not the same <see cref="PlexLibrary"/>.
/// </summary>
public class PlexAccountLibrary
{
    [Column(Order = 0)]
    public required int PlexAccountId { get; init; }

    public PlexAccount? PlexAccount { get; init; }

    [Column(Order = 1)]
    public required int PlexLibraryId { get; init; }

    public PlexLibrary? PlexLibrary { get; init; }

    [Column(Order = 2)]
    public required int PlexServerId { get; init; }

    public PlexServer? PlexServer { get; init; }

    [Column(Order = 4)]
    public required bool IsLibraryOwned { get; set; }
}

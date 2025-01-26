namespace Application.Contracts;

public record PlexServerAccessRapport(string _plexAccountName)
{
    private readonly string _plexAccountName = _plexAccountName;

    public List<int> Created { get; } = [];

    public List<int> Updated { get; } = [];

    public List<int> Deleted { get; } = [];

    public override string ToString() =>
        $@"
        Plex Server Access Rapport for account: {_plexAccountName}
        Created Access: {Created}
        Updated Access: {Updated}
        Updated Access: {Deleted}";
}

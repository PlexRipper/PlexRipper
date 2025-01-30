using System.Diagnostics.CodeAnalysis;
using System.Text;
using PlexRipper.Domain;

namespace Application.Contracts;

public record PlexLibraryAccessCrudRapport(string _plexAccountName, int PlexServerId, string _plexServerName)
{
    private readonly string _plexAccountName = _plexAccountName;
    private readonly string _plexServerName = _plexServerName;

    public int PlexServerId { get; } = PlexServerId;

    public List<PlexLibraryAccessRow> Data { get; set; } = new();

    public List<PlexLibraryAccessRow> GetGranted => Data.FindAll(x => x.State == PlexAccessState.Granted);

    public List<PlexLibraryAccessRow> GetUpdated => Data.FindAll(x => x.State == PlexAccessState.Updated);

    public List<PlexLibraryAccessRow> GetRevoked => Data.FindAll(x => x.State == PlexAccessState.Revoked);

    public void AddGranted(int plexLibraryId, string plexLibraryName)
    {
        Data.Add(new PlexLibraryAccessRow(PlexAccessState.Granted, PlexServerId, plexLibraryId, plexLibraryName));
    }

    public void AddUpdated(int plexLibraryId, string plexLibraryName)
    {
        Data.Add(new PlexLibraryAccessRow(PlexAccessState.Updated, PlexServerId, plexLibraryId, plexLibraryName));
    }

    public void AddRevoked(int plexLibraryId, string plexLibraryName)
    {
        Data.Add(new PlexLibraryAccessRow(PlexAccessState.Revoked, PlexServerId, plexLibraryId, plexLibraryName));
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Plex Library Access Rapport for account: {_plexAccountName} on server: {_plexServerName}");

        foreach (var state in Enum.GetValues<PlexAccessState>())
        {
            sb.AppendLine($"{state} Access:");
            var stateResults = Data.FindAll(x => x.State == state);
            if (stateResults.Any())
            {
                foreach (var result in stateResults)
                {
                    sb.AppendLine($" - {result.PlexLibraryName} (ID: {result.PlexLibraryId})");
                }
            }
            else
            {
                sb.AppendLine("No Changes");
            }
        }

        return sb.ToString();
    }
}

public record PlexLibraryAccessRow
{
    [SetsRequiredMembers]
    public PlexLibraryAccessRow(PlexAccessState state, int plexServerId, int plexLibraryId, string plexLibraryName)
    {
        State = state;
        PlexServerId = plexServerId;
        PlexLibraryId = plexLibraryId;
        PlexLibraryName = plexLibraryName;
    }

    public required int PlexServerId { get; set; }

    public required int PlexLibraryId { get; set; }

    public required string PlexLibraryName { get; set; }

    public required PlexAccessState State { get; set; }
}

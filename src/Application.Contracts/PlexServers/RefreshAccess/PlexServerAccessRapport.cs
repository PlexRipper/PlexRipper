using System.Diagnostics.CodeAnalysis;
using System.Text;
using PlexRipper.Domain;

namespace Application.Contracts;

public record PlexServerAccessRapport(string _plexAccountName)
{
    private readonly string _plexAccountName = _plexAccountName;

    public List<PlexServerAccessRow> Data { get; set; } = new();

    public void AddGranted(int plexServerId, string plexServerName)
    {
        Data.Add(new(PlexAccessState.Granted, plexServerId, plexServerName));
    }

    public void AddUpdated(int plexServerId, string plexServerName)
    {
        Data.Add(new(PlexAccessState.Updated, plexServerId, plexServerName));
    }

    public void AddRevoked(int plexServerId, string plexServerName)
    {
        Data.Add(new(PlexAccessState.Revoked, plexServerId, plexServerName));
    }

    public override string ToString()
    {
        var x = new StringBuilder();

        x.Append($"Plex Server Access Rapport for account: {_plexAccountName} \n");

        foreach (var state in Enum.GetValues<PlexAccessState>())
        {
            x.Append($"{state} Access:\n");
            var stateResults = Data.FindAll(x => x.State == state);
            if (stateResults.Any())
            {
                foreach (var result in stateResults)
                    x.Append($" - {result.PlexServerName} with id: {result.PlexServerId}");
            }
            else
            {
                x.Append("No Changes");
            }
        }

        return x.ToString();
    }
}

public record PlexServerAccessRow
{
    [SetsRequiredMembers]
    public PlexServerAccessRow(PlexAccessState state, int plexServerId, string plexServerName)
    {
        State = state;
        PlexServerId = plexServerId;
        PlexServerName = plexServerName;
    }

    public required int PlexServerId { get; set; }

    public required string PlexServerName { get; set; }

    public required PlexAccessState State { get; set; }
}

using System.Text;

namespace Reaparr.Application;

public record RefreshPlexServerAccessRapport(int PlexAccountId, string PlexAccountName)
{
    public List<RefreshPlexServerAccessRapportRow> Access { get; set; } = [];

    public void AddGranted(int plexServerId, string plexServerName)
    {
        Access.Add(new(PlexAccessState.Granted, plexServerId, plexServerName));
    }

    public void AddUpdated(int plexServerId, string plexServerName)
    {
        Access.Add(new(PlexAccessState.Updated, plexServerId, plexServerName));
    }

    public void AddRevoked(int plexServerId, string plexServerName)
    {
        Access.Add(new(PlexAccessState.Revoked, plexServerId, plexServerName));
    }

    public override string ToString()
    {
        var x = new StringBuilder();

        x.Append($"Plex Server Access Rapport for account: {PlexAccountName} \n");

        foreach (
            var state in new List<PlexAccessState>
            {
                PlexAccessState.Granted,
                PlexAccessState.Revoked,
                PlexAccessState.Updated,
            }
        )
        {
            x.Append($"{state} Access:\n");
            var stateResults = Access.FindAll(y => y.State == state);
            if (stateResults.Any())
            {
                foreach (var result in stateResults)
                    x.Append($" - {result.PlexServerName} with id: {result.PlexServerId}");
            }
            else
            {
                x.Append(" - No Changes");
            }
        }

        return x.ToString();
    }
}

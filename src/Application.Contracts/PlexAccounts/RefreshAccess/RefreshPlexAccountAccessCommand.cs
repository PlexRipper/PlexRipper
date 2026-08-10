namespace Reaparr.Application.Contracts;

/// <summary>
/// Validates enabled Plex accounts and refreshes their accessible Plex servers and libraries.
/// Pass a Plex account id to refresh only that account, or zero to refresh all enabled accounts.
/// </summary>
public record RefreshPlexAccountAccessCommand(int PlexAccountId = 0)
    : ICommand<Result<List<RefreshPlexAccountAccessRapportDTO>>>;
